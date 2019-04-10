using System;
using System.Collections;
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
using System.Xml;
using Path = System.IO.Path;

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
        private string datafile = "default.playlist";

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

        // File processing

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

        private void Files_Drop(object sender, DragEventArgs e)
        {
            string[] trackpaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            foreach (string s in trackpaths)
            {
                if (IsValidTrack(s))
                {
                    PlaylistBox.Items.Add(s);
                }
            }
            if (PlaylistBox.Items.Count > 0)
            {
                PlaylistBox.Visibility = Visibility.Visible;
                PlaylistBox.SelectedIndex = 0;
                _trackPath = trackpaths[0];
                TrackLabel.Content = _trackPath;
            }
        }

        // Media Play Triggers

        private void OpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlNodeList xmlNodes;
            XmlNode pathNode;
            string track;
            bool xmlLoaded = true;
            try
            {
                xdoc.Load(datafile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading XML data");
                xmlLoaded = false;
            }
            if (xmlLoaded)
            {
                PlaylistBox.Items.Clear();
                PlaylistBox.Visibility = Visibility.Visible;
                xmlNodes = xdoc.SelectNodes("Playlist/track");
                foreach (XmlNode node in xmlNodes)
                {
                    pathNode = node.SelectSingleNode("path");
                    track = pathNode.InnerText;
                    PlaylistBox.Items.Add(track);
                }
                PlaylistBox.SelectedIndex = 0;
            }
        }

        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter xmlwr;

            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;
            xmlwr = XmlWriter.Create(datafile, settings);
            ArrayList albumData = PlaylistTracks();

            if (albumData.Count == 0)
            {
                MessageBox.Show("No tracks to save!", "Error");
            }
            else
            {
                xmlwr.WriteStartElement("Playlist");
                foreach (string s in albumData)
                {
                    xmlwr.WriteStartElement("track");
                    xmlwr.WriteElementString("path", s);
                    xmlwr.WriteEndElement();
                }
                xmlwr.WriteEndElement();
                xmlwr.Close();
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Media handlers

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

        // Media event listeners

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            int nextTrackIndex = -1;
            int numberOfTracks = -1;
            MediaEle.Stop();
            numberOfTracks = PlaylistBox.Items.Count;
            if (numberOfTracks > 0)
            {
                nextTrackIndex = PlaylistBox.SelectedIndex + 1;
                if (nextTrackIndex >= numberOfTracks)
                {
                    nextTrackIndex = 0;
                }
                PlaylistBox.SelectedIndex = nextTrackIndex;
                PlayPlaylist();
            }
        }

        private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // this fires if the media source can't be found or can't be played
            MessageBox.Show("Unable to play " + _trackPath + " [" + e.ErrorException.Message + "]");
        }

        // Utility methods

        private void SetSliderDefaults()
        {
            // assign defaults (from slider positions) when a track starts playing
            MediaEle.Volume = VolumeSlider.Value;
            MediaEle.Play();
            _timer.Start();
        }

        private ArrayList PlaylistTracks()
        {
            int i = 0;
            string trackname = "";
            int playListSize = PlaylistBox.Items.Count;
            ArrayList tracks = new ArrayList();
            if (playListSize > 0)
            {
                for (i = 0; i < playListSize; i++)
                {
                    trackname = PlaylistBox.Items[i].ToString();
                    tracks.Add(trackname);
                }
            }
            return tracks;
        }

        private bool IsValidTrack(string track)
        {
            return track.EndsWith(".mp3");
        }
    }
}
