using ClearDashboard.Wpf.Application.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Xaml.Behaviors;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for ClockUserControl.xaml
    /// </summary>
    public partial class ClockUserControl : INotifyPropertyChanged
    {
        private readonly ILogger _logger;

        #region Member Variables      

        private List<string> utcComboList = new();

        private Timer _refreshTimer = new Timer(3000);

        private int _timeDisplayIndex = 0;

        private ReadOnlyCollection<TimeZoneInfo> _timezones = TimeZoneInfo.GetSystemTimeZones();

        private string _defaultIndividualText = "new individual";

        private string _defaultGroupText = "new group";

        #endregion //Member Variables


        #region Public Properties

        public ObservableCollection<ClockMenuItem> CheckedList { get; set; }

        #endregion //Public Properties


        #region Observable Properties

        private ObservableCollection<ClockMenuItem> _menuItems;
        public ObservableCollection<ClockMenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                OnPropertyChanged();
            }
        }

        #endregion //Observable Properties


        #region Constructor


        public ClockUserControl()
        {
            InitializeComponent();
            DataContext = this;

            //Construct ClockMenuItem of TimeZones
            foreach (var timezone in _timezones)
            {
                utcComboList.Add(timezone.DisplayName);
            }

            //Construct Individual ClockMenuItems
            ObservableCollection<ClockMenuItem> SettingsClockMenuItemList = new();

            StringCollection SettingsStringCollection = new StringCollection();
            SettingsStringCollection = Properties.Settings.Default.TimeZones;
            if (SettingsStringCollection != null)
            {
                foreach (var group in SettingsStringCollection)
                {
                    //menuItems = new ObservableCollection<ClockMenuItem>
                    var groupArr = group.Split(";");
                    ObservableCollection<ClockMenuItem> groupClockMenuItemList = new();

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
                                    groupClockMenuItemList.Add(new ClockMenuItem
                                    {
                                        AddButtonVisibility = Visibility.Collapsed,
                                        CheckBoxIsChecked = individualArr[0],
                                        CheckBoxVisibility = Visibility.Visible,
                                        TextBoxText = individualArr[1],
                                        Placeholder = _defaultIndividualText,
                                        TextBoxVisibility = Visibility.Visible,
                                        NameTime = TimeZoneInfo.ConvertTime(DateTime.Now, timezone).ToShortTimeString(),
                                        NameTimeVisibility = Visibility.Visible,
                                        TextBlockText = individualArr[2],
                                        TextBlockVisibility = Visibility.Collapsed,
                                        DeleteButtonVisibility = Visibility.Visible,
                                        GroupName = individualArr[3],
                                        MenuLevel = ClockMenuItem.ClockMenuLevel.Individual,
                                        TimeZoneInfo = timezone,
                                        //MenuItems = _timeZoneClockMenuItemList,
                                        UtcStringList = utcComboList,
                                        UtcComboVisibility = Visibility.Visible,
                                        UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                                    });
                                }
                            }
                        }
                    }

                    var selfArr = groupArr[0].Split(",");
                    ClockMenuItem groupMenuItem = new ClockMenuItem
                    {
                        AddButtonVisibility = Visibility.Visible,
                        CheckBoxIsChecked = selfArr[0],
                        CheckBoxVisibility = Visibility.Visible,
                        TextBoxText = selfArr[1],
                        Placeholder = _defaultGroupText,
                        TextBoxVisibility = Visibility.Visible,
                        NameTimeVisibility = Visibility.Collapsed,
                        TextBlockText = selfArr[2],
                        TextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        GroupName = selfArr[3],
                        MenuLevel = ClockMenuItem.ClockMenuLevel.Group,
                        MenuItems = groupClockMenuItemList,
                        UtcComboVisibility = Visibility.Collapsed,
                    };

                    SettingsClockMenuItemList.Add(groupMenuItem);
                }

            }

            SettingsClockMenuItemList.Add(new ClockMenuItem
            {
                AddButtonVisibility = Visibility.Visible,
                CheckBoxVisibility = Visibility.Collapsed,
                TextBoxVisibility = Visibility.Collapsed,
                NameTimeVisibility = Visibility.Collapsed,
                DeleteButtonVisibility = Visibility.Hidden,
                TextBlockVisibility = Visibility.Collapsed,
                MenuLevel = ClockMenuItem.ClockMenuLevel.Group,
                UtcComboVisibility = Visibility.Collapsed,
            });

            MenuItems = new ObservableCollection<ClockMenuItem>
            {
                new ClockMenuItem {

                    AddButtonVisibility = Visibility.Collapsed,
                    CheckBoxVisibility=Visibility.Collapsed,
                    TextBoxVisibility = Visibility.Collapsed,
                    NameTimeVisibility = Visibility.Visible,
                    TextBlockVisibility = Visibility.Visible,
                    DeleteButtonVisibility = Visibility.Collapsed,
                    MenuLevel = ClockMenuItem.ClockMenuLevel.Display,
                    MenuItems = SettingsClockMenuItemList,
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

        #endregion //Constructor


        #region Methods

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
            if (_timeDisplayIndex >= CheckedList.Count)
            {
                _timeDisplayIndex = -1;

                SetClockToLocalTime();
            }
            else
            {
                SetDisplayClockFromCheckedList();
            }
        }

        private void SetClockToLocalTime()
        {
            try
            {
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private string? GetLocalizedLocalTimeString()
        {
            var localization = IoC.Get<ILocalizationService>();
            return (localization != null)
                ? localization.Get("ClockUserControl_LocalTime")
                : throw new NullReferenceException("ILocalizationService has not been registered with the DI container.");

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
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (_timeDisplayIndex >= 0)
                    {
                        MenuItems[0].NameTime = CheckedList[_timeDisplayIndex].NameTime;

                        var displayText = CheckedList[_timeDisplayIndex].TextBoxText;
                        if (displayText == string.Empty)
                        {
                            displayText = _defaultIndividualText;
                        }

                        MenuItems[0].TextBlockText = " " + displayText;
                        MenuItems[0].Foreground = CheckedList[_timeDisplayIndex].Foreground;
                    }
                });
            }
            catch (Exception ex)
            {
                if (ex.Message != "A task was canceled.")
                {
                    SetClockToLocalTime();
                }
            }
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            SortMenuItemsGroup();
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
                    if (button.Tag is ObservableCollection<ClockMenuItem> list)
                    {
                        list.Insert(list.Count, new ClockMenuItem
                        {
                            AddButtonVisibility = Visibility.Collapsed,
                            CheckBoxIsChecked = "False",
                            CheckBoxVisibility = Visibility.Visible,
                            Placeholder = _defaultIndividualText,
                            TextBoxVisibility = Visibility.Visible,
                            NameTime = DateTime.Now.ToShortTimeString(),
                            TextBlockText = TimeZoneInfo.Local.DisplayName,
                            TextBlockVisibility = Visibility.Collapsed,
                            NameTimeVisibility = Visibility.Visible,
                            DeleteButtonVisibility = Visibility.Visible,
                            MenuLevel = ClockMenuItem.ClockMenuLevel.Individual,
                            TimeZoneInfo = TimeZoneInfo.Local,
                            //MenuItems = _timeZoneClockMenuItemList,
                            UtcComboVisibility = Visibility.Visible,
                            UtcStringList = utcComboList,
                            UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                        });

                        SortMenuItemsIndividual(list);
                        InstantClockRefresh();
                        SaveMenuToSettings();
                    }
                }
                else//Add Group
                {
                    MenuItems[0].MenuItems.Insert(MenuItems[0].MenuItems.Count - 1, new ClockMenuItem
                    {
                        AddButtonVisibility = Visibility.Visible,
                        CheckBoxIsChecked = "False",
                        CheckBoxVisibility = Visibility.Visible,
                        Placeholder = _defaultGroupText,
                        TextBoxVisibility = Visibility.Visible,
                        NameTimeVisibility = Visibility.Collapsed,
                        TextBlockText = "self",
                        TextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        MenuLevel = ClockMenuItem.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local,
                        MenuItems = new(),
                        UtcComboVisibility = Visibility.Collapsed,
                    });

                    //add an item to the group
                    MenuItems[0].MenuItems[MenuItems[0].MenuItems.Count - 2].MenuItems.Add(new ClockMenuItem
                    {
                        AddButtonVisibility = Visibility.Collapsed,
                        CheckBoxIsChecked = "False",
                        CheckBoxVisibility = Visibility.Visible,
                        Placeholder = _defaultIndividualText,
                        TextBoxVisibility = Visibility.Visible,
                        NameTime = DateTime.Now.ToShortTimeString(),
                        NameTimeVisibility = Visibility.Visible,
                        TextBlockText = TimeZoneInfo.Local.DisplayName,
                        TextBlockVisibility = Visibility.Collapsed,
                        DeleteButtonVisibility = Visibility.Visible,
                        //MenuItems = _timeZoneClockMenuItemList,
                        MenuLevel = ClockMenuItem.ClockMenuLevel.Individual,
                        TimeZoneInfo = TimeZoneInfo.Local,
                        UtcComboVisibility = Visibility.Visible,
                        UtcStringList = utcComboList,
                        UtcComboSelectedString = TimeZoneInfo.Local.DisplayName
                    });
                    
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
                List<ClockMenuItem> removeIndividualList = new();
                List<ClockMenuItem> removeGroupList = new();

                foreach (var group in MenuItems[0].MenuItems)
                {
                    if (group.DeleteButtonVisibility == Visibility.Collapsed)
                    {
                        removeGroupList.Add(group);
                    }

                    if (group.MenuItems != null)
                    {
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

                    if (comboBox.DataContext is ClockMenuItem item)
                    {
                        //find the group that the item is in
                        foreach (var group in MenuItems[0].MenuItems)
                        {
                            if (group.MenuItems != null)
                            {
                                foreach (var individual in group.MenuItems)
                                {
                                    if (individual == item)
                                    {
                                        SortMenuItemsIndividual(group.MenuItems);
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

        private void SortMenuItemsIndividual(ObservableCollection<ClockMenuItem> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                for (int j = 0; j < collection.Count - 1; j++)
                {
                    if (collection[j].TimeZoneInfo.BaseUtcOffset.CompareTo(
                            collection[j + 1].TimeZoneInfo.BaseUtcOffset) > 0)
                    {
                        collection.Move(j, j + 1);
                    }
                }
            }
        }

        private void SortMenuItemsGroup()
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
                    if (MenuItems[0].MenuItems[j].TextBoxText == string.Empty)
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
            if (sender is ComboBox cbox)
            {
                cbox.Background = Brushes.White;
            }
        }

        private void TextBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ComboBox cbox)
            {
                cbox.Background = Brushes.Transparent;
            }
        }

        #endregion // Methods



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

    class DoubleClickBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDoubleClick += AssociatedObjectMouseDoubleClick;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDoubleClick -= AssociatedObjectMouseDoubleClick;
            base.OnDetaching();
        }

        private void AssociatedObjectMouseDoubleClick(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.SelectAll();
        }
    }

    public class TextBoxEnterKeyUpdateBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.Key == Key.Return)
                {
                    if (e.Key == Key.Enter)
                    {
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
        }
    }
}
