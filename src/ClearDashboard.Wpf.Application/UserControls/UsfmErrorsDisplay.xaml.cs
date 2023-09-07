using System;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace ClearDashboard.Wpf.Application.UserControls
{
    
    /// <summary>
    /// Interaction logic for UsfmErrorsDisplay.xaml
    /// </summary>
    public partial class UsfmErrorsDisplay : UserControl
    {
        /// <summary>
        /// Identifies the TargetFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty ListViewHeightProperty = DependencyProperty.Register(nameof(ListViewHeight), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(200d));

        /// <summary>
        /// Gets or sets the font size for the target tokens.
        /// </summary>
        [TypeConverter(typeof(LengthConverter))]
        [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public double ListViewHeight
        {
            get => (double)GetValue(ListViewHeightProperty);
            set => SetValue(ListViewHeightProperty, value);
        }

        public UsfmErrorsDisplay()
        {
            InitializeComponent();
        }

        private void OnCopyToClipboardButtonClicked(object sender, RoutedEventArgs e)
        {
           SaveToClipboard();
        }

        private void OnSaveToFileButtonClicked(object sender, RoutedEventArgs e)
        {
           SaveToFile();
        }

        private IUsfmErrorHost TryGetUsfmErrorHost()
        {
            if (DataContext is IUsfmErrorHost host)
            {
                return host;
            }
            else
            {
                throw new InvalidCastException(
                    "Cannot cast the DataContext to IUsfmErrorHost.  Please ensure the DataContext implements IUsfmErrorHost.");
            }
        }

        private void SaveToClipboard()
        {
            var usfmErrorHost = TryGetUsfmErrorHost();
            var formattedUsfmErrors = usfmErrorHost.GetFormattedUsfmErrors();
            Clipboard.Clear();
            Clipboard.SetText(formattedUsfmErrors);
        }

        private  void SaveToFile()
        {
            var usfmErrorHost = TryGetUsfmErrorHost();
            var formattedUsfmErrors = usfmErrorHost.GetFormattedUsfmErrors();
            var fileName = usfmErrorHost.GetUsfmErrorsFileName();
            var dialog = new SaveFileDialog()
            {
                Filter = "Text Files(*.txt)|*.txt|All(*.*)|*",
                FileName = fileName
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, formattedUsfmErrors);
            }
        }
    }
}
