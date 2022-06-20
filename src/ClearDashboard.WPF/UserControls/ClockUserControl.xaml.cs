using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for ClockUserControl.xaml
    /// </summary>
    public partial class ClockUserControl : UserControl, INotifyPropertyChanged
    {
        System.Timers.Timer _refreshTimer = new System.Timers.Timer(3000);
        private int _myMinute;
        private int _myHour;

        private int _nameTimeMinute;
        private int _nameTimeHour;

        private string _savedMinuteString;
        private string _savedHourString;
        private int _savedMinuteInt;
        private int _savedHourInt;

        private int _timeDisplayIndex=0;

        private TimeZoneInfo _tempTimeZoneInfo = null;
        private string _tempHeader = "";


        private MenuItemNest _parentMenuItemNest;

        private ObservableCollection<MenuItemNest> _menuItems;
        public ObservableCollection<MenuItemNest> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
                OnPropertyChanged();
            }
        }


        public string localTime { get; set; }

        public ClockUserControl()
        {
            InitializeComponent();
            DataContext = this;
            
            //Construct MenuItemNest of TimeZones
            var timezones = TimeZoneInfo.GetSystemTimeZones();
            ObservableCollection<MenuItemNest> timeZoneMenuItemNest = new();
            foreach (var timezone in timezones)
            {
                timeZoneMenuItemNest.Add(new MenuItemNest
                {
                    Header = timezone.DisplayName, 
                    ClockCheckBoxVisibility = Visibility.Collapsed, 
                    ClockTextBoxVisibility = Visibility.Collapsed, 
                    ClockTextBlockVisibility = Visibility.Visible, 
                    ClockAddTimeZoneVisibility = Visibility.Collapsed,
                    NameTimeVisibility = Visibility.Collapsed,
                    TimeZoneInfo = timezone, 
                    MenuLevel = MenuItemNest.ClockMenuLevel.Utc, 
                    DeleteButtonVisibility = Visibility.Collapsed
                });
            }

            //Construct NameSettings MenuItemNests
            ObservableCollection<MenuItemNest> SettingsMenuItemNest = new();

            StringCollection SettingsStringCollection = new StringCollection();
            SettingsStringCollection = Properties.Settings.Default.TimeZones;

            foreach (var setting in SettingsStringCollection)
            {
                var splitArr = setting.Split(';');

                //cycle through each time zone and check what matches
                foreach(var timezone in timezones)
                {
                    if (timezone.DisplayName == splitArr[2])
                    {
                        SettingsMenuItemNest.Add(new MenuItemNest
                        {
                            //set the CheckMark/CheckBox
                            CheckBoxIsChecked = splitArr[0],
                            ClockCheckBoxVisibility = Visibility.Visible,
                            //set the name/TextBox
                            TextBoxText = splitArr[1],
                            ClockTextBoxVisibility = Visibility.Visible,
                            //set the UTC/TextBlock
                            Header = splitArr[2],
                            NameTime = TimeZoneInfo.ConvertTime(DateTime.Now, timezone).ToShortTimeString(),
                            ClockAddTimeZoneVisibility = Visibility.Collapsed,
                            NameTimeVisibility = Visibility.Visible,
                            //set MenuItems to TimeZoneMenuItemNest
                            MenuItems = timeZoneMenuItemNest,
                            MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                            DeleteButtonVisibility = Visibility.Visible,
                            TimeZoneInfo = timezone
                        });
                    }
                }

                //////calcualte or recall from database name Time in here somewhere
                //////we have the utc string so we take the UTC
                //////We also have to know our own UTC.  

                ////string nameTime;

                ////_savedMinuteInt = 0;
                ////_savedHourInt = 0;

                ////try
                ////{
                ////    _savedMinuteString = splitArr[2].Split(':')[1].PadLeft(2, '0').Substring(0, 2);
                ////    Int32.TryParse(_savedMinuteString, out _savedMinuteInt);
                ////    _nameTimeMinute = DateTime.UtcNow.Minute + _savedMinuteInt;
                ////    if (_nameTimeMinute > 60)
                ////    {
                ////        _nameTimeMinute -= 60;
                ////    }
                ////}
                ////catch
                ////{
                ////    _nameTimeMinute = 0;
                ////}

                ////_savedHourString = splitArr[2].Split(':')[0].PadLeft(7, '0').Substring(5, 2);
                ////Int32.TryParse(_savedHourString, out _savedHourInt);
                ////if ((splitArr[2]).PadLeft(5, '0').Substring(4, 1) == "+")
                ////{
                ////    _nameTimeHour = DateTime.UtcNow.Hour + _savedHourInt;
                ////}
                ////if ((splitArr[2]).PadLeft(5, '0').Substring(4, 1) == "-")
                ////{
                ////    _nameTimeHour = DateTime.UtcNow.Hour - _savedHourInt + 1;
                ////}

                ////if (_nameTimeHour > 24)
                ////{
                ////    _nameTimeHour -= 24;
                ////}

                ////if (_nameTimeHour > 12)
                ////{
                ////    _nameTimeHour -= 12;
                ////    nameTime = _nameTimeHour + ":" + _nameTimeMinute.ToString().PadLeft(2, '0') + " PM";

                ////}
                ////else
                ////{
                ////    nameTime = _nameTimeHour + ":" + _nameTimeMinute.ToString().PadLeft(2, '0') + " AM";

                ////}

                
            }

            //additional button
            SettingsMenuItemNest[SettingsMenuItemNest.Count - 1].ClockCheckBoxVisibility = Visibility.Collapsed;
            SettingsMenuItemNest[SettingsMenuItemNest.Count - 1].ClockTextBoxVisibility = Visibility.Collapsed;
            SettingsMenuItemNest[SettingsMenuItemNest.Count - 1].ClockTextBlockVisibility = Visibility.Collapsed;
            SettingsMenuItemNest[SettingsMenuItemNest.Count - 1].ClockAddTimeZoneVisibility = Visibility.Visible;
            SettingsMenuItemNest[SettingsMenuItemNest.Count - 1].NameTimeVisibility = Visibility.Collapsed;
            SettingsMenuItemNest[SettingsMenuItemNest.Count - 1].CheckBoxIsChecked = "True";
            SettingsMenuItemNest[SettingsMenuItemNest.Count - 1].DeleteButtonVisibility = Visibility.Collapsed;

            MenuItems = new ObservableCollection<MenuItemNest>
            {
                new MenuItemNest { 
                    Header = "Name 12:00am",
                    ClockCheckBoxVisibility=Visibility.Collapsed, 
                    ClockTextBoxVisibility = Visibility.Collapsed, 
                    ClockAddTimeZoneVisibility = Visibility.Collapsed,
                    NameTimeVisibility = Visibility.Collapsed,
                    MenuItems = SettingsMenuItemNest,
                    MenuLevel = MenuItemNest.ClockMenuLevel.Display,
                    DeleteButtonVisibility = Visibility.Collapsed
                }
            };

            ////Clock Refresh
            //_myMinute = DateTime.Now.Minute;
            //_myHour = DateTime.Now.Hour;

            //if (_myHour > 12)
            //{
            //    _myHour -= 12;
            //    localTime = "Local " + _myHour + ":" + _myMinute.ToString().PadLeft(2, '0') + " PM";

            //}
            //else
            //{
            //    localTime = "Local " + _myHour + ":" + _myMinute.ToString().PadLeft(2, '0') + " AM";

            //}
            MenuItems[0].Header = "Local " + DateTime.Now.ToShortTimeString();
            //ClockLabel.Content = Properties.Settings.Default.TimeZones[0];

            _refreshTimer.Elapsed += ClockRefresh;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
            
        }

        private void SaveMenuToSettings()
        {
            StringCollection settingsStringCollection = new();

            foreach (var menuItem in MenuItems[0].MenuItems)
            {
                string menuSettings = menuItem.CheckBoxIsChecked + ";" + menuItem.TextBoxText + ";" + menuItem.Header;
                settingsStringCollection.Add(menuSettings);
            }

            //if (optionalUtc == "None")
            //{
                
            //}
            //else
            //{ //(But we need to update the timezone also?)
            //    if (UtcButton.Tag is TimeZoneInfo utcButtonTimeZoneInfo)
            //    {
            //        _parentMenuItemNest.TimeZoneInfo = utcButtonTimeZoneInfo;
            //        _parentMenuItemNest.Header = utcButtonTimeZoneInfo.DisplayName;
            //        ClockLabel.Content = _parentMenuItemNest.Header;
            //        foreach (var menuItem in MenuItems[0].MenuItems)
            //        {
            //            string menuSettings = menuItem.CheckBoxIsChecked + ";" + menuItem.TextBoxText + ";" + menuItem.Header;
            //            SettingsStringCollection.Add(menuSettings);
            //        }
            //    }

                
            //}

            Properties.Settings.Default.TimeZones = settingsStringCollection;
            Properties.Settings.Default.Save();
        }

        private void ClockRefresh(object sender, ElapsedEventArgs e)
        {
            //Progress to the next Checked Menu Item.  If end of list reached then display local time

            _timeDisplayIndex++;

            bool checkedFound = false;
            while (!checkedFound)
            {
                if (_timeDisplayIndex <= MenuItems[0].MenuItems.Count - 1)
                {
                    if (MenuItems[0].MenuItems[_timeDisplayIndex - 1].CheckBoxIsChecked == "False")
                    {
                        _timeDisplayIndex++;
                    }
                    else
                    {
                        checkedFound = true;
                    }
                }
                else
                {
                    break;
                }
            }

            //while (MenuItems[0].MenuItems[_timeDisplayIndex - 1].CheckBoxIsChecked == "False")
            //{
            //    _timeDisplayIndex++;
            //}

            //Display Local Time
            if (_timeDisplayIndex > MenuItems[0].MenuItems.Count-1)
            {
                _timeDisplayIndex = 0;

                //_myMinute = DateTime.Now.Minute;
                //_myHour = DateTime.Now.Hour;

                this.Dispatcher.Invoke(() =>
                {
                    //if (_myHour > 12)
                    //{
                    //    _myHour -= 12;
                    //    localTime = "Local " + _myHour + ":" + _myMinute.ToString().PadLeft(2, '0') + " PM";
                    //}
                    //else
                    //{
                    //    localTime = "Local " + _myHour + ":" + _myMinute.ToString().PadLeft(2, '0') + " AM";
                    //}

                    MenuItems[0].Header = "Local "+DateTime.Now.ToShortTimeString();

                });
            }

            //Display Times on MenuItems List
            else
            {
                //Display Time
                MenuItems[0].Header = MenuItems[0].MenuItems[_timeDisplayIndex - 1].TextBoxText + " " + MenuItems[0].MenuItems[_timeDisplayIndex - 1].NameTime;
            }

            //Update Times
            foreach (MenuItemNest nest in MenuItems[0].MenuItems)
            {
                //string nameTime;

                //_savedMinuteInt = 0;
                //_savedHourInt = 0;

                //try
                //{
                //    _savedMinuteString = nest.Header.Split(':')[1].PadLeft(2, '0').Substring(0, 2);
                //    Int32.TryParse(_savedMinuteString, out _savedMinuteInt);
                //    _nameTimeMinute = DateTime.UtcNow.Minute + _savedMinuteInt;
                //    if (_nameTimeMinute > 60)
                //    {
                //        _nameTimeMinute -= 60;
                //    }
                //}
                //catch
                //{
                //    _nameTimeMinute = 0;
                //}

                //_savedHourString = nest.Header.Split(':')[0].PadLeft(7, '0').Substring(5, 2);
                //Int32.TryParse(_savedHourString, out _savedHourInt);
                //if ((nest.Header).PadLeft(5, '0').Substring(4, 1) == "+")
                //{
                //    _nameTimeHour = DateTime.UtcNow.Hour + _savedHourInt;
                //}
                //if ((nest.Header).PadLeft(5, '0').Substring(4, 1) == "-")
                //{
                //    _nameTimeHour = DateTime.UtcNow.Hour - _savedHourInt + 1;
                //}

                //if (_nameTimeHour > 24)
                //{
                //    _nameTimeHour -= 24;
                //}

                //if (_nameTimeHour > 12)
                //{
                //    _nameTimeHour -= 12;
                //    nameTime = _nameTimeHour + ":" + _nameTimeMinute.ToString().PadLeft(2, '0') + " PM";

                //}
                //else
                //{
                //    nameTime = _nameTimeHour + ":" + _nameTimeMinute.ToString().PadLeft(2, '0') + " AM";

                //}
                nest.NameTime = TimeZoneInfo.ConvertTime(DateTime.Now, nest.TimeZoneInfo).ToShortTimeString();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            SaveMenuToSettings();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            SaveMenuToSettings();
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                //if (item.Header is MenuItemNest min)
                //{
                //    ClockLabel.Content = min.Header;
                //}

                //if (item.Items.CurrentItem is MenuItemNest nest)
                //{
                //    _parentMenuItemNest = nest;
                //}
            }
        }

        private void OnMouseMove_Handler(object sender, MouseEventArgs e)
        {
            if (sender is MenuItem item)
            {
                //if (item.Header is MenuItemNest min)
                //{
                //    ClockLabel.Content = min.Header;
                //}

                if (item.Items.CurrentItem is MenuItemNest nest)
                {
                    _parentMenuItemNest = nest;
                }
            }
        }

        private void Click_OnHandler(object sender, RoutedEventArgs e)
        {
            //look at the datacontext enum of the sender and act accordingly aka do a save on the prper enum stage/cycle
            //the save function might as well get everything so why not just send it the whole object (sender is a menu item, datacontext is a nest)
            //we can either set the _parent Item menu nest here or sent it in the function.  or we can use it to constuct a tree
            //the save function needs the zone info and header of the lowest level and it needs to set those at the mid level
            //we cna just do that save stuff here and the call the save function.  the problem is we need both levels here

            //act here according to enum.  on forst iteratin we set temp variables.  one the second iteratin we set the toggle variables, the third one we do nothing
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is MenuItemNest nest)
                {
                    if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Utc)
                    {
                        _tempHeader = nest.Header;
                        _tempTimeZoneInfo = nest.TimeZoneInfo;
                    }

                    if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Individual)
                    {
                        nest.Header = _tempHeader;
                        nest.TimeZoneInfo = _tempTimeZoneInfo;
                    }
                }
            }
            SaveMenuToSettings();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //Construct MenuItemNest of TimeZones
            var timezones = TimeZoneInfo.GetSystemTimeZones();
            ObservableCollection<MenuItemNest> timeZoneMenuItemNest = new();
            foreach (var timezone in timezones)
            {
                timeZoneMenuItemNest.Add(new MenuItemNest
                {
                    Header = timezone.DisplayName,
                    ClockCheckBoxVisibility = Visibility.Collapsed,
                    ClockTextBoxVisibility = Visibility.Collapsed,
                    ClockTextBlockVisibility = Visibility.Visible,
                    ClockAddTimeZoneVisibility = Visibility.Collapsed,
                    NameTimeVisibility = Visibility.Collapsed,
                    TimeZoneInfo = timezone,
                    MenuLevel = MenuItemNest.ClockMenuLevel.Utc,
                    DeleteButtonVisibility = Visibility.Collapsed
                });
            }

            MenuItems[0].MenuItems.Insert(MenuItems[0].MenuItems.Count-1,new MenuItemNest
            {
                //set the CheckMark/CheckBox
                CheckBoxIsChecked = "False",
                ClockCheckBoxVisibility = Visibility.Visible,
                //set the name/TextBox
                TextBoxText = TimeZoneInfo.Local.StandardName,
                ClockTextBoxVisibility = Visibility.Visible,
                //set the UTC/TextBlock
                Header = TimeZoneInfo.Local.DisplayName,
                NameTime = DateTime.Now.ToShortTimeString(),
                ClockAddTimeZoneVisibility = Visibility.Collapsed,
                NameTimeVisibility = Visibility.Visible,
                //set MenuItems to TimeZoneMenuItemNest
                MenuItems = timeZoneMenuItemNest,
                MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                DeleteButtonVisibility = Visibility.Visible,
                TimeZoneInfo = TimeZoneInfo.Local
            });

            SaveMenuToSettings();
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Visibility=Visibility.Collapsed;
                List<MenuItemNest> removeItemList = new();
                foreach (var item in MenuItems[0].MenuItems)
                {
                    if (item.DeleteButtonVisibility == Visibility.Collapsed)
                    {
                       removeItemList.Add(item);
                    }
                }

                foreach (var item in removeItemList)
                {
                    if (item.ClockAddTimeZoneVisibility != Visibility.Visible)
                    {
                        MenuItems[0].MenuItems.Remove(item);
                    }
                }
                
                SaveMenuToSettings();
            }
        }
    }

    public class MenuItemNest: INotifyPropertyChanged
    {
        private readonly ICommand _command;

        public MenuItemNest()
        {
            _command = new MenuCommand(Execute);
        }

        public Visibility ClockCheckBoxVisibility { get; set; }
        private string _checkBoxIsChecked { get; set; }
        public string CheckBoxIsChecked
        {
            get { return _checkBoxIsChecked; }
            set
            {
                _checkBoxIsChecked = value;
                OnPropertyChanged();
            }
        }

        public Visibility ClockTextBoxVisibility { get; set; }
        private string _textBoxText { get; set; }
        public string TextBoxText
    {
            get { return _textBoxText; }
            set
            {
                _textBoxText = value;
                OnPropertyChanged();
            }
        }
        public Visibility ClockTextBlockVisibility { get; set; }
        private string _header { get; set; }
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }
        public Visibility ClockAddTimeZoneVisibility { get; set; }
        public Visibility NameTimeVisibility { get; set; }
        public Visibility DeleteButtonVisibility { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }

        public ClockMenuLevel MenuLevel { get; set; }

        public enum ClockMenuLevel
        {
            Display,
            Group,
            Individual,
            Utc
        }

        private string _nameTime { get; set; }
        public string NameTime
        {
            get { return _nameTime; }
            set
            {
                _nameTime = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<MenuItemNest> MenuItems { get; set; }

        public ICommand Command
        {
            get
            {
                return _command;
            }
        }

        private void Execute()
        {
            // (NOTE: In a view model, you normally should not use MessageBox.Show()).
            MessageBox.Show("Clicked at " + Header);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }
    }

    public class MenuCommand : ICommand
    {
        private readonly Action _action;

        public MenuCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object o)
        {
            _action();
        }

        public bool CanExecute(object o)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
