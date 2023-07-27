using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using SIL.Extensions;

namespace ClearDashboard.Wpf.Application.UserControls;

/// <summary>
/// A base class vor verse display controls.
/// </summary>
public class VerseDisplayBase : UserControl, INotifyPropertyChanged, IHandle<TokensUpdatedMessage>
{

    #region Static dependency properties

    /// <summary>
    /// Identifies the SourceFontFamily dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceFontFamilyProperty = DependencyProperty.Register(nameof(SourceFontFamily), typeof(FontFamily), typeof(VerseDisplayBase),
        new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

    /// <summary>
    /// Identifies the SourceFontSize dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceFontSizeProperty = DependencyProperty.Register(nameof(SourceFontSize), typeof(double), typeof(VerseDisplayBase),
        new PropertyMetadata(18d));

    /// <summary>
    /// Identifies the TargetFontFamily dependency property.
    /// </summary>
    public static readonly DependencyProperty TargetFontFamilyProperty = DependencyProperty.Register(nameof(TargetFontFamily), typeof(FontFamily), typeof(VerseDisplayBase),
        new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

    /// <summary>
    /// Identifies the TargetFontSize dependency property.
    /// </summary>
    public static readonly DependencyProperty TargetFontSizeProperty = DependencyProperty.Register(nameof(TargetFontSize), typeof(double), typeof(VerseDisplayBase),
        new PropertyMetadata(16d));

    /// <summary>
    /// Identifies the TokenVerticalSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty TokenVerticalSpacingProperty = DependencyProperty.Register(nameof(TokenVerticalSpacing), typeof(double), typeof(VerseDisplayBase),
        new PropertyMetadata(4d));

    /// <summary>
    /// Identifies the VerseBackground dependency property.
    /// </summary>
    public static readonly DependencyProperty VerseBackgroundProperty = DependencyProperty.Register(nameof(VerseBackground), typeof(Brush), typeof(VerseDisplayBase),
        new PropertyMetadata(Brushes.AliceBlue));

    /// <summary>
    /// Identifies the VerseBorderBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty VerseBorderBrushProperty = DependencyProperty.Register(nameof(VerseBorderBrush), typeof(Brush), typeof(VerseDisplayBase),
        new PropertyMetadata(Brushes.Black));

    /// <summary>
    /// Identifies the VerseBorderThickness dependency property.
    /// </summary>
    public static readonly DependencyProperty VerseBorderThicknessProperty = DependencyProperty.Register(nameof(VerseBorderThickness), typeof(Thickness), typeof(VerseDisplayBase),
        new PropertyMetadata(new Thickness(1)));

    /// <summary>
    /// Identifies the VerseMargin dependency property.
    /// </summary>
    public static readonly DependencyProperty VerseMarginProperty = DependencyProperty.Register(nameof(VerseMargin), typeof(Thickness), typeof(VerseDisplayBase),
        new PropertyMetadata(new Thickness(0, 10, 0, 10)));

    /// <summary>
    /// Identifies the VersePadding dependency property.
    /// </summary>
    public static readonly DependencyProperty VersePaddingProperty = DependencyProperty.Register(nameof(VersePadding), typeof(Thickness), typeof(VerseDisplayBase),
        new PropertyMetadata(new Thickness(10)));

    #endregion Static dependency properties
    #region Private event handlers

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task HandleAsync(TokensUpdatedMessage message, CancellationToken cancellationToken)
    {
        OnPropertyChanged(nameof(SourceTokens));
        OnPropertyChanged(nameof(TargetTokens));
        await Task.CompletedTask;
    }



    // ReSharper restore UnusedMember.Global

    #endregion
    #region Public events



    /// <summary>
    /// Occurs when a property is changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion
    #region Public properties

    /// <summary>
    /// Gets or sets the <see cref="IEventAggregator"/> that this control instance can use to listen for events.
    /// </summary>
    public static IEventAggregator? EventAggregator { get; set; }

    /// <summary>
    /// Gets the <see cref="FlowDirection"/> for the source tokens.
    /// </summary>
    public FlowDirection SourceFlowDirection => VerseDisplayViewModel is { IsSourceRtl: true } ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    /// <summary>
    /// Gets or sets the <see cref="FontFamily"/> to use for displaying the token.
    /// </summary>
    public FontFamily SourceFontFamily
    {
        get => (FontFamily)GetValue(SourceFontFamilyProperty);
        set => SetValue(SourceFontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size for the token.
    /// </summary>
    public double SourceFontSize
    {
        get => (double)GetValue(SourceFontSizeProperty);
        set => SetValue(SourceFontSizeProperty, value);
    }

    /// <summary>
    /// Gets the collection of <see cref="TokenDisplayViewModel"/> source objects to display in the control.
    /// </summary>
    public IEnumerable SourceTokens => VerseDisplayViewModel != null ? VerseDisplayViewModel.SourceTokenDisplayViewModels : new TokenDisplayViewModelCollection();

    /// <summary>
    /// Gets the <see cref="FlowDirection"/> for the target tokens.
    /// </summary>
    public FlowDirection TargetFlowDirection => VerseDisplayViewModel is { IsTargetRtl: true } ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    /// <summary>
    /// Gets or sets the <see cref="FontFamily"/> to use for displaying the target tokens.
    /// </summary>
    public FontFamily TargetFontFamily
    {
        get => (FontFamily)GetValue(TargetFontFamilyProperty);
        set => SetValue(TargetFontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size for the target tokens.
    /// </summary>
    public double TargetFontSize
    {
        get => (double)GetValue(TargetFontSizeProperty);
        set => SetValue(TargetFontSizeProperty, value);
    }

    /// <summary>
    /// Gets the collection of <see cref="TokenDisplayViewModel"/> target objects to display in the control.
    /// </summary>
    public TokenDisplayViewModelCollection TargetTokens => VerseDisplayViewModel != null ? VerseDisplayViewModel.TargetTokenDisplayViewModels : new TokenDisplayViewModelCollection();

    /// <summary>
    /// Gets or sets the visibility of the target (alignment) verse.
    /// </summary>
    public Visibility TargetVisibility => TargetTokens.Any() ? Visibility.Visible : Visibility.Collapsed;


    /// <summary>
    /// Gets or sets the vertical spacing below the token.
    /// </summary>
    public double TokenVerticalSpacing
    {
        get => (double)GetValue(TokenVerticalSpacingProperty);
        set => SetValue(TokenVerticalSpacingProperty, value);
    }



    /// <summary>
    /// Gets or sets the <see cref="Brush"/> used to draw the background of the tokens list.
    /// </summary>
    public Brush VerseBackground
    {
        get => (Brush)GetValue(VerseBackgroundProperty);
        set => SetValue(VerseBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> used to draw the border around the tokens list.
    /// </summary>
    public Brush VerseBorderBrush
    {
        get => (Brush)GetValue(VerseBorderBrushProperty);
        set => SetValue(VerseBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness for the tokens list.
    /// </summary>
    public Thickness VerseBorderThickness
    {
        get => (Thickness)GetValue(VerseBorderThicknessProperty);
        set => SetValue(VerseBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets the strongly-typed VerseDisplayViewModel bound to this control.
    /// </summary>
    public VerseDisplayViewModel VerseDisplayViewModel => (DataContext?.GetType().Name != "NamedObject" ? DataContext as VerseDisplayViewModel : null)!;

    /// <summary>
    /// Gets or sets the margin for the tokens list.
    /// </summary>
    public Thickness VerseMargin
    {
        get => (Thickness)GetValue(VerseMarginProperty);
        set => SetValue(VerseMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets a collection of <see cref="TokenDisplayViewModel"/> objects that are selected in the UI for this verse.
    /// </summary>
    public TokenDisplayViewModelCollection VerseSelectedTokens { get; set; } = new();

    /// <summary>
    /// Gets or sets the padding for the tokens list.
    /// </summary>
    public Thickness VersePadding
    {
        get => (Thickness)GetValue(VersePaddingProperty);
        set => SetValue(VersePaddingProperty, value);
    }


    #endregion Public properties

  

    public VerseDisplayBase()
    {
      

        if (EventAggregator != null)
        {
            EventAggregator.SubscribeOnUIThread(this);
        }
    }
}