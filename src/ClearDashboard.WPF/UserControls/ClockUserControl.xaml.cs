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
using ClearDashboard.Wpf.Helpers;
using Microsoft.Extensions.Logging;
using SIL.Extensions;

namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for ClockUserControl.xaml
    /// </summary>
    public partial class ClockUserControl : UserControl, INotifyPropertyChanged
    {
        private ILogger _logger;
        private List<string> utcComboList = new();

        System.Timers.Timer _refreshTimer = new System.Timers.Timer(3000);

        private int _timeDisplayIndex = 0;

        private ReadOnlyCollection<TimeZoneInfo> _timezones = TimeZoneInfo.GetSystemTimeZones();

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
            foreach (var timezone in _timezones)
            {
                utcComboList.Add(timezone.DisplayName);
            }

            //Construct Individual MenuItemNests
            ObservableCollection<MenuItemNest> SettingsMenuItemNest = new();

            StringCollection SettingsStringCollection = new StringCollection();
            SettingsStringCollection = Properties.Settings.Default.TimeZones;
            if (SettingsStringCollection != null)
            {
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
                            foreach (var timezone in _timezones)
                            {
                                if (timezone.DisplayName == individualArr[2])
                                {
                                    groupMenuItemNest.Add(new MenuItemNest
                                    {
                                        AddButtonVisibility = Visibility.Collapsed,
                                        CheckBoxIsChecked = individualArr[0],
                                        CheckBoxVisibility = Visibility.Visible,
                                        TextBoxText = individualArr[1],
                                        TextBoxVisibility = Visibility.Visible,
                                        NameTime = TimeZoneInfo.ConvertTime(DateTime.Now, timezone).ToShortTimeString(),
                                        NameTimeVisibility = Visibility.Visible,
                                        TextBlockText = individualArr[2],
                                        TextBlockVisibility = Visibility.Collapsed,
                                        DeleteButtonVisibility = Visibility.Visible,
                                        GroupName = individualArr[3],
                                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                                        TimeZoneInfo = timezone,
                                        //MenuItems = _timeZoneMenuItemNest,
                                        UtcStringList = utcComboList,
                                        UtcComboVisibility = Visibility.Visible,
                                        UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                                    });
                                }
                            }
                        }
                    }

                    var selfArr = groupArr[0].Split(",");
                    MenuItemNest groupMenuItem = new MenuItemNest
                    {
                        AddButtonVisibility = Visibility.Visible,
                        CheckBoxIsChecked = selfArr[0],
                        CheckBoxVisibility = Visibility.Visible,
                        TextBoxText = selfArr[1],
                        TextBoxVisibility = Visibility.Visible,
                        NameTimeVisibility = Visibility.Collapsed,
                        TextBlockText = selfArr[2],
                        TextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        GroupName = selfArr[3],
                        MenuLevel = MenuItemNest.ClockMenuLevel.Group,
                        MenuItems = groupMenuItemNest,
                        UtcComboVisibility = Visibility.Collapsed,
                    };

                    SettingsMenuItemNest.Add(groupMenuItem);
                }

            }

            SettingsMenuItemNest.Add(new MenuItemNest
            {
                AddButtonVisibility = Visibility.Visible,
                CheckBoxVisibility = Visibility.Collapsed,
                TextBoxVisibility = Visibility.Collapsed,
                NameTimeVisibility = Visibility.Collapsed,
                DeleteButtonVisibility = Visibility.Collapsed,
                TextBlockVisibility = Visibility.Collapsed,
                MenuLevel = MenuItemNest.ClockMenuLevel.Group,
                UtcComboVisibility = Visibility.Collapsed,
            });

            MenuItems = new ObservableCollection<MenuItemNest>
            {
                new MenuItemNest {

                    AddButtonVisibility = Visibility.Collapsed,
                    CheckBoxVisibility=Visibility.Collapsed,
                    TextBoxVisibility = Visibility.Collapsed,
                    NameTimeVisibility = Visibility.Visible,
                    TextBlockVisibility = Visibility.Visible,
                    DeleteButtonVisibility = Visibility.Collapsed,
                    MenuLevel = MenuItemNest.ClockMenuLevel.Display,
                    MenuItems = SettingsMenuItemNest,
                    UtcComboVisibility = Visibility.Collapsed,
                }
            };

            MenuItems[0].NameTime = DateTime.Now.ToString("HH:mm");
            MenuItems[0].TextBlockText = GetLocalizedLocalTimeString();


            _refreshTimer.Elapsed += ClockRefresh;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
            InstantClockRefresh();
        }

        private void SaveMenuToSettings()
        {
            StringCollection settingsStringCollection = new();

            foreach (var group in MenuItems[0].MenuItems)
            {
                string groupSettings = group.CheckBoxIsChecked + "," +
                                       group.TextBoxText + "," +
                                       group.TextBlockText + "," +
                                       group.GroupName;
                if (group.MenuItems != null)
                {
                    foreach (var individual in group.MenuItems)
                    {
                        string individualSettings =
                            individual.CheckBoxIsChecked + "," +
                            individual.TextBoxText + "," +
                            individual.TextBlockText + "," +
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
            UpdateClockMenuItems();

            //set display to local time
            _timeDisplayIndex++;
            if (_timeDisplayIndex>=CheckedList.Count)
            {
                _timeDisplayIndex = -1;

                this.Dispatcher.Invoke(() =>
                {
                    MenuItems[0].NameTime = DateTime.Now.ToString("HH:mm");
                    MenuItems[0].TextBlockText = GetLocalizedLocalTimeString();

                    if (DateTime.Now.Hour >= 9 && DateTime.Now.Hour < 17)
                    {
                        MenuItems[0].Foreground = Brushes.LimeGreen;
                    }
                    else if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 22)
                    {
                        MenuItems[0].Foreground = Brushes.DarkOrange;
                    }
                    else
                    {
                        MenuItems[0].Foreground = Brushes.Red;
                    }

                });
            }
            else
            {
                SetDisplayClockFromCheckedList();
            }
        }

        private string GetLocalizedLocalTimeString()
        {
            return LocalizationStrings.Get("ClockUserControl_localTime", _logger);
        }

        private void InstantClockRefresh()
        {
            UpdateClockMenuItems();

            if (_timeDisplayIndex < CheckedList.Count && _timeDisplayIndex != -1)
            {
                SetDisplayClockFromCheckedList();
            }
        }

        private void UpdateClockMenuItems()
        {
            //create list of checked menu items and update old ones

            CheckedList = new();
            foreach (var group in MenuItems[0].MenuItems)
            {
                if (group.CheckBoxIsChecked == "True")
                {
                    foreach (var individual in group.MenuItems)
                    {
                        var tempTime = TimeZoneInfo.ConvertTime(DateTime.Now, individual.TimeZoneInfo);
                        individual.NameTime = tempTime.ToString("HH:mm");

                        if (tempTime.Hour >= 9 && tempTime.Hour < 17)
                        {
                            individual.Foreground = Brushes.LimeGreen;
                        }
                        else if (tempTime.Hour >= 8 && tempTime.Hour < 22)
                        {
                            individual.Foreground = Brushes.DarkOrange;
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
                            individual.NameTime = tempTime.ToString("HH:mm");

                            if (tempTime.Hour >= 9 && tempTime.Hour < 17)
                            {
                                individual.Foreground = Brushes.LimeGreen;
                            }
                            else if (tempTime.Hour >= 8 && tempTime.Hour < 22)
                            {
                                individual.Foreground = Brushes.DarkOrange;
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
        }

        private void SetDisplayClockFromCheckedList()
        {
            this.Dispatcher.Invoke(() =>
            {
               
             MenuItems[0].NameTime = CheckedList[_timeDisplayIndex].NameTime;
            MenuItems[0].TextBlockText = " " + CheckedList[_timeDisplayIndex].TextBoxText;
            MenuItems[0].Foreground = CheckedList[_timeDisplayIndex].Foreground;
        });
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            sortMenuItemsGroup();
            InstantClockRefresh();
            SaveMenuToSettings();
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            InstantClockRefresh();
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
                        nest.Insert(nest.Count, new MenuItemNest
                        {
                            AddButtonVisibility = Visibility.Collapsed,
                            CheckBoxIsChecked = "True",
                            CheckBoxVisibility = Visibility.Visible,
                            TextBoxText = "newindividual",
                            TextBoxVisibility = Visibility.Visible,
                            NameTime = DateTime.Now.ToShortTimeString(),
                            TextBlockText = TimeZoneInfo.Local.DisplayName,
                            TextBlockVisibility = Visibility.Collapsed,
                            NameTimeVisibility = Visibility.Visible,
                            DeleteButtonVisibility = Visibility.Visible,
                            MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                            TimeZoneInfo = TimeZoneInfo.Local,
                            //MenuItems = _timeZoneMenuItemNest,
                            UtcComboVisibility = Visibility.Visible,
                            UtcStringList = utcComboList,
                            UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                        });
                        
                        sortMenuItemsIndividual(nest);
                        InstantClockRefresh();
                        SaveMenuToSettings();
                    }
                }
                else//Add Group
                {
                    MenuItems[0].MenuItems.Insert(MenuItems[0].MenuItems.Count - 1, new MenuItemNest
                    {
                        AddButtonVisibility = Visibility.Visible,
                        CheckBoxIsChecked = "True",
                        CheckBoxVisibility = Visibility.Visible,
                        TextBoxText = "newgroup",
                        TextBoxVisibility = Visibility.Visible,
                        NameTimeVisibility = Visibility.Collapsed,
                        TextBlockText = "self",
                        TextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local,
                        MenuItems = new(),
                        UtcComboVisibility = Visibility.Collapsed,
                    });

                    //add an item to the group
                    MenuItems[0].MenuItems[MenuItems[0].MenuItems.Count - 2].MenuItems.Add(new MenuItemNest
                    {
                        AddButtonVisibility = Visibility.Collapsed,
                        CheckBoxIsChecked = "True",
                        CheckBoxVisibility = Visibility.Visible,
                        TextBoxText = "newindividual",
                        TextBoxVisibility = Visibility.Visible,
                        NameTime = DateTime.Now.ToShortTimeString(),
                        NameTimeVisibility = Visibility.Visible,
                        TextBlockText = TimeZoneInfo.Local.DisplayName,
                        TextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        //MenuItems = _timeZoneMenuItemNest,
                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local,
                        UtcComboVisibility = Visibility.Visible,
                        UtcStringList = utcComboList,
                        UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                    });

                    sortMenuItemsGroup();
                    InstantClockRefresh();
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
                            if (item.AddButtonVisibility != Visibility.Visible)
                            {
                                group.MenuItems.Remove(item);
                            }
                        }
                    }
                    
                }
                foreach (var group in removeGroupList)
                {
                    if (group.TextBoxVisibility == Visibility.Visible)
                    {
                        MenuItems[0].MenuItems.Remove(group);
                    }
                }

                SaveMenuToSettings();
            }
        }

        private void UtcComboSelected(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.Tag is TimeZoneInfo timeZoneInfo)
                {
                    foreach (var timezone in _timezones)
                    {
                        if (timezone.DisplayName == (string)comboBox.SelectedItem)
                        {
                            comboBox.Tag = timezone;
                        }
                    }

                    if (comboBox.DataContext is MenuItemNest nest)
                    {
                        //find the group that the nest is in
                        foreach (var group in MenuItems[0].MenuItems)
                        {
                            if (group.MenuItems != null)
                            {
                                foreach (var individual in group.MenuItems)
                                {
                                    if (individual == nest)
                                    {
                                        sortMenuItemsIndividual(group.MenuItems);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //call clock refesh without the dispaly part
                    InstantClockRefresh();
                    SaveMenuToSettings();
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

        private void sortMenuItemsIndividual(ObservableCollection<MenuItemNest> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                for (int j = 0; j < collection.Count - 1; j++)
                {
                    if (collection[j].TimeZoneInfo.BaseUtcOffset.CompareTo(
                            collection[j + 1].TimeZoneInfo.BaseUtcOffset) >0)
                    {
                        collection.Move(j,j+1);
                    }
                }
            }
        }

        private void sortMenuItemsGroup()
        {
            for (int i = 0; i < MenuItems[0].MenuItems.Count - 1; i++)
            {
                for (int j = 0; j < MenuItems[0].MenuItems.Count - 2; j++)
                {
                    if (MenuItems[0].MenuItems[j].TextBoxText.CompareTo(MenuItems[0].MenuItems[j + 1].TextBoxText) > 0)
                    {
                        MenuItems[0].MenuItems.Move(
                            MenuItems[0].MenuItems.IndexOf(MenuItems[0].MenuItems[j]), 
                            MenuItems[0].MenuItems.IndexOf(MenuItems[0].MenuItems[j + 1]));
                    }
                }
            }
        }

        private void TextBox_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBox box)
            {
                box.Foreground = Brushes.Gray;
            }
            if (sender is ComboBox cbox)
            {
                cbox.Background = Brushes.CornflowerBlue;
            }
        }

        private void TextBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBox box)
            {
                box.Foreground = Brushes.Black;
            }
            if (sender is ComboBox cbox)
            {
                cbox.Background = Brushes.Transparent;
            }
        }
    }

    public class MenuItemNest : INotifyPropertyChanged
    {
        public MenuItemNest()
        {
        }

        public Visibility AddButtonVisibility { get; set; }
        public Visibility DeleteButtonVisibility { get; set; }

        public Visibility CheckBoxVisibility { get; set; }
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
        public Visibility TextBoxVisibility { get; set; }

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
        public Visibility TextBlockVisibility { get; set; }
        private string _textBlockText { get; set; }
        public string TextBlockText
        {
            get { return _textBlockText; }
            set
            {
                _textBlockText = value;
                OnPropertyChanged();
            }
        }

        public Visibility UtcComboVisibility { get; set; }
        private string _utcComboSelectedString { get; set; } = "(UTC) Coordinated Universal Time";
        public string UtcComboSelectedString
        {
            get { return _utcComboSelectedString; }
            set
            {
                _utcComboSelectedString = value;
                OnPropertyChanged();
            }
        }

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

        private Brush _foreground { get; set; } = Brushes.LimeGreen;
        public Brush Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged();
            }
        }

        public List<string> UtcStringList { get; set; }

        public ObservableCollection<MenuItemNest> MenuItems { get; set; }

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
}
