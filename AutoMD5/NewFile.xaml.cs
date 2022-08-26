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
using System.ComponentModel;

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
                string filename = linkinputbox.Text.Replace("/", "{fs}").Replace("http", "").Replace(":", ""); // clean the string
                // first, we need to add the record to the files list file
                File.WriteAllText(datafile, File.ReadAllText(datafile) + "\n[m:" + md5previewtext.Content + "|i:" + newinstructorfile + "|f:" + filename + "|auto_download:" + AutoUpdateCheck.IsChecked + "|auto_exec:" + autoexeccheck.IsChecked + "]");

                // now copy file from tmp to files
                File.Copy(@"c:\\ProgramData\AutoMD5\tmp\" + filename, @"c:\\ProgramData\AutoMD5\files\" + filename);

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

                if (hasinstructor)
                {
                    // try to execute the instructor batch file
                    System.Diagnostics.Process.Start(openFileDialog.FileName);
                }

                // call getchecks in main window
                ((MainWindow)this.Owner).passback = true;
                ((MainWindow)this.Owner).getchecks();

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


                var client = new WebClient();

                // create a temp working dir
                Directory.CreateDirectory(@"c:\\ProgramData\AutoMD5\tmp\");

                md5previewtext.Content = "Checking hash..";

                Uri newuri = new Uri(linkinputbox.Text);

                // download the file to a temp dir
                client.QueryString.Add("filename", filename);
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileAsync(newuri, @"c:\\ProgramData\AutoMD5\tmp\" + filename);

            }
            catch (Exception e)
            {
                md5previewtext.Content = "ERROR: INVALID FILE URL PROVIDED.";
                addbutton.IsEnabled = false;
                Console.WriteLine(e.Message);
            }
            checkremote.IsEnabled = true;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            md5previewtext.Content = "Checking hash.. (" + e.ProgressPercentage + "%)";
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string filename = ((System.Net.WebClient)(sender)).QueryString["filename"];
                contop(filename);
            }
            else
            {
                md5previewtext.Content = e.Error.Message;
            }
        }

        void contop(string filename)
        {
            // move these..

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
