using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for TranslationControl.xaml
    /// </summary>
    public partial class TranslationControl : UserControl
    {
        /// <summary>
        /// Identifies the TokenDisplayProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenDisplayProperty = DependencyProperty.Register("TokenDisplay", typeof(TokenDisplay), typeof(TranslationControl));

        /// <summary>
        /// Identifies the TranslationOptions dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationOptionsProperty = DependencyProperty.Register("TranslationOptions", typeof(IEnumerable), typeof(TranslationControl));

        /// <summary>
        /// Gets or sets the <see cref="TokenDisplay"/> token display information to display in this control.
        /// </summary>
        public TokenDisplay TokenDisplay
        {
            get => (TokenDisplay)GetValue(TokenDisplayProperty);
            set => SetValue(TokenDisplayProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="TokenDisplay"/> objects to display in the control.
        /// </summary>
        public IEnumerable TranslationOptions
        {
            get => (IEnumerable)GetValue(TranslationOptionsProperty);
            set => SetValue(TranslationOptionsProperty, value);
        }

        public TranslationControl()
        {
            InitializeComponent();
        }
    }
}
