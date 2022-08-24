using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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

namespace AutoMD5
{
    public partial class MainWindow : Window
    {
        string datafile = @"c:\\ProgramData\AutoMD5\files.txt";

        public MainWindow()
        {
            InitializeComponent();
            getchecks();
        }

        void getchecks()
        {
            try
            {
                if (File.Exists(datafile))
                {
                    // cont
                    string s = File.ReadAllText(datafile);
                    string[] lines = s.Split('[');
                    filelist.Items.Clear();

                    foreach (string f in lines)
                    {
                        string s1 = f.Replace("[", "").Replace("]", ""); // m:###################|i:instructor.bat|f:file.file|auto_download:1|auto_exec:1
                        string[] s1s = s.Split('|'); // split each propery into it an array

                        filelist.Items.Add(s1s[2].Replace("f:", "")); // add file name
                    }

                }
                else
                {
                    // create the required files
                    Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\");
                    Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\files\");
                    File.Create(datafile);
                }
            }
            catch
            {
            }

        }

        void getdata(string input)
        {
            try
            {
                string s = File.ReadAllText(datafile);               
                string[] lines = s.Split('[');

                foreach (string f in lines)
                {
                    if (f.Contains(input))
                    {
                        string s1 = f.Replace("[", "").Replace("]", ""); // m:###################|i:instructor.bat|f:file.file|auto_download:1|auto_exec:1
                        string[] s1s = s.Split('|'); // split each propery into it an array

                        title.Content = s1s[2].Replace("f:", ""); // set the title to the file's name
                        file_md5value.Content = s1s[0].Replace("[m:", ""); // set the md5 title
                        string instructorfilecontents = "NO DEFINED INSTRUCTIONS FILE";
                        try
                        {
                            instructorfilecontents = s1s[1].Replace("i:", "") + " (INSTRUCTOR FILE):\n" + File.ReadAllText(s1s[1].Replace("i:", ""));
                        }
                        catch
                        {
                            // no instructor file
                        }
                        instructortext.Text = s1s[2].Replace("f:", "") + ": \n" + s1s[3] + "\n" + s1s[4].Replace("]","") + "\n\n" + instructorfilecontents;

                        // re-download and calculate new hash
                        // add back removed characters
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
                        
                        string url_final = url.Replace("_fs_", "/");
                        Console.WriteLine(url_final);

                        string newmd5 = "";
                        string filename = s1s[2].Replace("f:", "");

                        try
                        {
                            // create a temp working dir
                            Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\tmp\");

                            // download the file to a temp dir
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(url_final, @"c:\\ProgramData\AutoMD5\tmp\" + filename);
                            }

                            // check the md5
                            using (var md5 = MD5.Create())
                            {
                                using (var stream = File.OpenRead(@"c:\\ProgramData\AutoMD5\tmp\" + filename))
                                {
                                    var hash = md5.ComputeHash(stream);
                                    newmd5 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                                }
                            }
                            string oldmd5 = "";
                            file_md5value.Content = "";
                            oldmd5 = file_md5value.Content.ToString();
                            // compare md5s
                            if (newmd5 != oldmd5)
                            {
                                // file is outdated! move new file to files folder and update md5 value
                                statustext.Content = "Outdated MD5.";

                                // copy the file out of temp into files folder
                                File.Copy(@"c:\\ProgramData\AutoMD5\tmp\" + filename, @"c:\\ProgramData\AutoMD5\files\" + filename);

                                // update only md5 in file
                                string data = File.ReadAllText(datafile).Replace(oldmd5, newmd5);
                                File.WriteAllText(datafile, data);

                                statustext.Content = "File updated just now.";
                            }
                            else
                            {
                                statustext.Content = "File is up to date.";
                            }
                            try
                            {
                                // execute instructor
                                System.Diagnostics.Process.Start(s1s[1].Replace("i:", ""));
                            }
                            catch
                            {
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                title.Content = "ERROR READING ENTRY";
                file_md5value.Content = "?????????";
                instructortext.Text = e.Message;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // new file
            var win = new NewFile();
            win.Show();
        }

        private void checkupdates_Click(object sender, RoutedEventArgs e)
        {
            // recheck the MD5s
            getchecks();
        }

        private void aboutbutton_Click(object sender, RoutedEventArgs e)
        {
            // credits
            var win = new info();
            win.Show();
        }

        private void filelist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object o = null;
            try
            {
                // menu item clicked
                o = filelist.SelectedItem;

                if (o != null)
                {
                    getdata(o.ToString());
                }  
            }
            catch { }
        }
    }
}
