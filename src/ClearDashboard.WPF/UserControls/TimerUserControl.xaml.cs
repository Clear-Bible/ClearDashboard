using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignColors.Recommended;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;


namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for TimerUserControl.xaml
    /// </summary>
    public partial class TimerUserControl : UserControl
    {
        System.Timers.Timer _timer = new System.Timers.Timer(1000);
        
        public TimerUserControl()
        {
            InitializeComponent();
            _timerOn = false;
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
        }

        private int _secondsLeft;
        public int SecondsLeft
        {
            get { return _secondsLeft; }
            set
            {
                _secondsLeft = value;
            }
        }

        private bool _timerOn;
        public bool TimerOn
        {
            get { return _timerOn; }
            set
            {
                _timerOn = value;
            }
        }

        private TimeSpan _time;


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void StartStop_OnClick(object sender, RoutedEventArgs e)
        {
            TimerLabel.Foreground = Brushes.White;
            if (_timerOn)
            {
                //Set the symbol to play
                PauseIcon.Visibility = Visibility.Collapsed;
                PlayIcon.Visibility = Visibility.Visible;
                _timerOn = false;
                _timer.Stop();
            }
            else
            {
                //Set the symbol to pause
                PauseIcon.Visibility = Visibility.Visible;
                PlayIcon.Visibility = Visibility.Collapsed;
                _timerOn = true;
                _timer.Enabled = true;
            }

           
        }
        
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (_secondsLeft <= 0)
            {
                _secondsLeft--;
                //_timer.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    TimerBorder.Background = Brushes.Red;
                    StartStopButton.Background = Brushes.Salmon;

                    _time = TimeSpan.FromSeconds(_secondsLeft*-1);
                    TimerLabel.Content = (int)_time.TotalHours + "h " + _time.Minutes + "m " + _time.Seconds + "s";
                });
            }
            else
            {
                _secondsLeft--;

                this.Dispatcher.Invoke(() =>
                {
                    if (_secondsLeft <= 300)
                    {
                        TimerBorder.Background = Brushes.OrangeRed;
                        StartStopButton.Background = Brushes.DarkOrange;
                    }
                    else
                    {
                        TimerBorder.Background = Brushes.Transparent;
                        StartStopButton.Background = Brushes.DodgerBlue;
                    }

                    _time = TimeSpan.FromSeconds(_secondsLeft);
                    TimerLabel.Content = (int)_time.TotalHours+"h "+ _time.Minutes+"m "+ _time.Seconds+"s";
                });
            }

            
            
        }

        private void TimerTbx_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TimerLabel.Foreground = Brushes.LightGray;
            if ((TimerBox.CaretIndex != TimerBox.Text.Length) && (TimerBox.CaretIndex != 0))
            {
                // cut and paste to end of string
                string temp = TimerBox.Text.Substring(TimerBox.CaretIndex - 1, 1);
                TimerBox.Text = TimerBox.Text.Remove(TimerBox.CaretIndex - 1, 1);
                TimerBox.Text += temp;
            }

            TimerBox.Select(TimerBox.Text.Length, 0);

            string hours = "00";
            string minutes = "00";
            string seconds = "00";

            if (TimerBox.Text.Length >= 2)
            {
                seconds = TimerBox.Text.Substring(TimerBox.Text.Length - 2, 2);
            }
            else if (TimerBox.Text.Length == 1)
            {
                seconds = "0" + TimerBox.Text.Substring(TimerBox.Text.Length - 1, 1);
            }

            if (TimerBox.Text.Length >= 4)
            {
                minutes = TimerBox.Text.Substring(TimerBox.Text.Length - 4, 2);
            }
            else if (TimerBox.Text.Length == 3)
            {
                minutes = "0" + TimerBox.Text.Substring(TimerBox.Text.Length - 3, 1);
            }

            if (TimerBox.Text.Length >= 6)
            {
                hours = TimerBox.Text.Substring(TimerBox.Text.Length - 6, 2);
            }
            else if (TimerBox.Text.Length == 5)
            {
                hours = "0" + TimerBox.Text.Substring(TimerBox.Text.Length - 5, 1);
            }

            TimerLabel.Content = hours + "h " + minutes + "m " + seconds + "s";

            //convert to seconds
            _secondsLeft = Convert.ToInt32(seconds);
            _secondsLeft += Convert.ToInt32(minutes) * 60;
            _secondsLeft += Convert.ToInt32(hours) * 60 * 60;
        }
        
        private void TimerTbx_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key==Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void TimerTbx_OnGotFocus(object sender, RoutedEventArgs e)
        {
            //Pause the Timer
            PauseIcon.Visibility = Visibility.Collapsed;
            PlayIcon.Visibility = Visibility.Visible;
            _timerOn = false;
            _timer.Stop();

            TimerLabel.Foreground = Brushes.LightGray;
            TimerBox.Text = "000000";
            //TimerTbx.Text = _time.Hours.ToString().PadLeft(2, '0') + _time.Minutes.ToString().PadLeft(2, '0') + _time.Seconds.ToString().PadLeft(2,'0');
        }

        private void TimerTbx_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (_secondsLeft == 0)
            {
                TimerBorder.Background = Brushes.Red;
                StartStopButton.Background = Brushes.Salmon;
            }

            if (_secondsLeft <= 300)
            {
                TimerBorder.Background = Brushes.OrangeRed;
                StartStopButton.Background = Brushes.DarkOrange;
            }

            else
            {
                TimerBorder.Background = Brushes.Transparent;
                StartStopButton.Background = Brushes.DodgerBlue;
            }
        }
    }
}
