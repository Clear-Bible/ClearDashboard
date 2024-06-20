using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// Interaction logic for GlossListDropdown.xaml
    /// </summary>
    public partial class GlossListDropdown : UserControl
    {

        public static readonly RoutedEvent DropDownOpeningEvent = EventManager.RegisterRoutedEvent
            (nameof(DropDownOpening), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GlossListDropdown));

        /// <summary>
        /// Identifies the SplitTokenDialogViewModel dependency property.
        /// </summary>
        public static readonly DependencyProperty SplitTokenDialogViewModelProperty = DependencyProperty.Register(nameof(SplitTokenDialogViewModel), typeof(SplitTokenDialogViewModel), typeof(GlossListDropdown));

        public GlossListDropdown()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when a new semantic domain is added.
        /// </summary>
        public event RoutedEventHandler DropDownOpening
        {
            add => AddHandler(DropDownOpeningEvent, value);
            remove => RemoveHandler(DropDownOpeningEvent, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TokenDisplayViewModel"/> token display information to display in this control.
        /// </summary>
        public SplitTokenDialogViewModel SplitTokenDialogViewModel
        {
            get => (SplitTokenDialogViewModel)GetValue(SplitTokenDialogViewModelProperty);
            set => SetValue(SplitTokenDialogViewModelProperty, value);
        }

        private void DropdownToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new SplitTokenEventArgs(DropDownOpeningEvent, this, SplitTokenDialogViewModel));
        }
    }

    public class SplitTokenEventArgs : RoutedEventArgs
    {
        public SplitTokenEventArgs(RoutedEvent routedEvent, SplitTokenDialogViewModel splitTokenDialogViewModel) : base(routedEvent)
        {
            SplitTokenDialogViewModel = splitTokenDialogViewModel;
        }

        public SplitTokenEventArgs(RoutedEvent routedEvent, object source, SplitTokenDialogViewModel splitTokenDialogViewModel) : base(routedEvent, source)
        {
            SplitTokenDialogViewModel = splitTokenDialogViewModel;
        }

        public SplitTokenDialogViewModel? SplitTokenDialogViewModel { get; set; }
    }
}
