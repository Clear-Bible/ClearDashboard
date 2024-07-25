using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.Views.Lexicon;
using MahApps.Metro.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// Interaction logic for GlossListDropdown.xaml
    /// </summary>
    public partial class GlossListDropdown : UserControl, IHandle<GlossSetMessage>
    {

        public static readonly RoutedEvent DropDownOpeningEvent = EventManager.RegisterRoutedEvent
            (nameof(DropDownOpening), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GlossListDropdown));

        /// <summary>
        /// Identifies the SplitTokenDialogViewModel dependency property.
        /// </summary>
        public static DependencyProperty SplitTokenDialogViewModelProperty = DependencyProperty.Register
            (nameof(SplitTokenDialogViewModel), typeof(SplitTokenDialogViewModel), typeof(GlossListDropdown));

        /// <summary>
        /// Identifies the SplitTokenDialogViewModel dependency property.
        /// </summary>
        public static DependencyProperty TokenTextProperty = DependencyProperty.Register
            (nameof(TokenText), typeof(string), typeof(GlossListDropdown));

        private IEventAggregator? _eventAggregator;
        public GlossListDropdown()
        {
            InitializeComponent();

           

            Loaded += (sender, args) =>
            {
                _eventAggregator = IoC.Get<IEventAggregator>();
                _eventAggregator.SubscribeOnPublishedThread(this);

                Window.GetWindow(this)!.Closing += (o, eventArgs) =>
                {
                    _eventAggregator.Unsubscribe(this);
                };
            };
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

        public string TokenText
        {
            get => (string)GetValue(TokenTextProperty);
            set => SetValue(TokenTextProperty, value);
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
            if (toggleButton.IsChecked != null && toggleButton.IsChecked.Value)
            {
                RaiseEvent(splitInstructionViewModel != null
                    ? new SplitTokenEventArgs(DropDownOpeningEvent, this, lexiconDialogView, splitInstructionViewModel)
                    : new SplitTokenEventArgs(DropDownOpeningEvent, this, lexiconDialogView, TokenText, TokenText));
            }
        }

        public async Task HandleAsync(GlossSetMessage message, CancellationToken cancellationToken)
        {
            if (message.Translation != null)
            {
                TokenText = message.Translation.Text!;
            }

            DropdownToggleButton.IsChecked = !DropdownToggleButton.IsChecked;

            await Task.CompletedTask;
        }
    }

    public class SplitTokenEventArgs : RoutedEventArgs
    {
        public SplitTokenEventArgs(RoutedEvent routedEvent, LexiconDialogView lexiconDialogView, SplitInstructionViewModel splitInstructionViewModel) : base(routedEvent)
        {
            LexiconDialogView = lexiconDialogView;
            SplitInstructionViewModel = splitInstructionViewModel;
        }

        public SplitTokenEventArgs(RoutedEvent routedEvent, object source, LexiconDialogView lexiconDialogView, SplitInstructionViewModel? splitInstructionViewModel = null) : base(routedEvent, source)
        {
            LexiconDialogView = lexiconDialogView;
            SplitInstructionViewModel = splitInstructionViewModel;
            TrainingText = splitInstructionViewModel?.TrainingText;
            TokenText = splitInstructionViewModel?.TokenText;
        }

        public SplitTokenEventArgs(RoutedEvent routedEvent, object source, LexiconDialogView lexiconDialogView,
            string? tokenText = null, string? trainingText = null) : base(routedEvent, source)
        {
            LexiconDialogView = lexiconDialogView;
            TrainingText = trainingText;
            TokenText = tokenText;
        }


        public LexiconDialogView? LexiconDialogView { get; set; }

        public SplitInstructionViewModel? SplitInstructionViewModel { get; set; }

        public bool HasSplitInstruction => SplitInstructionViewModel != null;

        public bool HasTokenAndTrainingText => !string.IsNullOrEmpty(TrainingText) && !string.IsNullOrEmpty(TokenText);

        public string? TrainingText { get; set; }
        public string? TokenText { get; set; }
    }
}
