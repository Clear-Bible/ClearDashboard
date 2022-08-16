using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClearDashboard.Wpf.ViewModels
{
    public class BackgroundTaskStatus : INotifyPropertyChanged
    {
        public enum StatusEnum
        {
            Working,
            Completed,
            Error
        }

        private StatusEnum _taskStatus;

        public StatusEnum TaskStatus
        {
            get => _taskStatus;
            set
            {
                _taskStatus = value;
                OnPropertyChanged();
            }
        }
        
        //private bool _isCompleted = false;
        //public bool IsCompleted  
        //{
        //    get => _isCompleted;
        //    set
        //    {
        //        _isCompleted = value;
        //        OnPropertyChanged();
        //    }
        //}

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private DateTime _startTime = DateTime.Now;
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime _endTime = DateTime.Now;
        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        //private bool _isError = false;

        //public bool IsError
        //{
        //    get => _isError;
        //    set
        //    {
        //        _isError = value;
        //        OnPropertyChanged();
        //    }
        //}





        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
