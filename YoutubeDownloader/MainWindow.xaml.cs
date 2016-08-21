using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                txtDestinationURL.Text = fbd.SelectedPath;
            }
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtDestinationURL.Text) && !string.IsNullOrEmpty(txtSourceURL.Text))
                {
                    if (rbVideo.IsChecked != null && rbWAV.IsChecked != null)
                    {
                        if (rbVideo.IsChecked.Value == true || rbWAV.IsChecked.Value == true)
                        {
                            if (rbVideo.IsChecked.Value == true)
                            {
                                lblResult.Text = "Downloading. Please wait";
                                Task<string> resultText = YoutubeDownloadPerformer.DownloadVideo(txtSourceURL.Text, txtDestinationURL.Text);
                                lblResult.Text = await resultText;
                            }
                            else
                            {
                                lblResult.Text = "Downloading. Please wait";
                                Task<string> resultText = YoutubeDownloadPerformer.DownloadAudio(txtSourceURL.Text, txtDestinationURL.Text);
                                lblResult.Text = await resultText;
                            }
                        }
                        else
                        {
                            SetInformativeErrorMessage();
                        }
                    }
                    else
                    {
                        SetInformativeErrorMessage();
                    }
                }
                else
                {
                    SetInformativeErrorMessage();
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "An error has occurred. Please try again";
            }
        }

        private void SetInformativeErrorMessage()
        {
            lblResult.Text = "Please provide an source URL and an destination URL and select in which format you want the Youtube video to be downloaded.";
        }
    }
}