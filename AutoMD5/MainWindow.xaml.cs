using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AutoMD5
{
    public partial class MainWindow : Window
    {
        string datafile = @"c:\\ProgramData\AutoMD5\files.txt";
        bool isupdated = false;
        List<string> files = new List<string>();
        public bool passback = false;
        int selected;

        public MainWindow()
        {
            InitializeComponent();
            getchecks();
        }

        void DarkMode()
        {
            this.Background = System.Windows.Media.Brushes.Black;
            appname.Foreground = System.Windows.Media.Brushes.White;
            title.Foreground = System.Windows.Media.Brushes.White;
            file_md5value.Foreground = System.Windows.Media.Brushes.White;
            statustext.Foreground = System.Windows.Media.Brushes.White;
            filelist.Background = System.Windows.Media.Brushes.DarkGray;
            filelist.Foreground = System.Windows.Media.Brushes.White;
            instructortext.Background = System.Windows.Media.Brushes.DarkGray;
            instructortext.Foreground = System.Windows.Media.Brushes.White;
        }
        void LightMode()
        {
            this.Background = System.Windows.Media.Brushes.White;
            appname.Foreground = System.Windows.Media.Brushes.Black;
            title.Foreground = System.Windows.Media.Brushes.Black;
            file_md5value.Foreground = System.Windows.Media.Brushes.Black;
            statustext.Foreground = System.Windows.Media.Brushes.Black;
            filelist.Background = System.Windows.Media.Brushes.White;
            filelist.Foreground = System.Windows.Media.Brushes.Black;
            instructortext.Background = System.Windows.Media.Brushes.White;
            instructortext.Foreground = System.Windows.Media.Brushes.Black;
        }

        public void getchecks()
        {
            filelist.SelectedItem = selected;
            // reset if passback, only an attempt for fixing this bug
            if (passback)
            {
                files.Clear();
                passback = false;
            }

            try
            {
                if (File.Exists(datafile))
                {
                    // cont
                    string s = File.ReadAllText(datafile);
                    string[] lines = s.Split('[');
                    filelist.Items.Clear();

                    int i = 0; // localised counter

                    var gridView = new GridView(); // new gridview obj
                    this.filelist.View = gridView;

                    // add the culumns
                    gridView.Columns.Add(new GridViewColumn
                    {
                        Header = "#",
                        DisplayMemberBinding = new Binding("count")
                    });
                    gridView.Columns.Add(new GridViewColumn
                    {
                        Header = "Latest?",
                        DisplayMemberBinding = new Binding("IsUpdated")
                    });
                    gridView.Columns.Add(new GridViewColumn
                    {
                        Header = "File Name",
                        DisplayMemberBinding = new Binding("filename")
                    });
                    gridView.Columns.Add(new GridViewColumn
                    {
                        Header = "SSL?",
                        DisplayMemberBinding = new Binding("IsSSL")
                    });
                    gridView.Columns.Add(new GridViewColumn
                    {
                        Header = "Raw ID",
                        DisplayMemberBinding = new Binding("id")
                    });

                    foreach (string f in lines)
                    {

                        if (f != "\n")
                        {
                            string s1 = f.Replace("[", "").Replace("]", ""); // m:###################|i:instructor.bat|f:file.file|auto_download:1|auto_exec:1

                            string[] s1s = s1.Split('|'); // split each propery into it an array

                            string s1_url = s1s[2].Replace("f:", "");
                            string url;

                            if (s1_url.StartsWith("s"))
                            {
                                // ssl mode
                                s1_url = s1_url.Remove(0, 1); // remove s so we can add it back to the correct part
                                url = s1_url = "https:" + s1_url;
                            }
                            else
                            {
                                url = s1_url = "http:" + s1_url;
                            }

                            string url_final = url.Replace("{fs}", "/");
                            string[] urlsplittitle = url_final.Split('/');
                            string icon = "";

                            if (isupdated)
                            {
                                icon = "✔️";
                            }
                            else
                            {
                                icon = "❌";
                            }

                            // replace %20 with spaces
                            if (url_final.Contains("%20"))
                            {
                                filelist.Items.Add(new ListItem { IsUpdated = icon, count = i, id = s1s[2].Replace("f:", ""), IsSSL = "✔️", filename = urlsplittitle[urlsplittitle.Length - 1].Replace("%20", " ") });
                            }
                            else
                            {
                                filelist.Items.Add(new ListItem { IsUpdated = icon, count = i, id = s1s[2].Replace("f:", ""), IsSSL = "❌", filename = urlsplittitle[urlsplittitle.Length - 1] });
                            }
                            files.Add(s1s[2].Replace("f:", ""));
                            i++; 
                        }
                        filelist.SelectedItem = selected;
                    }
                }
                else
                {
                    // create the required files
                    Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\");
                    Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\files\");
                    File.Create(datafile);

                    // show welcome win
                    var win = new Welcome();
                    win.Show();
                }
            }
            catch
            {
            }
        }
        bool isdoingsomething;
        void getdata(string input)
        {
            if (int.Parse(input) < files.Count())
            {
                string target = files[int.Parse(input)];

                try
                {
                    string s = File.ReadAllText(datafile);
                    string[] lines = s.Split('[');

                    foreach (string f in lines)
                    {
                        if (f.Contains(target) && !isdoingsomething)
                        {
                            isdoingsomething = true;
                            checkupdates.IsEnabled = false;
                            string filename;
                            string[] s1s;

                            string s1 = f.Replace("[", "").Replace("]", ""); // m:###################|i:instructor.bat|f:file.file|auto_download:1|auto_exec:1
                            s1s = s1.Split('|'); // split each propery into it an array

                            string s1_url = s1s[2].Replace("f:", "");
                            string url;

                            if (s1_url.StartsWith("s"))
                            {
                                // ssl mode
                                s1_url = s1_url.Remove(0, 1); // remove s so we can add it back to the correct part
                                url = s1_url = "https:" + s1_url;
                            }
                            else
                            {
                                url = s1_url = "http:" + s1_url;
                            }

                            string url_final = url.Replace("{fs}", "/");
                            string[] urlsplittitle = url_final.Split('/');

                            // replace %20 with space

                            if (url_final.Contains("%20"))
                            {
                                title.Content = urlsplittitle[urlsplittitle.Length - 1].Replace("%20", " ");
                            }
                            else
                            {
                                title.Content = urlsplittitle[urlsplittitle.Length - 1];
                            }

                            file_md5value.Content = s1s[0].Replace("[m:", ""); // set the old md5 value from the file

                            string instructorfilecontents = "NO DEFINED INSTRUCTIONS FILE";
                            try
                            {
                                instructorfilecontents = s1s[1].Replace("i:", "") + " (INSTRUCTOR FILE):\n" + File.ReadAllText(s1s[1].Replace("i:", ""));
                            }
                            catch
                            {
                                // no instructor file
                            }

                            instructortext.Text = url_final + ": \n" + s1s[3] + "\n" + s1s[4].Replace("]", "") + "\n\n" + instructorfilecontents;

                            // re-download and calculate new hash
                            // add back removed characters

                            filename = s1s[2].Replace("f:", "");

                            try
                            {
                                var client = new WebClient();

                                // create a temp working dir
                                Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\tmp\");

                                statustext.Content = "Checking hash..";
                                checkselectedbutton.IsEnabled = false;
                                removeselectedbutton.IsEnabled = false;
                                filelist.IsEnabled = false;

                                Uri newuri = new Uri(url_final);

                                Console.WriteLine("Downloading " + url_final);

                                // download the file to a temp dir
                                client.QueryString.Add("filename", filename);
                                client.QueryString.Add("s1", s1);
                                client.DownloadFileCompleted += client_DownloadFileCompleted;
                                client.DownloadProgressChanged += client_DownloadProgressChanged;
                                client.DownloadFileAsync(newuri, @"c:\\ProgramData\AutoMD5\tmp\" + filename);
                                client.Dispose();
                            }
                            catch (Exception e)
                            {
                                statustext.Content = e.Message;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    title.Content = "ERROR READING ENTRY";
                    file_md5value.Content = "";
                    statustext.Content = "⚠️ Error detected.";
                    instructortext.Text = e.Message;
                }
            }

        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            statustext.Content = "Checking hash.. (" + e.ProgressPercentage + "%)";
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string filename = ((System.Net.WebClient)(sender)).QueryString["filename"];
                string s1 = ((System.Net.WebClient)(sender)).QueryString["s1"];
                contop(filename, s1);
            }
            else
            {
                statustext.Content = e.Error.Message;
            }
        }

        // file completed download
        void contop (string filename, string s1) {

            // convert s1 back to s1s
            string[] s1s = s1.Split('|');

            // check the md5
            var md5 = MD5.Create();
            var stream = File.OpenRead(@"c:\\ProgramData\AutoMD5\tmp\" + filename);
            var hash = md5.ComputeHash(stream);
            string newmd5 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            stream.Close();

            string oldmd5 = "";
            file_md5value.Content = s1s[0].Replace("[m:", "");
            oldmd5 = file_md5value.Content.ToString().Replace("\n", "");

            // compare md5s
            if (newmd5 != oldmd5)
            {
                isupdated = false;
                // file is outdated! move new file to files folder and update md5 value
                statustext.Content = "⚠️ Outdated MD5";
                instructortext.Text = "MD5 MISMATCH! \nOLD: " + oldmd5 + "\nNEW: " + newmd5 + "\n\n" + instructortext.Text;

                // delete the old file, then copy the file out of temp into files folder
                File.Delete(@"c:\\ProgramData\AutoMD5\files\" + filename);
                File.Copy(@"c:\\ProgramData\AutoMD5\tmp\" + filename, @"c:\\ProgramData\AutoMD5\files\" + filename);

                // update only md5 in file
                string data = File.ReadAllText(datafile).Replace(oldmd5, newmd5);
                File.WriteAllText(datafile, data);

                try
                {
                    // execute instructor
                    System.Diagnostics.Process.Start(s1s[1].Replace("i:", ""));
                }
                catch
                {
                    statustext.Content = "⚠️ Issue with instructor file.";
                }

                statustext.Content = "ℹ️ File updated just now.";
                isupdated = true;
            }
            else
            {
                statustext.Content = "✔️ File is up to date.";
                isupdated = true;
            }

            // delete temp
            File.Delete(@"c:\\ProgramData\AutoMD5\tmp\" + filename);
            checkselectedbutton.IsEnabled = true;
            removeselectedbutton.IsEnabled = true;
            filelist.IsEnabled = true;
            isdoingsomething = false;
            checkupdates.IsEnabled = true;
            filelist.SelectedItem = selected;
            getchecks();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // new file
            var win = new NewFile();
            win.Owner = this;
            win.Show();
        }

        private void checkupdates_Click(object sender, RoutedEventArgs e)
        {
            checkupdates.IsEnabled = false;
            // recheck the MD5s
            getchecks();
            for(int i = 0; i < 49; i+=1)
            {
                selected = i;
                getdata(i.ToString());             
            }         
        }

        private void aboutbutton_Click(object sender, RoutedEventArgs e)
        {
            // credits
            var win = new info();
            win.Show();
        }

        private void filelist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected = filelist.SelectedIndex;
            object o = null;
            try
            {
                // menu item clicked
                o = filelist.SelectedIndex; // starts at 0

                if (o != null)
                {
                    getdata(o.ToString());
                }
            }
            catch
            {
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            object o = null;
            try
            {
                // menu item clicked
                o = filelist.SelectedIndex; // starts at 0

                if (o != null)
                {
                    getdata(o.ToString());
                }
            }
            catch
            {
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            object o = null;
            o = filelist.SelectedIndex;
            string s = o.ToString();

            // remove current entry from file

            string[] lines = File.ReadAllLines(datafile);
            string linetoremovedata = "";

            int i = 0;
            int linetoremove = 0;

            foreach (string l in lines)
            {
                if (l.Contains(s))
                {
                    linetoremove = i;
                    linetoremovedata = l;
                }
                i++;
            }

            string newdata = File.ReadAllText(datafile).Replace(linetoremovedata, "");
            File.WriteAllText(datafile, newdata);

            // now delete the file from /files
            string s1 = linetoremovedata.Replace("[", "").Replace("]", ""); // m:###################|i:instructor.bat|f:file.file|auto_download:1|auto_exec:1
            string[] s1s = s1.Split('|'); // split each propery into it an array
            string s1_url = s1s[2].Replace("f:", "");
            File.Delete(@"c:\\ProgramData\AutoMD5\files\" + s1_url);

            title.Content = "Select a file..";
            instructortext.Text = "";
            file_md5value.Content = "";
            statustext.Content = "";
            checkselectedbutton.IsEnabled = false;
            removeselectedbutton.IsEnabled = false;

            // update the list view
            getchecks();
        }

        private void lightmode_emoji_Click(object sender, RoutedEventArgs e)
        {
            LightMode();
        }

        private void darkmode_button_Click(object sender, RoutedEventArgs e)
        {
            DarkMode();
        }
    }
}
