using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.UserControls;

public class MenuItemNest : INotifyPropertyChanged
{
    #region Member Variables      

    #endregion //Member Variables


    #region Public Properties
    public enum ClockMenuLevel
    {
        Display,
        Group,
        Individual,
        Utc
    }


    public Visibility AddButtonVisibility { get; set; }
    public Visibility DeleteButtonVisibility { get; set; }
    public Visibility TextBoxVisibility { get; set; }
    public Visibility CheckBoxVisibility { get; set; }
    public TimeZoneInfo TimeZoneInfo { get; set; }
    public ClockMenuLevel MenuLevel { get; set; }
    public List<string> UtcStringList { get; set; }
    public ObservableCollection<MenuItemNest> MenuItems { get; set; }


    #endregion //Public Properties


    #region Observable Properties

    // ReSharper disable once InconsistentNaming
    private string _checkBoxIsChecked { get; set; }
    public string? CheckBoxIsChecked
    {
        get => _checkBoxIsChecked;
        set
        {
            _checkBoxIsChecked = value;
            OnPropertyChanged();
        }
    }


    private string _textBoxText { get; set; }
    public string TextBoxText
    {
        get => _textBoxText;
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
        get => _nameTime;
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
        get => _textBlockText;
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
        get => _utcComboSelectedString;
        set
        {
            _utcComboSelectedString = value;
            OnPropertyChanged();
        }
    }

    
    private string _groupName;
    public string GroupName
    {
        get => _groupName;
        set { _groupName = value; }
    }


    private Brush _foreground { get; set; } = Brushes.LimeGreen;
    public Brush Foreground
    {
        get => _foreground;
        set
        {
            _foreground = value;
            OnPropertyChanged();
        }
    }

    #endregion //Observable Properties


    #region Constructor
    public MenuItemNest()
    {

    }

    #endregion //Constructor


    #region Methods

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propName = null)
    {
        var handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propName));
        }
    }

    #endregion // Methods

}