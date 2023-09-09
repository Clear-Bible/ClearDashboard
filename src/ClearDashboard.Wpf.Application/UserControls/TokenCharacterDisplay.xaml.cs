using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.UserControls
{
    public partial class TokenCharacterDisplay : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Identifies the BackgroundColor1 dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColor1Property = DependencyProperty.Register(
            nameof(BackgroundColor1), typeof(Brush), typeof(TokenCharacterDisplay),
            new PropertyMetadata(Brushes.LightGray));
        
        /// <summary>
        /// Identifies the BackgroundColor2 dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColor2Property = DependencyProperty.Register(
            nameof(BackgroundColor2), typeof(Brush), typeof(TokenCharacterDisplay),
            new PropertyMetadata(Brushes.Transparent));
        
        /// <summary>
        /// Identifies the Threshold dependency property.
        /// </summary>
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register(
            nameof(Threshold), typeof(int), typeof(TokenCharacterDisplay),
            new PropertyMetadata(0, OnThresholdChanged));

        private static void OnThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TokenCharacterDisplay;
            control?.OnPropertyChanged(nameof(ComputedBackgroundColor));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
        /// Gets the character to display.
        /// </summary>
        public char Character => TokenCharacter?.Character ?? new char();

        /// <summary>
        /// Gets the <see cref="Brush"/> used to draw the background.
        /// </summary>
        public Brush ComputedBackgroundColor => TokenCharacter != null && TokenCharacter.Index < Threshold ? BackgroundColor1 : BackgroundColor2;

        /// <summary>
        /// Gets or sets the threshold used to determine the background color.
        /// </summary>
        public int Threshold
        {
            get => (int)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="TokenCharacterViewModel"/> to display.
        /// </summary>
        private TokenCharacterViewModel TokenCharacter => (TokenCharacterViewModel)DataContext;

        public TokenCharacterDisplay()
        {
            InitializeComponent();

            this.DataContextChanged += OnDataContextChanged;
            //var dataContext = DependencyPropertyDescriptor.FromProperty(DataContextProperty, typeof(FrameworkElement));
            //dataContext.AddValueChanged(this, OnDataContextChanged);
        }

        private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(Character));
            OnPropertyChanged(nameof(ComputedBackgroundColor));
        }
    }
}
