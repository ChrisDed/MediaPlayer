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

        public MainWindow()
        {
            InitializeComponent();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_positionSliderDragging)
                PositionSlider.Value = MediaEle.Position.TotalMilliseconds;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            MediaEle.Play();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            MediaEle.Stop();
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            MediaEle.Pause();
        }

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

        private void MediaEle_MediaOpened(object sender, RoutedEventArgs e)
        {
            PositionSlider.Maximum = MediaEle.NaturalDuration.TimeSpan.TotalMilliseconds;
            SpeedSlider.Value = 1;
        }

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
    }
}
