using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClearDashboard.Wpf.ViewModels
{
    public enum StatusEnum
    {
        Working,
        Completed,
        Error,
        CancelTaskRequested,
    }


    public class BackgroundTaskStatus : INotifyPropertyChanged
    {
        private bool _cancelTaskRequest = false;
        public bool CancelTaskRequest
        {
            get => _cancelTaskRequest;
            set
            {
                _cancelTaskRequest = value;
                OnPropertyChanged();
            }
        }

        private StatusEnum _taskStatus = StatusEnum.Working;
        public StatusEnum TaskStatus
        {
            get => _taskStatus;
            set
            {
                _taskStatus = value;
                OnPropertyChanged();
            }
        }
        
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



#pragma warning disable CS8618
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS8618

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
