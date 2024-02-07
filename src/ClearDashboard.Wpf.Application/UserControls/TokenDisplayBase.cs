using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

namespace ClearDashboard.Wpf.Application.UserControls;

/// <summary>
/// A base class for derived token display controls.
/// </summary>
public class TokenDisplayBase : UserControl, IHandle<AlignmentAddedMessage>, IHandle<AlignmentDeletedMessage>
{
    #region Static DependencyProperties

    /// <summary>
    /// Identifies the CompositeIndicatorComputedColor dependency property.
    /// </summary>
    public static readonly DependencyProperty CompositeIndicatorComputedColorProperty = DependencyProperty.Register(
        nameof(CompositeIndicatorComputedColor), typeof(Brush), typeof(TokenDisplayBase),
        new PropertyMetadata(Brushes.LightGray));

    /// <summary>
    /// Identifies the CompositeIndicatorVisibility dependency property.
    /// </summary>
    public static readonly DependencyProperty CompositeIndicatorVisibilityProperty = DependencyProperty.Register(
        nameof(CompositeIndicatorVisibility), typeof(Visibility), typeof(TokenDisplayBase),
        new PropertyMetadata(Visibility.Visible));


    /// <summary>
    /// Identifies the TokenBorder dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenBorderProperty = DependencyProperty.Register(
        nameof(TokenBorder), typeof(Brush), typeof(TokenDisplayBase),
        new PropertyMetadata(Brushes.Transparent));


    /// <summary>
    /// Identifies the SurfaceText dependency property.
    /// </summary>
    public static readonly DependencyProperty SurfaceTextProperty =
        DependencyProperty.Register(nameof(SurfaceText), typeof(string), typeof(TokenDisplayBase));

    /// <summary>
    /// Identifies the TokenAlternateColor dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenAlternateColorProperty = DependencyProperty.Register(
        nameof(TokenAlternateColor), typeof(Brush), typeof(TokenDisplayBase),
        new PropertyMetadata(Brushes.DarkGray));

    /// <summary>
    /// Identifies the TokenFlowDirection dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenFlowDirectionProperty = DependencyProperty.Register(
        nameof(TokenFlowDirection), typeof(FlowDirection), typeof(TokenDisplayBase)
        //new PropertyMetadata(FlowDirection.LeftToRight)
        );

    /// <summary>
    /// Identifies the TokenFontFamily dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenFontFamilyProperty = DependencyProperty.Register(
        nameof(TokenFontFamily), typeof(FontFamily), typeof(TokenDisplayBase),
        new PropertyMetadata(new FontFamily(
            new Uri(
                "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"),
            ".Resources/Roboto/#Roboto")));

    /// <summary>
    /// Identifies the TokenFontSize dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenFontSizeProperty = DependencyProperty.Register(
        nameof(TokenFontSize), typeof(double), typeof(TokenDisplayBase),
        new PropertyMetadata(18d));


    /// <summary>
    /// Identifies the TokenForeground dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenForegroundProperty = DependencyProperty.Register(
        nameof(TokenForeground), typeof(Brush), typeof(TokenDisplayBase),
        new PropertyMetadata(Brushes.Black));

    /// <summary>
    /// Identifies the TokenMargin dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenMarginProperty = DependencyProperty.Register(nameof(TokenMargin),
        typeof(Thickness), typeof(TokenDisplayBase),
        new PropertyMetadata(new Thickness(0, 0, 0, 0)));

    #endregion Static DependencyProperties



    #region Private Event Handlers


    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TokenDisplayViewModel.PropertyChanged += TokenDisplayViewModelPropertyChanged;
        CalculateLayout();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (TokenDisplayViewModel is not null)
        {
            TokenDisplayViewModel.PropertyChanged -= TokenDisplayViewModelPropertyChanged;
        }
    }

    private void TokenDisplayViewModelPropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        CalculateLayout();
    }


    public async Task HandleAsync(AlignmentAddedMessage message, CancellationToken cancellationToken)
    {
        if (message.SourceTokenDisplayViewModel == TokenDisplayViewModel || message.TargetTokenDisplayViewModel == TokenDisplayViewModel)
        {
            CalculateLayout();
        }
        await Task.CompletedTask;
    }

    public async Task HandleAsync(AlignmentDeletedMessage message, CancellationToken cancellationToken)
    {
        if (message.Alignment.AlignedTokenPair.SourceToken.TokenId.IdEquals(TokenDisplayViewModel.AlignmentToken.TokenId) ||
            message.Alignment.AlignedTokenPair.TargetToken.TokenId.IdEquals(TokenDisplayViewModel.AlignmentToken.TokenId))
        {
            TokenDisplayViewModel.IsHighlighted = false;
            CalculateLayout();
        }
        await Task.CompletedTask;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the <see cref="EventAggregator"/> to be used for participating in the Caliburn Micro eventing system.
    /// </summary>
    public static IEventAggregator? EventAggregator { get; set; }


    /// <summary>
    /// Gets or sets the <see cref="Brush"/> used to draw the token border.
    /// </summary>
    /// <remarks>
    /// This property should not be set explicitly; it is computed from the token's selection status.
    /// </remarks>
    public Brush TokenBorder
    {
        get => (Brush)GetValue(TokenBorderProperty);
        protected set => SetValue(TokenBorderProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> used to draw the composite token indicator.
    /// </summary>
    /// <remarks>
    /// This should not be set explicitly; it is computed based on whether the token is part of a composite token.
    /// </remarks>
    public Brush CompositeIndicatorComputedColor
    {
        get => (Brush)GetValue(CompositeIndicatorComputedColorProperty);
        protected set => SetValue(CompositeIndicatorComputedColorProperty, value);
    }


    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> of the composite indicator.
    /// </summary>
    /// <remarks>
    /// This should  not be set explicitly; it is computed based on whether the token is part of a composite token.
    /// </remarks>
    public Visibility CompositeIndicatorVisibility
    {
        get => (Visibility)GetValue(CompositeIndicatorVisibilityProperty);
        protected set => SetValue(CompositeIndicatorVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the surface text to be displayed.
    /// </summary>
    /// <remarks>
    /// This should not be set directly; it is computed based on the orientation of the display.
    /// </remarks>
    public string SurfaceText
    {
        get => (string)GetValue(SurfaceTextProperty);
        protected set => SetValue(SurfaceTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the alternate <see cref="Brush"/> used to draw the token in an alignment view when
    /// it does not participate in an alignment pair.
    /// </summary>
    public Brush TokenAlternateColor
    {
        get => (Brush)GetValue(TokenAlternateColorProperty);
        set => SetValue(TokenAlternateColorProperty, value);
    }

    /// <summary>
    /// Gets the strongly-typed <see cref="TokenDisplayViewModel"/> data source for this control.
    /// </summary>
    public TokenDisplayViewModel TokenDisplayViewModel => (TokenDisplayViewModel)DataContext;

    /// <summary>
    /// Gets or sets the <see cref="FlowDirection"/> to use for displaying the tokens.
    /// </summary>
    public FlowDirection TokenFlowDirection
    {
        get => (FlowDirection)GetValue(TokenFlowDirectionProperty);
        set => SetValue(TokenFlowDirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="FontFamily"/> to use for displaying the token.
    /// </summary>
    public FontFamily TokenFontFamily
    {
        get => (FontFamily)GetValue(TokenFontFamilyProperty);
        set => SetValue(TokenFontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size for the token.
    /// </summary>
    public double TokenFontSize
    {
        get => (double)GetValue(TokenFontSizeProperty);
        set => SetValue(TokenFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> used to draw the token.
    /// </summary>
    /// <remarks>
    /// This property should not be set explicitly; it is computed from the token's alignment status.
    /// </remarks>
    public Brush TokenForeground
    {
        get => (Brush)GetValue(TokenForegroundProperty);
        set => SetValue(TokenForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin around each token for display.
    /// </summary>
    /// <remarks>
    /// This property should not be set explicitly; it is computed from the token horizontal and vertical spacing.
    /// </remarks>
    public Thickness TokenMargin
    {
        get => (Thickness)GetValue(TokenMarginProperty);
        protected set => SetValue(TokenMarginProperty, value);
    }


    #endregion Public Properties

    private double HorizontalSpacing => 5d;

    protected virtual void CalculateLayout()
    {
        var tokenLeftMargin = TokenDisplayViewModel.PaddingBefore.Length * HorizontalSpacing;
        var tokenRightMargin = TokenDisplayViewModel.PaddingAfter.Length * HorizontalSpacing;

        CompositeIndicatorVisibility = TokenDisplayViewModel.IsCompositeTokenMember ? Visibility.Visible : Visibility.Hidden;
        CompositeIndicatorComputedColor = TokenDisplayViewModel.CompositeIndicatorColor;

        TokenBorder = TokenDisplayViewModel.IsHighlighted ? TokenDisplayViewModel.IsManualAlignment ? Brushes.MediumOrchid : Brushes.Aquamarine : Brushes.Transparent;
        TokenForeground = TokenDisplayViewModel.VerseDisplay is AlignmentDisplayViewModel
            ? TokenDisplayViewModel.IsAligned ? Brushes.Black : TokenAlternateColor
            : Brushes.Black;
        TokenMargin = new Thickness(tokenLeftMargin, 0, tokenRightMargin, 0);
        SurfaceText = TokenDisplayViewModel.SurfaceText;
    }

    public TokenDisplayBase()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        EventAggregator?.SubscribeOnUIThread(this);
        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void Dispatcher_ShutdownStarted(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;

        EventAggregator?.Unsubscribe(this);
    }

    //~TokenDisplayBase()
    //{

    //    // Prevent errors related to the destructor getting called on a non-UI thread.
    //    Execute.OnUIThread(() =>
    //    {
    //        Loaded -= OnLoaded;
    //        Unloaded -= OnUnloaded;

    //        EventAggregator?.Unsubscribe(this);
    //    });
       
    //}

}