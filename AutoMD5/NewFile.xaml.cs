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

        public NewFile()
        {
            InitializeComponent();
        }

        public void DownloadFile()
        {
            // first, we need to add the record to the files list file
            File.WriteAllText(datafile, File.ReadAllText(datafile) + "\n[m:" + md5previewtext.Content + "|i:" + newinstructorfile + "|f:" + linkinputbox.Text + "|auto_download:" + AutoUpdateCheck + "|auto_exec:" + autoexeccheck + "]");

            // now download the file to /files
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(linkinputbox.Text, @"c:\\ProgramData\AutoMD5\files\" + linkinputbox.Text);
                }
            }
            catch
            {
                // could not download file

            }
        }

        private void addbutton_Copy_Click(object sender, RoutedEventArgs e)
        {
            // select instructor file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Batch files (*.bat)|*.bat";
            if (openFileDialog.ShowDialog() == true)
            {
                instrcutorfiletext.Content = openFileDialog.FileName;
                newinstructorfile = openFileDialog.FileName;
            }
        }

        private void link_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(linkinputbox.Text))
                    {
                        md5previewtext.Content = stream;
                    }
                }
                addbutton.IsEnabled = true;
            }
            catch
            {
                md5previewtext.Content = "ERROR: INVALID FILE URL PROVIDED.";
                addbutton.IsEnabled = false;
            }

        }

        private void addbutton_Click(object sender, RoutedEventArgs e)
        {
            // add file
            DownloadFile();
        }
    }
}
