using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
using ClearDashboard.Wpf.Application.Views.Lexicon;

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
        public static DependencyProperty SplitTokenDialogViewModelProperty = DependencyProperty.Register
            (nameof(SplitTokenDialogViewModel), typeof(SplitTokenDialogViewModel), typeof(GlossListDropdown));

        public GlossListDropdown()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
           base.OnInitialized(e);
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
        /// Gets or sets the <see cref="SplitTokenDialogViewModel"/> split token display information to display in this control.
        /// </summary>
        public SplitTokenDialogViewModel SplitTokenDialogViewModel
        {
            get => (SplitTokenDialogViewModel)GetValue(SplitTokenDialogViewModelProperty);
            set => SetValue(SplitTokenDialogViewModelProperty, value);
        }

        private async void DropdownToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var toggleButton = (ToggleButton)sender;
            var parent = toggleButton.Parent;
            var popup = parent.FindChild<Popup>();
            var border = popup.Child as Border;
            var lexiconDialogView = border.Child as LexiconDialogView;
            var splitInstructionViewModel = this.DataContext as SplitInstructionViewModel;
            var v = this.LexiconDialogView;
            RaiseEvent(new SplitTokenEventArgs(DropDownOpeningEvent, this, lexiconDialogView, splitInstructionViewModel ));
        }
    }

    public class SplitTokenEventArgs : RoutedEventArgs
    {
        public SplitTokenEventArgs(RoutedEvent routedEvent, LexiconDialogView lexiconDialogView, SplitInstructionViewModel splitInstructionViewModel) : base(routedEvent)
        {
            LexiconDialogView = lexiconDialogView;
            SplitInstructionViewModel = splitInstructionViewModel;
        }

        public SplitTokenEventArgs(RoutedEvent routedEvent, object source, LexiconDialogView lexiconDialogView, SplitInstructionViewModel splitInstructionViewModel) : base(routedEvent, source)
        {
            LexiconDialogView = lexiconDialogView;
            SplitInstructionViewModel = splitInstructionViewModel;
        }

        
        public LexiconDialogView? LexiconDialogView { get; set; }

        public SplitInstructionViewModel SplitInstructionViewModel { get; set; }
    }
}
