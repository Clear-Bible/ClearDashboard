using ClearDashboard.Wpf.Application.ViewModels.Main;
using System;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClearDashboard.Wpf.Application.Views.PopUps
{
    /// <summary>
    /// Interaction logic for SlackMessageView.xaml
    /// </summary>
    public partial class SlackMessageView : Window, INotifyPropertyChanged
    {

        private SlackMessageViewModel _viewModel = new SlackMessageViewModel();

        #region Observable Properties

        private List<FileItem> _attachedFilesCodeBehind = new();
        public List<FileItem> AttachedFilesCodeBehind
        {
            get => _attachedFilesCodeBehind;
            set
            {
                _attachedFilesCodeBehind = value;
                OnPropertyChanged("AttachedFilesCodeBehind");
            }
        }

        #endregion

        public SlackMessageView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SlackMessageViewModel)
            {
                _viewModel = DataContext as SlackMessageViewModel;
            }
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;

                var fileItem = new FileItem 
                { 
                    FileName = System.IO.Path.GetFileName(filePath), 
                    FilePath = filePath
                };

                // make sure we don't add the same file twice
                if (AttachedFilesCodeBehind.Any(f => f.FilePath == filePath))
                {
                    return;
                }

                AttachedFilesCodeBehind.Add(fileItem);
                AttachedFilesListBox.Items.Add(fileItem);

                _viewModel.AttachedFiles = AttachedFilesCodeBehind;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void RemoveAttachedFile_Click(object sender, RoutedEventArgs e)
        {
            var fileItem = (sender as Button).DataContext as FileItem;
            AttachedFilesCodeBehind.Remove(fileItem);
            AttachedFilesListBox.Items.Remove(fileItem);
            _viewModel.AttachedFiles = AttachedFilesCodeBehind;
        }
    }
}
