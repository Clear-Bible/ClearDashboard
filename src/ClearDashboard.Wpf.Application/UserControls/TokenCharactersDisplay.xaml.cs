using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.UserControls
{
    public partial class TokenCharactersDisplay
    {
        /// <summary>
        /// Identifies the CharacterClicked routed event.
        /// </summary>
        public static readonly RoutedEvent CharacterClickedEvent = EventManager.RegisterRoutedEvent
            (nameof(CharacterClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenCharactersDisplay));

        /// <summary>
        /// Identifies the BackgroundColor1 dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColor1Property = DependencyProperty.Register(
            nameof(BackgroundColor1), typeof(Brush), typeof(TokenCharactersDisplay),
            new PropertyMetadata(Brushes.LightGray));
        
        /// <summary>
        /// Identifies the BackgroundColor2 dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColor2Property = DependencyProperty.Register(
            nameof(BackgroundColor2), typeof(Brush), typeof(TokenCharactersDisplay),
            new PropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Identifies the CharacterWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty CharacterWidthProperty = DependencyProperty.Register(
            nameof(CharacterWidth), typeof(double), typeof(TokenCharactersDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the Threshold dependency property.
        /// </summary>
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register(
            nameof(Threshold), typeof(int), typeof(TokenCharactersDisplay));

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the background when it is less than the threshold.
        /// </summary>
        public Brush BackgroundColor1
        {
            get => (Brush)GetValue(BackgroundColor1Property);
            set => SetValue(BackgroundColor1Property, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the background when it is greater than or equal to the threshold.
        /// </summary>
        public Brush BackgroundColor2
        {
            get => (Brush)GetValue(BackgroundColor2Property);
            set => SetValue(BackgroundColor2Property, value);
        }

        /// <summary>
        /// Gets or sets the width of each character.
        /// </summary>
        public double CharacterWidth
        {
            get => (double)GetValue(CharacterWidthProperty);
            set => SetValue(CharacterWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the threshold used to determine the background color.
        /// </summary>
        public int Threshold
        {
            get => (int)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, value);
        }

        /// <summary>
        /// Occurs when an individual character is clicked.
        /// </summary>
        public event RoutedEventHandler CharacterClicked
        {
            add => AddHandler(CharacterClickedEvent, value);
            remove => RemoveHandler(CharacterClickedEvent, value);
        }

        private void RaiseTokenCharacterEvent(RoutedEvent routedEvent, TokenCharacterEventArgs e)
        {
            RaiseEvent(new TokenCharacterEventArgs
            {
                RoutedEvent = routedEvent,
                TokenCharacter = e.TokenCharacter,
            });
        }

        private void OnCharacterClicked(object sender, RoutedEventArgs args)
        {
            RaiseTokenCharacterEvent(CharacterClickedEvent, args as TokenCharacterEventArgs);
        }

        /// <summary>
        /// Gets the collection <see cref="TokenCharacterViewModel"/> to display.
        /// </summary>
        private TokenCharacterViewModelCollection TokenCharacters => (TokenCharacterViewModelCollection)DataContext;

        public TokenCharactersDisplay()
        {
            InitializeComponent();
        }
    }
}
