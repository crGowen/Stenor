using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Stenor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Microsoft.Win32.OpenFileDialog ofd;

        [DllImport("./func/StenorBackend.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetImgSize(string path);

        [DllImport("./func/StenorBackend.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetRequiredPixelsForEncode(string path);

        [DllImport("./func/StenorBackend.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int EncodeToContainer(string inputFile, string containerFile);

        [DllImport("./func/StenorBackend.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int ParseImage(string inputFile);

        private string CleanFilePath(string unclean)
        {
            string buildStr = "";
            foreach(char c in unclean)
            {
                if (c == '\\') buildStr = buildStr + "/";
                else buildStr = buildStr + c;
            }
            return buildStr;
        }

        int img1size = 0;
        int img2size = 0;

        public MainWindow()
        {
            InitializeComponent();
            ofd = new Microsoft.Win32.OpenFileDialog();
        }

        private void FTBE_Click(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true) FTBEfield.Text = ofd.FileName;
        }

        private void EncodeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (img2size >= img1size && img1size > 0)
            {
                EncodeToContainer(CleanFilePath(FTBEfield.Text), CleanFilePath(CIfield.Text));
                encMsg.Foreground = Brushes.Green;
                encMsg.Text = "ENCODING COMPLETE.";
            }
            else
            {
                encMsg.Foreground = Brushes.Red;
                encMsg.Text = "Container Image is NOT big enough! Will not encode.";
            }
        }

        private void CI_Click(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true) CIfield.Text = ofd.FileName;
        }

        private void DecodeBtn_Click(object sender, RoutedEventArgs e)
        { 
            if(FTBDfield.Text.Length > 4)
            {
                if (FTBDfield.Text.Substring(FTBDfield.Text.Length - 4) == ".png")
                {
                    ParseImage(CleanFilePath(FTBDfield.Text));
                    decMsg.Foreground = Brushes.Green;
                    decMsg.Text = "DECODING COMPLETE.";
                }
                else
                {
                    decMsg.Foreground = Brushes.Red;
                    decMsg.Text = "Select a .PNG file to decode.";
                }
            }
            else
            {
                decMsg.Foreground = Brushes.Red;
                decMsg.Text = "Select a .PNG file to decode.";
            }
            
        }

        private void FTBD_Click(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true) FTBDfield.Text = ofd.FileName;
        }

        private void CIBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CIfield.Text.Length > 4)
            {
                if (CIfield.Text.Substring(CIfield.Text.Length - 4) == ".png")
                {
                    CIfield.Foreground = Brushes.Black;
                    warning_text2.Foreground = Brushes.Black;
                    img2size = GetImgSize(CleanFilePath(CIfield.Text));
                    warning_text2.Text = "This container image is " + img2size + " pixels.";
                }
                else
                {
                    CIfield.Foreground = Brushes.Red;
                    warning_text2.Foreground = Brushes.Red;
                    warning_text2.Text = "You need to select a .PNG file!";
                }
            }
            else if (CIfield.Text.Length > 0)
            {
                CIfield.Foreground = Brushes.Red;
                warning_text2.Foreground = Brushes.Red;
                warning_text2.Text = "You need to select a .PNG file!";
            }
            else warning_text2.Text = "";

        }

        private void FTBEBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FTBEfield.Foreground = Brushes.Black;
            warning_text.Foreground = Brushes.Black;
            warning_text.Text = "";
            img1size = GetRequiredPixelsForEncode(CleanFilePath(FTBEfield.Text)) * 24 + 6 + 32;
            warning_text.Text = "Encoding this image will require a container image of at least " + img1size + " pixels.";
            encMsg.Text = "";
        }

        private void FTBDBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FTBDfield.Text.Length > 4)
            {
                if (FTBDfield.Text.Substring(FTBDfield.Text.Length - 4) == ".png")
                {
                    FTBDfield.Foreground = Brushes.Black;
                    warning_text3.Foreground = Brushes.Black;
                }
                else
                {
                    FTBDfield.Foreground = Brushes.Red;
                    warning_text3.Foreground = Brushes.Red;
                    warning_text3.Text = "You need to select a .PNG file!";
                }
            }
            else if (FTBDfield.Text.Length > 0)
            {
                FTBDfield.Foreground = Brushes.Red;
                warning_text3.Foreground = Brushes.Red;
                warning_text3.Text = "You need to select a .PNG file!";
            }
            else warning_text3.Text = "";
            decMsg.Text = "";
        }
    }
}
