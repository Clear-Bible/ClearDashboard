using System;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;


namespace ClearDashboard.Wpf.Application.UserControls
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

        private void TimerTbx_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }



        private void StartStop_OnClick(object sender, RoutedEventArgs e)
        {
            SetAppearance();

            TimerLabel.Foreground = Brushes.White;
            if (_timerOn)
            {
                PauseTimer();
            }
            else
            {
                //Start Timer
                PauseIcon.Visibility = Visibility.Visible;
                PlayIcon.Visibility = Visibility.Collapsed;
                _timerOn = true;
                _timer.Enabled = true;
            }
        }
        
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            _secondsLeft--;
         
            SetAppearance();
        }

        private void SetAppearance()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (_secondsLeft <= 0)
                {
                    SetAppearNoTimeLeft();
                }
                else
                {
                    if (_secondsLeft <= 300)
                    {
                        SetAppearLittleTimeLeft();
                    }
                    else
                    {
                        SetAppearMuchTimeLeft();
                    }
                }
                SetTimerLabel();
            });
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

            SetLabelAndSeconds(hours, minutes, seconds);
        }

        private void SetLabelAndSeconds(string hours, string minutes, string seconds)
        {
            TimerLabel.Content = hours + "h " + minutes + "m " + seconds + "s";

            //convert to seconds
            _secondsLeft = Convert.ToInt32(seconds);
            _secondsLeft += Convert.ToInt32(minutes) * 60;
            _secondsLeft += Convert.ToInt32(hours) * 60 * 60;
        }


        private void TimerTbx_OnGotFocus(object sender, RoutedEventArgs e)
        {
           SetAppearMuchTimeLeft();

           PauseTimer();

            TimerLabel.Foreground = Brushes.LightGray;
            TimerBox.Text = "00000000000000";
            SetLabelAndSeconds("00", "00","00");


            //TimerTbx.Text = _time.Hours.ToString().PadLeft(2, '0') + _time.Minutes.ToString().PadLeft(2, '0') + _time.Seconds.ToString().PadLeft(2,'0');
        }

        private void TimerTbx_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TimerLabel.Foreground = Brushes.White;
            if (_secondsLeft == 0)
            {
                //SetAppearNoTimeLeft();
            }

            if (_secondsLeft <= 300)
            {
                //SetAppearLittleTimeLeft();
            }

            else
            {
                //SetAppearMuchTimeLeft();
            }
        }



        private void SetTimerLabel()
        {
            _time = TimeSpan.FromSeconds(_secondsLeft);
            TimerLabel.Content = (int)_time.TotalHours + "h " + _time.Minutes + "m " + _time.Seconds + "s";
        }

        private void PauseTimer()
        {
            //Pause the Timer
            PauseIcon.Visibility = Visibility.Collapsed;
            PlayIcon.Visibility = Visibility.Visible;
            _timerOn = false;
            _timer.Stop();
        }

        private void SetAppearNoTimeLeft()
        {
            TimerBorder.Background = Brushes.Red;
            //StartStopButton.Background = Brushes.Salmon;
        }

        private void SetAppearLittleTimeLeft()
        {
            TimerBorder.Background = Brushes.OrangeRed;
            //StartStopButton.Background = Brushes.DarkOrange;
        }

        private void SetAppearMuchTimeLeft()
        {
            TimerBorder.Background = Brushes.Transparent;
            //StartStopButton.Background = Brushes.DodgerBlue;
        }
    }
}
