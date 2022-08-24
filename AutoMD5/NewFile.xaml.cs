using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using System.Net;

namespace AutoMD5
{
    public partial class NewFile : Window
    {
        string datafile = @"c:\\ProgramData\AutoMD5\files.txt";
        string newinstructorfile = "";
        OpenFileDialog openFileDialog = new OpenFileDialog();
        bool hasinstructor = false;

        public NewFile()
        {
            InitializeComponent();
        }

        public void DownloadFile()
        {
            try
            {
                // add an exclusive link paramter for rechecking?

                string filename = linkinputbox.Text.Replace("/", "{fs}").Replace("http", "").Replace(":", ""); // clean the string
                // first, we need to add the record to the files list file
                File.WriteAllText(datafile, File.ReadAllText(datafile) + "\n[m:" + md5previewtext.Content + "|i:" + newinstructorfile + "|f:" + filename + "|auto_download:" + AutoUpdateCheck.IsChecked + "|auto_exec:" + autoexeccheck.IsChecked + "]");

                // now download the file to /files

                using (var client = new WebClient())
                {
                    client.DownloadFile(linkinputbox.Text, @"c:\\ProgramData\AutoMD5\files\" + filename);
                }

                if (hasinstructor)
                {
                    // try to execute the instructor batch file
                    System.Diagnostics.Process.Start(openFileDialog.FileName);
                }

                // close window, it worked!
                this.Close();
            }
            catch
            {
                if (hasinstructor)
                {
                    warntext.Content = "ERROR: Could not add file. Check instructor.";
                }
                else
                {
                    warntext.Content = "ERROR: Issue with remote file.";
                }
                // could not download file
            }
        }

        private void addbutton_Copy_Click(object sender, RoutedEventArgs e)
        {
            // select instructor file
            openFileDialog.Filter = "Batch files (*.bat)|*.bat";
            if (openFileDialog.ShowDialog() == true)
            {
                instrcutorfiletext.Content = openFileDialog.FileName;
                newinstructorfile = openFileDialog.FileName;
                hasinstructor = true;
            }
        }

        private void link_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        public void checkcompat ()
        {
            string filename = linkinputbox.Text.Replace("/", "{fs}").Replace("http", "").Replace(":", ""); // clean the string
            try
            {
                checkremote.IsEnabled = false;
                // check if relevent files have been created
                if (!File.Exists(datafile))
                {
                    // create the required files
                    Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\");
                    Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\files\");
                    File.Create(datafile);
                }

                // create a temp working dir
                Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\tmp\");
                // download the file to a temp dir
                using (var client = new WebClient())
                {
                    client.DownloadFile(linkinputbox.Text, @"c:\\ProgramData\AutoMD5\tmp\" + filename);        
                }

                // check the md5
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(@"c:\\ProgramData\AutoMD5\tmp\" + filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        md5previewtext.Content = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }

                // enable the add button
                addbutton.IsEnabled = true;
            }
            catch (Exception e)
            {
                md5previewtext.Content = "ERROR: INVALID FILE URL PROVIDED.";
                addbutton.IsEnabled = false;
                Console.WriteLine(e.Message);
            }

            // clean up the temp folder
            if (Directory.Exists(@"c:\\ProgramData\AutoMD5\tmp\"))
            {
                try
                {
                    File.Delete(@"c:\\ProgramData\AutoMD5\tmp\" + filename);
                    Directory.Delete(@"c:\\ProgramData\AutoMD5\tmp\");
                }
                catch
                {
                }
            }
            checkremote.IsEnabled = true;
        }

        private void addbutton_Click(object sender, RoutedEventArgs e)
        {
            // add file
            DownloadFile();
        }

        private void checkremote_Click(object sender, RoutedEventArgs e)
        {
            checkcompat();
        }
    }
}
