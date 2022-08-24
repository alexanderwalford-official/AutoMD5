using System;
using System.Collections.Generic;
using System.IO;
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
        }

        void getchecks()
        {
            if (File.Exists(datafile))
            {
                // cont
                string[] files = Directory.GetFiles(@"c:\\ProgramData\AutoMD5\files\");

                foreach (string file in files)
                {
                    filelist.Items.Add(file);
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

        }
    }
}
