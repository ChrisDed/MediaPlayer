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
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _positionSliderDragging;
        private string _trackPath;

        public MainWindow()
        {
            InitializeComponent();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += new EventHandler(Timer_Tick);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_positionSliderDragging)
                PositionSlider.Value = MediaEle.Position.TotalMilliseconds;
        }

        private void MediaEle_MediaOpened(object sender, RoutedEventArgs e)
        {
            PositionSlider.Maximum = MediaEle.NaturalDuration.TimeSpan.TotalMilliseconds;
            SpeedSlider.Value = 1;
        }

        // Button Event Handlers

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistBox.Items.Count > 0)
                PlayPlaylist();
            else
                PlayTrack();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            MediaEle.Stop();
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            MediaEle.Pause();
        }

        // Slider event handlers

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaEle.Volume = VolumeSlider.Value;
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaEle.SpeedRatio = SpeedSlider.Value;
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_positionSliderDragging)
                MediaEle.Position = TimeSpan.FromMilliseconds(PositionSlider.Value);
        }

        // Slider Click event handlers

        private void PositionSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _positionSliderDragging = false;
            MediaEle.Play();
        }

        private void PositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _positionSliderDragging = true;
            MediaEle.Stop();
        }

        // Menu handlers

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> result;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "";
            dlg.DefaultExt = ".mp3";
            dlg.Filter = ".mp3|*.mp3|.mpg|*.mpg|.wmv|*.wmv|All files (*.*)|*.*";
            dlg.CheckFileExists = true;
            result = dlg.ShowDialog();
            if (result == true)
            {
                PlaylistBox.Items.Clear();
                PlaylistBox.Visibility = Visibility.Hidden;
                // Open document
                _trackPath = dlg.FileName;
                TrackLabel.Content = _trackPath;
                PlayTrack();
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            String folderpath = "";
            string[] files;
            // Note: Must add a reference to System.Windows.Forms
            // in Solution Explorer in order to have access to FolderBrowserDialog
            System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                folderpath = fd.SelectedPath;
            }
            if (folderpath != "")
            {
                PlaylistBox.Items.Clear();
                PlaylistBox.Visibility = Visibility.Visible;
                files = Directory.GetFiles(folderpath, "*.mp3");
                foreach (string file in files)
                {
                    PlaylistBox.Items.Add(file);
                }
                PlaylistBox.SelectedIndex = 0;
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PlayTrack()
        {
            bool validPath = true;
            FileInfo fi = null;
            Uri src;
            try
            {
                fi = new FileInfo(_trackPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                validPath = false;
            }
            if (validPath)
            {
                // check that the file actually exists
                if (!fi.Exists)
                {
                    MessageBox.Show("Cannot find " + _trackPath);
                }
                else
                {
                    src = new Uri(_trackPath);
                    MediaEle.Source = src;
                    // assign the defaults (from slider positions) when a track starts playing
                    SetSliderDefaults();
                }
            }
        }

        private void PlayPlaylist()
        {
            int selectedItemIndex = -1;
            if (PlaylistBox.Items.Count > 0)
            {
                selectedItemIndex = PlaylistBox.SelectedIndex;
                if (selectedItemIndex > -1)
                {
                    _trackPath = PlaylistBox.Items[selectedItemIndex].ToString();
                    TrackLabel.Content = _trackPath;
                    PlayTrack();
                }
            }
        }

        private void SetSliderDefaults()
        {
            // assign defaults (from slider positions) when a track starts playing
            MediaEle.SpeedRatio = SpeedSlider.Value;
            MediaEle.Volume = VolumeSlider.Value;
            MediaEle.Balance = BalanceSlider.Value;
            MediaEle.Play();
            _timer.Start();
        }
    }
}
