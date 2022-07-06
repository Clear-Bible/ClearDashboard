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
using SIL.Extensions;

namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for ClockUserControl.xaml
    /// </summary>
    public partial class ClockUserControl : UserControl, INotifyPropertyChanged
    {
        System.Timers.Timer _refreshTimer = new System.Timers.Timer(3000);

        private int _timeDisplayIndex = 0;

        private TimeZoneInfo _localTimeZoneInfo = TimeZoneInfo.Local;
        private TimeZoneInfo _tempTimeZoneInfo = null;
        private string _tempHeader = "";

        private ObservableCollection<MenuItemNest> _timeZoneMenuItemNest;

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

        public ObservableCollection<MenuItemNest> CheckedList { get; set; }
        
        public ClockUserControl()
        {
            InitializeComponent();
            DataContext = this;

            //Construct MenuItemNest of TimeZones
            var timezones = TimeZoneInfo.GetSystemTimeZones();
            _timeZoneMenuItemNest = new();
            foreach (var timezone in timezones)
            {
                _timeZoneMenuItemNest.Add(new MenuItemNest
                {
                    ClockAddTimeZoneVisibility = Visibility.Collapsed,
                    ClockCheckBoxVisibility = Visibility.Collapsed,
                    ClockTextBoxVisibility = Visibility.Collapsed,
                    NameTimeVisibility = Visibility.Collapsed,
                    ClockTextBlockText = timezone.DisplayName,
                    ClockTextBlockVisibility = Visibility.Visible,
                    DeleteButtonVisibility = Visibility.Collapsed,
                    TimeZoneInfo = timezone,
                    MenuLevel = MenuItemNest.ClockMenuLevel.Utc,
                    
                });
            }

            //Construct Individual MenuItemNests
            ObservableCollection<MenuItemNest> SettingsMenuItemNest = new();

            StringCollection SettingsStringCollection = new StringCollection();
            SettingsStringCollection = Properties.Settings.Default.TimeZones;

            foreach (var group in SettingsStringCollection)
            {
                //menuItems = new ObservableCollection<MenuItemNest>
                var groupArr = group.Split(";");
                ObservableCollection<MenuItemNest> groupMenuItemNest = new();

                foreach (var individual in groupArr)
                {
                    var individualArr = individual.Split(",");
                    if (individualArr[2] != "self")
                    {
                        //cycle through each time zone and check what matches
                        foreach (var timezone in timezones)
                        {
                            if (timezone.DisplayName == individualArr[2])
                            {
                                groupMenuItemNest.Add(new MenuItemNest
                                {
                                    ClockAddTimeZoneVisibility = Visibility.Collapsed,
                                    CheckBoxIsChecked = individualArr[0],
                                    ClockCheckBoxVisibility = Visibility.Visible,
                                    TextBoxText = individualArr[1],
                                    ClockTextBoxVisibility = Visibility.Visible,
                                    NameTime = TimeZoneInfo.ConvertTime(DateTime.Now, timezone).ToShortTimeString(),
                                    NameTimeVisibility = Visibility.Visible,
                                    ClockTextBlockText = individualArr[2],
                                    DeleteButtonVisibility = Visibility.Visible,
                                    GroupName = individualArr[3],
                                    MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                                    TimeZoneInfo = timezone,
                                    MenuItems = _timeZoneMenuItemNest,
                                });
                            }
                        }
                    }
                }

                var selfArr = groupArr[0].Split(",");
                MenuItemNest groupMenuItem = new MenuItemNest
                {
                    ClockAddTimeZoneVisibility = Visibility.Visible,
                    CheckBoxIsChecked = selfArr[0],
                    ClockCheckBoxVisibility = Visibility.Visible,
                    TextBoxText = selfArr[1],
                    ClockTextBoxVisibility = Visibility.Visible,
                    NameTimeVisibility = Visibility.Collapsed,
                    ClockTextBlockText = selfArr[2],
                    ClockTextBlockVisibility = Visibility.Collapsed,
                    DeleteButtonVisibility = Visibility.Visible,
                    GroupName = selfArr[3],
                    MenuLevel = MenuItemNest.ClockMenuLevel.Group,
                    MenuItems = groupMenuItemNest
                };

                SettingsMenuItemNest.Add(groupMenuItem);
            }

            SettingsMenuItemNest.Add(new MenuItemNest
            {

                ClockAddTimeZoneVisibility = Visibility.Visible,
                ClockCheckBoxVisibility = Visibility.Collapsed,
                ClockTextBoxVisibility = Visibility.Collapsed,
                NameTimeVisibility = Visibility.Collapsed,
                DeleteButtonVisibility = Visibility.Collapsed,
                ClockTextBlockVisibility = Visibility.Collapsed,
                MenuLevel = MenuItemNest.ClockMenuLevel.Group,
            });

            MenuItems = new ObservableCollection<MenuItemNest>
            {
                new MenuItemNest {

                    ClockAddTimeZoneVisibility = Visibility.Collapsed,
                    ClockCheckBoxVisibility=Visibility.Collapsed,
                    ClockTextBoxVisibility = Visibility.Collapsed,
                    NameTimeVisibility = Visibility.Visible,
                    ClockTextBlockVisibility = Visibility.Visible,
                    DeleteButtonVisibility = Visibility.Collapsed,
                    MenuLevel = MenuItemNest.ClockMenuLevel.Display,
                    MenuItems = SettingsMenuItemNest,
                }
            };

            MenuItems[0].NameTime = DateTime.Now.ToShortTimeString().PadLeft(8,'0');
            MenuItems[0].ClockTextBlockText = " Local Time";
         

            _refreshTimer.Elapsed += ClockRefresh;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        private void SaveMenuToSettings()
        {
            StringCollection settingsStringCollection = new();

            foreach (var group in MenuItems[0].MenuItems)
            {
                string groupSettings = group.CheckBoxIsChecked + "," +
                                       group.TextBoxText + "," +
                                       group.ClockTextBlockText + "," +
                                       group.GroupName;
                if (group.MenuItems != null)
                {
                    foreach (var individual in group.MenuItems)
                    {
                        string individualSettings =
                            individual.CheckBoxIsChecked + "," +
                            individual.TextBoxText + "," +
                            individual.ClockTextBlockText + "," +
                            individual.GroupName;
                        groupSettings = groupSettings + ";" + individualSettings;

                    }
                    settingsStringCollection.Add(groupSettings);
                }
            }
            
            Properties.Settings.Default.TimeZones = settingsStringCollection;
            Properties.Settings.Default.Save();
        }

        private void ClockRefresh(object sender, ElapsedEventArgs e)
        {
            //create list of checked menu items and update old ones
            CheckedList = new();
            foreach (var group in MenuItems[0].MenuItems)
            {
                if (group.CheckBoxIsChecked=="True")
                {
                    foreach (var individual in group.MenuItems)
                    {
                        var tempTime = TimeZoneInfo.ConvertTime(DateTime.Now, individual.TimeZoneInfo);
                        individual.NameTime = tempTime.ToShortTimeString();
                        if (tempTime.Hour >= 9 && tempTime.Hour < 17)
                        {
                            individual.Foreground = Brushes.Green;
                        }
                        else
                        {
                            individual.Foreground = Brushes.Red;
                        }
                        CheckedList.Add(individual);
                    }
                }
                else
                {
                    if (group.MenuItems != null)
                    {
                        foreach (var individual in group.MenuItems)
                        {
                            var tempTime = TimeZoneInfo.ConvertTime(DateTime.Now, individual.TimeZoneInfo);
                            individual.NameTime = tempTime.ToShortTimeString();
                            if (tempTime.Hour >= 9 && tempTime.Hour < 17)
                            {
                                individual.Foreground = Brushes.Green;
                            }
                            else
                            {
                                individual.Foreground = Brushes.Red;
                            }
                            if (individual.CheckBoxIsChecked == "True")
                            {
                                CheckedList.Add(individual);
                            }
                        }
                    }
                }
            }

            //update UI to a new menu Item 
            _timeDisplayIndex++;
            if (_timeDisplayIndex>=CheckedList.Count)
            {
                _timeDisplayIndex = -1;

                this.Dispatcher.Invoke(() =>
                {
                    MenuItems[0].NameTime = DateTime.Now.ToShortTimeString().PadLeft(8, '0');
                    MenuItems[0].ClockTextBlockText = " Local Time";
                    if (DateTime.Now.Hour > 9 && DateTime.Now.Hour < 17)
                    {
                        MenuItems[0].Foreground = Brushes.ForestGreen;
                    }
                    else
                    {
                        MenuItems[0].Foreground = Brushes.Red;
                    }
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    MenuItems[0].NameTime = CheckedList[_timeDisplayIndex].NameTime.PadLeft(8, '0');
                    MenuItems[0].ClockTextBlockText = " " + CheckedList[_timeDisplayIndex].TextBoxText;
                    MenuItems[0].Foreground = CheckedList[_timeDisplayIndex].Foreground;
                });
            }
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            SaveMenuToSettings();
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SaveMenuToSettings();
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag != null)//Add Individual
                {
                    if (button.Tag is ObservableCollection<MenuItemNest> nest)
                    {
                        ////find where to put new item
                        //int insertIndex=0;
                        //foreach (MenuItemNest item in nest)
                        //{
                        //    if (_localTimeZoneInfo.BaseUtcOffset < item.TimeZoneInfo.BaseUtcOffset)
                        //    {
                        //        break;
                        //    } 
                            
                        //    insertIndex++;
                            
                        //}

                        nest.Insert(nest.Count, new MenuItemNest
                        {
                            ClockAddTimeZoneVisibility = Visibility.Collapsed,
                            CheckBoxIsChecked = "False",
                            ClockCheckBoxVisibility = Visibility.Visible,
                            TextBoxText = "New Individual",
                            ClockTextBoxVisibility = Visibility.Visible,
                            NameTime = DateTime.Now.ToShortTimeString(),
                            ClockTextBlockText = TimeZoneInfo.Local.DisplayName,
                            NameTimeVisibility = Visibility.Visible,
                            DeleteButtonVisibility = Visibility.Visible,
                            MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                            TimeZoneInfo = TimeZoneInfo.Local,
                            MenuItems = _timeZoneMenuItemNest,
                        });
                        
                        SaveMenuToSettings();
                    }
                }
                else//Add Group
                {
                    MenuItems[0].MenuItems.Insert(MenuItems[0].MenuItems.Count - 1, new MenuItemNest
                    {
                        ClockAddTimeZoneVisibility = Visibility.Visible,
                        CheckBoxIsChecked = "False",
                        ClockCheckBoxVisibility = Visibility.Visible,
                        TextBoxText = "New Group",
                        ClockTextBoxVisibility = Visibility.Visible,
                        NameTimeVisibility = Visibility.Collapsed,
                        ClockTextBlockText = "self",
                        ClockTextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local,
                        MenuItems = new(),
                    });

                    //add an item to the group
                    MenuItems[0].MenuItems[MenuItems[0].MenuItems.Count - 2].MenuItems.Add(new MenuItemNest
                    {
                        ClockAddTimeZoneVisibility = Visibility.Collapsed,
                        CheckBoxIsChecked = "False",
                        ClockCheckBoxVisibility = Visibility.Visible,
                        TextBoxText = "New Individual",
                        ClockTextBoxVisibility = Visibility.Visible,
                        NameTime = DateTime.Now.ToShortTimeString(),
                        NameTimeVisibility = Visibility.Visible,
                        ClockTextBlockText = TimeZoneInfo.Local.DisplayName,
                        DeleteButtonVisibility = Visibility.Visible,
                        MenuItems = _timeZoneMenuItemNest,
                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local
                    });

                    SaveMenuToSettings();
                }
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Visibility = Visibility.Collapsed;
                List<MenuItemNest> removeIndividualList = new();
                List<MenuItemNest> removeGroupList = new();
                bool allClear = false;

                foreach (var group in MenuItems[0].MenuItems)
                {
                    if (group.DeleteButtonVisibility == Visibility.Collapsed)
                    {
                        removeGroupList.Add(group);
                    }

                    if(group.MenuItems!=null){
                        foreach (var individual in group.MenuItems)
                        {
                            if (individual.DeleteButtonVisibility == Visibility.Collapsed)
                            {
                                removeIndividualList.Add(individual);
                            }
                        }

                        foreach (var item in removeIndividualList)
                        {
                            if (item.ClockAddTimeZoneVisibility != Visibility.Visible)
                            {
                                group.MenuItems.Remove(item);
                            }
                        }
                    }
                    
                }
                foreach (var group in removeGroupList)
                {
                    if (group.ClockTextBoxVisibility == Visibility.Visible)
                    {
                        MenuItems[0].MenuItems.Remove(group);
                    }
                }

                SaveMenuToSettings();
            }
        }

        private void Utc_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is MenuItemNest nest)
                {
                    if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Utc)
                    {
                        _tempHeader = nest.ClockTextBlockText;
                        _tempTimeZoneInfo = nest.TimeZoneInfo;
                    }

                    if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Individual)
                    {
                        nest.ClockTextBlockText = _tempHeader;
                        nest.TimeZoneInfo = _tempTimeZoneInfo;
                    }

                    if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Group)
                    {
                       //nest.MenuItems.Move(0, nest.MenuItems.Count-1);
                       SaveMenuToSettings();
                    }
                }
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
    }

    public class MenuItemNest : INotifyPropertyChanged
    {
        private readonly ICommand _command;

        public MenuItemNest()
        {
            //_command = new MenuCommand(Execute);
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
        public Visibility NameTimeVisibility { get; set; }
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
        public Visibility ClockTextBlockVisibility { get; set; }
        private string _clockTextBlockText { get; set; }
        public string ClockTextBlockText
        {
            get { return _clockTextBlockText; }
            set
            {
                _clockTextBlockText = value;
                OnPropertyChanged();
            }
        }

        public Visibility ClockAddTimeZoneVisibility { get; set; }
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

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }

        private Brush _foreground { get; set; } = Brushes.Orange;
        public Brush Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
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
            MessageBox.Show("Clicked at " + ClockTextBlockText);
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
