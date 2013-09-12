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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Runtime.InteropServices;

namespace AccXtract
{
    public partial class Form1 : Form
    {
        GroupBox lastGroup;
        string AccXtractFolder;
        public Form1()
        {
            InitializeComponent();
            string home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            lastGroup = localGroup;
            LoadLocalChrome(home);
        }

        private void LoadLocalChrome(string home)
        {
            string chromeDir = home +@"\AppData\Local\Google\Chrome\";
            if (Directory.Exists(chromeDir))
            {
                string line;

                System.IO.StreamReader file = new StreamReader(chromeDir + @"User Data\Local State");
                int currentNumberOfProfiles = 0;
                Button currentButton = (Button)chromeLocalPanel.Controls[chromeLocalPanel.Controls.Count - 1];
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("\"shortcut_name\""))
                    {

                        string[] values = line.Split('"');
                        Console.WriteLine(values[3]);

                        if (currentNumberOfProfiles != 0)
                        {
                            currentButton = addButtonToPanel(values[3], chromeLocalPanel);
                        }

                        else
                        {
                            if (values[3] != "") currentButton.Text = values[3];
                        }

                        //this will be changed later
                        //will add support for removing profiles
                        currentButton.Enabled = false;

                        currentNumberOfProfiles++;
                    }
                }

                file.Close();

                //Resize for the scroll bar
                if (chromeLocalPanel.HorizontalScroll.Visible) chromeLocalPanel.Size = new Size(chromeLocalPanel.Size.Width, chromeLocalPanel.Size.Height + 17);
            }

            else
            {
                chromeLocalPanel.Enabled = false;
            }
        }

        private void loadFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog diag = new FolderBrowserDialog();
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AccXtractFolder = diag.SelectedPath;
                string[] computers = Directory.GetDirectories(AccXtractFolder);
                foreach (string computer in computers)
                {
                    string[] components = computer.Split('\\');
                    string computerName = components[components.Length - 1];

                    
                    lastGroup = addNewComputer(computer);
                }
            }
        }

        private void addChromeProfile(object sender, EventArgs e)
        {
            Process[] chrome = Process.GetProcessesByName("chrome");
            if (chrome.Length == 0)
            {
                Button button = (Button)sender;
                string profileName = button.Text;

                string localStatePath = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + @"\AppData\Local\Google\Chrome\User Data\";
                StreamReader localState = new StreamReader(localStatePath + "\\Local State");
                StreamWriter output = new StreamWriter(localStatePath + "\\Local State.out");

                string line = "";
                bool foundProfiles = false;

                while (!foundProfiles && line != null)
                {
                    line = localState.ReadLine();
                    output.WriteLine(line);
                    if (line.Contains(@"""profile"": {"))
                    {
                        foundProfiles = true;
                    }
                }

                bool wroteNewProfile = false;
                int highestProfileNumber = 0;

                while (!wroteNewProfile && line != null)
                {
                    line = localState.ReadLine();

                    if (line.Contains(@"""Profile "))
                    {
                        string number = line[line.IndexOf('e') + 2].ToString();
                        highestProfileNumber = Convert.ToInt32(number);
                    }

                    if (line.Contains(@"}"))
                    {
                        if (!line.Contains(@","))
                        {
                            line += ",";
                            output.WriteLine(line);
                            highestProfileNumber++;

                            //WARNING
                            //do ***NOT*** modify spacing of the quoted parted in ANY WAY if you want it to keep working
                            //Chrome is very picky with this thing
                            output.Write(@"         ""Profile " + highestProfileNumber + @""": {
            ""avatar_icon"": ""chrome://theme/IDR_PROFILE_AVATAR_12"",
            ""background_apps"": false,
            ""managed_user_id"": """",
            ""name"": """ + profileName + @""",
            ""shortcut_name"": """ + profileName + @""",
            ""user_name"": """"
         }
");
                            wroteNewProfile = true;
                        }

                        else output.WriteLine(line);
                    }

                    else output.WriteLine(line);
                }

                while (line != null)
                {
                    line = localState.ReadLine();
                    output.WriteLine(line);
                }

                localState.Close();
                output.Close();

                GroupBox group = (GroupBox)button.Parent.Parent;
                string computerName = group.Text;

                string newUserProfile = localStatePath + "Profile " + highestProfileNumber.ToString();
                string capturedUserProfile = AccXtractFolder + "\\" + computerName + "\\Chrome\\" + button.Text;

                Directory.CreateDirectory(newUserProfile);

                //Add cookies to profile
                File.Copy(capturedUserProfile + "\\cookies", newUserProfile + "\\cookies", true);

                #region Add passwords
                //Add passwords to profile

                string[] data = File.ReadAllLines(capturedUserProfile + "\\passwords.txt");

                //connect to the extracted Login Data database
                SQLiteConnection conn = new SQLiteConnection("Data Source=" + capturedUserProfile + "\\Login Data");
                conn.Open();

                //Begin re-encryption
                List<string> passwords = new List<string>();
                for (int i = 0; i < data.Length; i++)
                {
                    //get every third object (passwords)
                    //I left the other stuff in there in case someone to manually read the passwords.txt file
                    //or do something else with it

                    if (((i + 1) % 3) == 0)
                    {
                        passwords.Add(data[i]);

                        string decryptedPassword = data[i];
                        byte[] decryptedPassBytes = Encoding.ASCII.GetBytes(decryptedPassword);
                        byte[] encryptedPassword = DPAPI.encryptBytes(decryptedPassBytes);
                        //byte[] binEncryptedPassword = System.Text.Encoding.ASCII.GetBytes(encryptedPassword);


                        SQLiteCommand replaceData = conn.CreateCommand();
                        replaceData.CommandText = "UPDATE logins SET password_value = @pass WHERE action_url = \"" + data[i - 2] + "\"";
                        SQLiteParameter param = new SQLiteParameter("@pass", DbType.Binary);
                        param.Value = encryptedPassword;
                        replaceData.Parameters.Add(param);
                        replaceData.ExecuteNonQuery();
                    }
                }

                conn.Close();

                //End re-encryption

                File.Copy(capturedUserProfile + "\\Login Data", newUserProfile + "\\Login Data", true);

                

                #endregion


                File.Copy(localStatePath + "\\Local State.out", localStatePath + "\\Local State", true);
                File.Delete(localStatePath + "Local State.out");

                button.Enabled = false;
            }

            else MessageBox.Show("Please close Chrome and try again");
        }

        private void addFirefoxProfile(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string profileName = button.Text;
            GroupBox group = (GroupBox)button.Parent.Parent;
            string computerName = group.Text;

            Process[] firefox = Process.GetProcessesByName("firefox");
            if (firefox.Length == 0){
                string firefoxPath = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\AppData\\Roaming\\Mozilla\\Firefox\\";
                if (Directory.Exists(firefoxPath))
                {
                    StreamReader ProfilesINI = new StreamReader(firefoxPath + "\\profiles.ini");
                    StreamWriter outProfilesINI = new StreamWriter(firefoxPath + "\\profiles.ini.out");

                    string line = ProfilesINI.ReadLine();
                    int profileNumber = 0;
                    while (line != null)
                    {
                        
                        if (line.Contains("StartWithLastProfile="))
                        {
                            line = line.Replace("1", "0");
                        }

                        else if (line.Contains("[Profile"))
                        {
                            //done in case of more than 9 profiles
                            string numberString = line.Substring(line.IndexOf('e') + 1, line.Length - line.IndexOf('e') - 2);

                            profileNumber = Convert.ToInt32(numberString);
                        }
                        outProfilesINI.WriteLine(line);
                        line = ProfilesINI.ReadLine();
                    }

                    if (Directory.Exists(firefoxPath + "\\Profiles\\AccXtract." + profileName)) profileName += profileNumber.ToString();
                    outProfilesINI.Write(@"
[Profile" + (profileNumber + 1) + @"]
Name=" + profileName + @"
IsRelative=1
Path=Profiles/AccXtract." + profileName);


                    ProfilesINI.Close();
                    outProfilesINI.Close();
                    File.Delete(firefoxPath + "\\profiles.ini");
                    File.Move(firefoxPath + "\\profiles.ini.out", firefoxPath + "\\profiles.ini");

                    string newProfile = firefoxPath + "\\Profiles\\AccXtract." + profileName;
                    Directory.CreateDirectory(newProfile);

                    string[] files = Directory.GetFiles(AccXtractFolder + "\\" + computerName + "\\Firefox\\" + button.Text);
                    string[] name = new string[0];
                    foreach (string file in files)
                    {
                        name = file.Split('\\');
                        File.Copy(file, newProfile + "\\" + name[name.Length - 1]);
                    }
                }

                else MessageBox.Show("Please install Firefox");


                button.Enabled = false;
            }

            else MessageBox.Show("Please close Firefox and try again");
        }

        #region UI Functions
        private GroupBox addNewComputer(string path)
        {
            GroupBox newGroup = new GroupBox();

            string[] components = path.Split('\\');
            string computerName = components[components.Length - 1];

            newGroup.Text = computerName;
            newGroup.AutoSize = true;
            newGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            newGroup.Tag = computerName + "Group";

            Point location = new Point(lastGroup.Location.X, lastGroup.Location.Y + lastGroup.Height + 4);
            newGroup.Location = location;

            this.Controls.Add(newGroup);

            Panel lastPanel = null;

            #region Chrome
            if (Directory.Exists(path + "\\Chrome"))
            {
                lastPanel = addPanel(@"Google Chrome (Click to add to Chrome)", lastPanel, newGroup);
                string[] profiles = Directory.GetDirectories(path + "\\Chrome");

                foreach (string profile in profiles)
                {
                    string[] components2 = profile.Split('\\');
                    string profileName = components2[components2.Length - 1];
                    Button newButton = addButtonToPanel(profileName, lastPanel);
                    newButton.Click += addChromeProfile;
                }
            }
            #endregion

            #region Firefox
            if (Directory.Exists(path + "\\Firefox"))
            {
                lastPanel = addPanel(@"Firefox (Click to add to FF)", lastPanel, newGroup);
                string[] profiles = Directory.GetDirectories(path + "\\Firefox");

                foreach (string profile in profiles)
                {
                    string[] components2 = profile.Split('\\');
                    string profileName = components2[components2.Length - 1];
                    Button newButton = addButtonToPanel(profileName, lastPanel);
                    newButton.Click += addFirefoxProfile;
                }
            }
            #endregion

            return newGroup;
        }

        private Panel addPanel(string title, Panel lastPanel, GroupBox group)
        {
            Panel newPanel = new Panel();
            Label newLabel = new Label();

            if (lastPanel != null)
            {
                newLabel.Location = new Point(7, lastPanel.Location.Y + lastPanel.Height + 12);
                newPanel.Location = new Point(10, newLabel.Location.Y + newLabel.Height + 3);
            }

            else
            {
                newLabel.Location = new Point(7, 20);
                newPanel.Location = new Point(10, 37);
            }

            newLabel.Text = title;
            newLabel.AutoSize = true;
            
            newPanel.AutoScroll = true;
            newPanel.Size = chromeLocalPanel.Size;
            newPanel.Tag = title + "Panel";

            group.Controls.Add(newLabel);
            group.Controls.Add(newPanel);

            return newPanel;
        }

        private Button addButtonToPanel(string title, Panel panel)
        {
            Button newButton = new Button();

            newButton.Text = title;
            newButton.AutoSize = true;
            newButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            if (panel.Controls.Count != 0)
            {
                Button oldButton = (Button)panel.Controls[panel.Controls.Count - 1];
                newButton.Location = new Point(oldButton.Location.X + oldButton.Width + 3, 5);
            }

            else
            {
                newButton.Location = new Point(4, 5);
            }

            panel.Controls.Add(newButton);

            if (panel.HorizontalScroll.Visible) panel.Size = new Size(panel.Size.Width, panel.Size.Height + 7);

            return newButton;
        }

        #endregion
    }
}
