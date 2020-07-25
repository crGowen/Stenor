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
using System.IO;
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

        [DllImport("./stenorbe.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetImgBytesStorable(string path);

        [DllImport("./stenorbe.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetRequiredBytesForEncode(string path);

        [DllImport("./stenorbe.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int OutputFileToImg(string inputFile, string containerFile);

        [DllImport("./stenorbe.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int DecodeFromImg(string inputFile);

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

        int fileSize = 0;
        int containerStorableSize = 0;

        //Interop boundary issues are a maybe? so instead of std exceptions, let's just use ints
        private void ShowErr(int eCode)
        {
            switch (eCode)
            {
                case 5:
                    MessageBox.Show("Input file has no file extension, or the file extension is more than ten letters long.");
                    break;
                case 10:
                    MessageBox.Show("Invalid file type for container.");
                    break;
                case 20:
                    MessageBox.Show("Error occured whilst outputting data to image file. Likely issue is that the image is not a 24bit RGB .png file.");
                    break;
                case 30:
                    MessageBox.Show("Error occured whilst outputting data to wav file. Likely issue is that the wav file is not formatted as 16bit PCM with no FACT chunk.");
                    break;
                case 40:
                    MessageBox.Show("Error occured whilst converting input file to binary string.");
                    break;
                case 50:
                    MessageBox.Show("File chosen for decoding is neither a .wav nor a .png file.");
                    break;
                case 60:
                    MessageBox.Show("Error occured whilst extracting binary encoding from image. Image probably doesn't contain any stenor-encoded information.");
                    break;
                case 70:
                    MessageBox.Show("Error occured whilst extracting binary encoding from audio file.");
                    break;
                case 80:
                    MessageBox.Show("Error occured whilst coverting binary string to decoded file.");
                    break;
                case 90:
                    MessageBox.Show("Error occured getting image size. Ensure the container image is a 24bit RGB .png file.");
                    break;
                case 100:
                    MessageBox.Show("Error occured getting audio file size. Ensure the container wav file is formatted as 16bit PCM with no FACT chunk.");
                    break;
                case 110:
                    MessageBox.Show("Error occuring checking input file storage size requirement. Likely a formatting error with the specified file.");
                    break;
                default:
                    MessageBox.Show("An undefined error occured.");
                    break;
            }

        }

        public MainWindow()
        {
            InitializeComponent();
            ofd = new Microsoft.Win32.OpenFileDialog();
        }

        private string GetFileType(string inputFile)
        {
            string type = "";
            bool finished = false;
            for (int i = inputFile.Length - 1; i >= 0 && !finished; i--)
            {
                if (inputFile[i] == '.') finished = true;
                else type = inputFile[i] + type;
            }
            if (!finished) return ".";
            else return type;
        }

        private void FTBE_Click(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true) FTBEfield.Text = ofd.FileName;
        }

        private void EncodeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (containerStorableSize >= fileSize)
            {
                if (fileSize > 0)
                {
                    int result = OutputFileToImg(CleanFilePath(FTBEfield.Text), CleanFilePath(CIfield.Text));
                    if (result==0)
                    {
                        encMsg.Foreground = Brushes.Green;
                        encMsg.Text = "ENCODING COMPLETE.";
                    }
                    else
                    {
                        encMsg.Foreground = Brushes.Red;
                        encMsg.Text = "ENCODING FAILED.";
                        ShowErr(result);
                    }
                    
                }
                else
                {
                    encMsg.Foreground = Brushes.Red;
                    encMsg.Text = "Specified file is not valid or does not exist.";
                }                
            }
            else
            {
                encMsg.Foreground = Brushes.Red;
                encMsg.Text = "Container is NOT big enough! Will not encode.";
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
                if (FTBDfield.Text.Substring(FTBDfield.Text.Length - 4) == ".png" || FTBDfield.Text.Substring(FTBDfield.Text.Length - 4) == ".jpg")
                {
                    if (File.Exists(CleanFilePath(FTBDfield.Text)))
                    {
                       int result = DecodeFromImg(CleanFilePath(FTBDfield.Text));
                        if (result == 0)
                        {
                            decMsg.Foreground = Brushes.Green;
                            decMsg.Text = "DECODING COMPLETE.";
                        }
                        else
                        {
                            decMsg.Foreground = Brushes.Red;
                            decMsg.Text = "DECODING FAILED.";
                            ShowErr(result);
                        }
                        
                    }
                    else
                    {
                        decMsg.Foreground = Brushes.Red;
                        decMsg.Text = "Specified file does not exist.";
                    }
                        
                }
                else
                {
                    decMsg.Foreground = Brushes.Red;
                    decMsg.Text = "Select a .PNG or .JPG file to decode.";
                }
            }
            else
            {
                decMsg.Foreground = Brushes.Red;
                decMsg.Text = "Select a .PNG or .JPG file to decode.";
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
                if (CIfield.Text.Substring(CIfield.Text.Length - 4).ToLower() == ".png" || CIfield.Text.Substring(CIfield.Text.Length - 4).ToLower() == ".jpg")
                {
                    CIfield.Foreground = Brushes.Black;
                    warning_text2.Foreground = Brushes.Black;
                    containerStorableSize = GetImgBytesStorable(CleanFilePath(CIfield.Text));
                    if (containerStorableSize == 0) ShowErr(90);
                    warning_text2.Text = "This image can be used to store " + containerStorableSize + " bytes.";
                }
                else
                {
                    CIfield.Foreground = Brushes.Red;
                    warning_text2.Foreground = Brushes.Red;
                    warning_text2.Text = "You need to select a .PNG or .JPG file!";
                    containerStorableSize = 0;
                }
            }
            else if (CIfield.Text.Length > 0)
            {
                CIfield.Foreground = Brushes.Red;
                warning_text2.Foreground = Brushes.Red;
                warning_text2.Text = "You need to select a .PNG or .JPG file!";
                containerStorableSize = 0;
            }
            else
            {
                warning_text2.Text = "";
                containerStorableSize = 0;
            }

        }

        private void FTBEBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FTBEfield.Foreground = Brushes.Black;
            warning_text.Foreground = Brushes.Black;
            warning_text.Text = "";
            if (GetFileType(CleanFilePath(FTBEfield.Text))!="." && GetFileType(CleanFilePath(FTBEfield.Text))!="")
            {
                if(File.Exists(CleanFilePath(FTBEfield.Text)))
                {
                    fileSize = GetRequiredBytesForEncode(CleanFilePath(FTBEfield.Text));
                    if (fileSize == 0) ShowErr(110);
                    warning_text.Text = "Encoding this file will require a container that can store " + fileSize + " bytes.";
                }
                else
                {
                    fileSize = 0;
                    FTBEfield.Foreground = Brushes.Red;
                    warning_text.Foreground = Brushes.Red;
                    warning_text.Text = "The specified file does not exist.";
                }
                
            }
            else
            {
                fileSize = 0;
                FTBEfield.Foreground = Brushes.Red;
                warning_text.Foreground = Brushes.Red;
                warning_text.Text = "Select a valid file.";
            }
            encMsg.Text = "";
        }

        private void FTBDBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FTBDfield.Text.Length > 4)
            {
                if (FTBDfield.Text.Substring(FTBDfield.Text.Length - 4) == ".png" || FTBDfield.Text.Substring(FTBDfield.Text.Length - 4) == ".jpg")
                {
                    if (!File.Exists(CleanFilePath(FTBDfield.Text)))
                    {
                        FTBDfield.Foreground = Brushes.Red;
                        warning_text3.Foreground = Brushes.Red;
                        warning_text3.Text = "The specified file does not exist.";
                    }
                    else
                    {
                        FTBDfield.Foreground = Brushes.Black;
                        warning_text3.Foreground = Brushes.Black;
                        warning_text3.Text = "";
                    }
                }
                else
                {
                    FTBDfield.Foreground = Brushes.Red;
                    warning_text3.Foreground = Brushes.Red;
                    warning_text3.Text = "You need to select a .PNG or JPG file.";
                }
            }
            else if (FTBDfield.Text.Length > 0)
            {
                FTBDfield.Foreground = Brushes.Red;
                warning_text3.Foreground = Brushes.Red;
                warning_text3.Text = "You need to select a .PNG or JPG file.";
            }
            else warning_text3.Text = "";
            decMsg.Text = "";
        }
    }
}
