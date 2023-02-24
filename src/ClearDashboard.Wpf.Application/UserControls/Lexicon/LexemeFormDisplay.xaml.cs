﻿using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control for displaying a collection of Lexeme <see cref="Form"/> values.
    /// </summary>
    public partial class LexemeFormDisplay
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the LexemeFormRemoved routed event.
        /// </summary>
        public static readonly RoutedEvent LexemeFormRemovedEvent = EventManager.RegisterRoutedEvent
            ("LexemeFormRemoved", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeFormDisplay));
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Lexeme dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeProperty = DependencyProperty.Register(nameof(Lexeme), typeof(Lexeme), typeof(LexemeFormDisplay));

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(LexemeFormDisplay));

        /// <summary>
        /// Identifies the LexemeFormBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormBackgroundProperty = DependencyProperty.Register(nameof(LexemeFormBackground), typeof(SolidColorBrush), typeof(LexemeFormDisplay));

        /// <summary>
        /// Identifies the LexemeFormCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormCornerRadiusProperty = DependencyProperty.Register(nameof(LexemeFormCornerRadius), typeof(CornerRadius), typeof(LexemeFormDisplay),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the LexemeFormMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormMarginProperty = DependencyProperty.Register(nameof(LexemeFormMargin), typeof(Thickness), typeof(LexemeFormDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LexemeFormPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormPaddingProperty = DependencyProperty.Register(nameof(LexemeFormPadding), typeof(Thickness), typeof(LexemeFormDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LexemeForms dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormsProperty = DependencyProperty.Register(nameof(LexemeForms), typeof(LexemeFormCollection), typeof(LexemeFormDisplay));

        #endregion Static DependencyProperties
        #region Private event handlers

        private void RaiseLexemeFormEvent(RoutedEvent routedEvent, Form form)
        {
            RaiseEvent(new LexemeFormEventArgs()
            {
                RoutedEvent = routedEvent,
                Lexeme = Lexeme,
                Form = form
            });
        }

        private void OnRemoveLexemeForm(object sender, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var lexemeForm = control?.DataContext as Form;

            if (lexemeForm != null)
            {
                RaiseLexemeFormEvent(LexemeFormRemovedEvent, lexemeForm);
            }
        }

        #endregion
        #region Public properties

        /// <summary>
        /// Gets or sets the <see cref="LexemeViewModel"/> that the forms are associated with.
        /// </summary>
        public LexemeViewModel Lexeme
        {
            get => (LexemeViewModel) GetValue(LexemeProperty);
            set => SetValue(LexemeProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation for displaying the forms
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for individual form boxes.
        /// </summary>
        public Thickness LexemeFormMargin
        {
            get => (Thickness)GetValue(LexemeFormMarginProperty);
            set => SetValue(LexemeFormMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual form boxes.
        /// </summary>
        public Thickness LexemeFormPadding
        {
            get => (Thickness)GetValue(LexemeFormPaddingProperty);
            set => SetValue(LexemeFormPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush for individual form boxes.
        /// </summary>
        public SolidColorBrush LexemeFormBackground
        {
            get => (SolidColorBrush)GetValue(LexemeFormBackgroundProperty);
            set => SetValue(LexemeFormBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the corner radius for individual form boxes.
        /// </summary>
        public CornerRadius LexemeFormCornerRadius
        {
            get => (CornerRadius)GetValue(LexemeFormCornerRadiusProperty);
            set => SetValue(LexemeFormCornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="Form"/> objects to display in the control.
        /// </summary>
        public LexemeFormCollection LexemeForms
        {
            get => (LexemeFormCollection)GetValue(LexemeFormsProperty);
            set => SetValue(LexemeFormsProperty, value);
        }
        #endregion Public properties
        #region Public events
        /// <summary>
        /// Occurs when an new label is removed.
        /// </summary>
        public event RoutedEventHandler LexemeFormRemoved
        {
            add => AddHandler(LexemeFormRemovedEvent, value);
            remove => RemoveHandler(LexemeFormRemovedEvent, value);
        }
        #endregion

        public LexemeFormDisplay()
        {
            InitializeComponent();
        }
    }
}
