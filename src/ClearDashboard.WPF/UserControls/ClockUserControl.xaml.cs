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
        private List<string> utcComboList = new();

    System.Timers.Timer _refreshTimer = new System.Timers.Timer(3000);

        private int _timeDisplayIndex = 0;

        private TimeZoneInfo _localTimeZoneInfo = TimeZoneInfo.Local;
        private TimeZoneInfo _tempTimeZoneInfo = null;
        private string _tempHeader = "";
        private ReadOnlyCollection<TimeZoneInfo> _timezones = TimeZoneInfo.GetSystemTimeZones();

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
            _timeZoneMenuItemNest = new();
            foreach (var timezone in _timezones)
            {
                utcComboList.Add(timezone.DisplayName);
                //_timeZoneMenuItemNest.Add(new MenuItemNest
                //{
                //    ClockAddTimeZoneVisibility = Visibility.Collapsed,
                //    ClockCheckBoxVisibility = Visibility.Collapsed,
                //    ClockTextBoxVisibility = Visibility.Collapsed,
                //    NameTimeVisibility = Visibility.Collapsed,
                //    ClockTextBlockText = timezone.DisplayName,
                //    ClockTextBlockVisibility = Visibility.Visible,
                //    DeleteButtonVisibility = Visibility.Collapsed,
                //    TimeZoneInfo = timezone,
                //    MenuLevel = MenuItemNest.ClockMenuLevel.Utc,
                //    UtcComboVisibility = Visibility.Collapsed,
                //});
            }

            //Construct Individual MenuItemNests
            ObservableCollection<MenuItemNest> SettingsMenuItemNest = new();

            StringCollection SettingsStringCollection = new StringCollection();
            SettingsStringCollection = Properties.Settings.Default.TimeZones;
            if (SettingsStringCollection != null) {
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
                                        ClockAddTimeZoneVisibility = Visibility.Collapsed,
                                        CheckBoxIsChecked = individualArr[0],
                                        ClockCheckBoxVisibility = Visibility.Visible,
                                        TextBoxText = individualArr[1],
                                        ClockTextBoxVisibility = Visibility.Visible,
                                        NameTime = TimeZoneInfo.ConvertTime(DateTime.Now, timezone).ToShortTimeString(),
                                        NameTimeVisibility = Visibility.Visible,
                                        ClockTextBlockText = individualArr[2],
                                        ClockTextBlockVisibility = Visibility.Collapsed,
                                        DeleteButtonVisibility = Visibility.Visible,
                                        GroupName = individualArr[3],
                                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                                        TimeZoneInfo = timezone,
                                        //MenuItems = _timeZoneMenuItemNest,
                                        utcStringList = utcComboList,
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
                        MenuItems = groupMenuItemNest,
                        UtcComboVisibility = Visibility.Collapsed,
                    };

                    SettingsMenuItemNest.Add(groupMenuItem);
                }

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
                UtcComboVisibility = Visibility.Collapsed,
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
                    UtcComboVisibility = Visibility.Collapsed,
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
                            individual.NameTime = tempTime.ToShortTimeString();
                            
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

            //update UI to a new menu Item 
            _timeDisplayIndex++;
            if (_timeDisplayIndex>=CheckedList.Count)
            {
                _timeDisplayIndex = -1;

                this.Dispatcher.Invoke(() =>
                {
                    MenuItems[0].NameTime = DateTime.Now.ToShortTimeString().PadLeft(8, '0');
                    MenuItems[0].ClockTextBlockText = " Local Time";
                    
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
            sortMenuItemsGroup();
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
                        nest.Insert(nest.Count, new MenuItemNest
                        {
                            ClockAddTimeZoneVisibility = Visibility.Collapsed,
                            CheckBoxIsChecked = "False",
                            ClockCheckBoxVisibility = Visibility.Visible,
                            TextBoxText = "newindividual",
                            ClockTextBoxVisibility = Visibility.Visible,
                            NameTime = DateTime.Now.ToShortTimeString(),
                            ClockTextBlockText = TimeZoneInfo.Local.DisplayName,
                            ClockTextBlockVisibility = Visibility.Collapsed,
                            NameTimeVisibility = Visibility.Visible,
                            DeleteButtonVisibility = Visibility.Visible,
                            MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                            TimeZoneInfo = TimeZoneInfo.Local,
                            //MenuItems = _timeZoneMenuItemNest,
                            UtcComboVisibility = Visibility.Visible,
                            utcStringList = utcComboList,
                            UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                        });
                        
                        sortMenuItemsIndividual(nest);
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
                        TextBoxText = "newgroup",
                        ClockTextBoxVisibility = Visibility.Visible,
                        NameTimeVisibility = Visibility.Collapsed,
                        ClockTextBlockText = "self",
                        ClockTextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local,
                        MenuItems = new(),
                        UtcComboVisibility = Visibility.Collapsed,
                    });

                    //add an item to the group
                    MenuItems[0].MenuItems[MenuItems[0].MenuItems.Count - 2].MenuItems.Add(new MenuItemNest
                    {
                        ClockAddTimeZoneVisibility = Visibility.Collapsed,
                        CheckBoxIsChecked = "False",
                        ClockCheckBoxVisibility = Visibility.Visible,
                        TextBoxText = "newindividual",
                        ClockTextBoxVisibility = Visibility.Visible,
                        NameTime = DateTime.Now.ToShortTimeString(),
                        NameTimeVisibility = Visibility.Visible,
                        ClockTextBlockText = TimeZoneInfo.Local.DisplayName,
                        ClockTextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        //MenuItems = _timeZoneMenuItemNest,
                        MenuLevel = MenuItemNest.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local,
                        UtcComboVisibility = Visibility.Visible,
                        utcStringList = utcComboList,
                        UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                    });

                    sortMenuItemsGroup();
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

        //private void Utc_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (sender is MenuItem menuItem)
        //    {
        //        if (menuItem.DataContext is MenuItemNest nest)
        //        {
        //            if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Utc)
        //            {
        //                _tempHeader = nest.ClockTextBlockText;
        //                _tempTimeZoneInfo = nest.TimeZoneInfo;
        //            }

        //            if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Individual)
        //            {
        //                nest.ClockTextBlockText = _tempHeader;
        //                nest.TimeZoneInfo = _tempTimeZoneInfo;
        //            }

        //            if (nest.MenuLevel == MenuItemNest.ClockMenuLevel.Group)
        //            {
        //               sortMenuItemsIndividual(nest.MenuItems);
        //               SaveMenuToSettings();
        //            }
        //        }
        //    }
        //}

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
                        //find the group that hte nest is in
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
        }

        private void TextBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBox box)
            {
                box.Foreground = Brushes.Black;
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

        public List<string> utcStringList { get; set; }
        public Visibility UtcComboVisibility { get; set; }
        private string _utcComboSelectedString{ get; set; } = "(UTC) Coordinated Universal Time";
        public string UtcComboSelectedString
        {
            get { return _utcComboSelectedString; }
            set
            {
                _utcComboSelectedString = value;
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
