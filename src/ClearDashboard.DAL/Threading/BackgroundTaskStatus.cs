using System;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace ClearDashboard.DataAccessLayer.Threading
{
  


    public class BackgroundTaskStatus : INotifyPropertyChanged
    {
        /// <summary>
        /// Type of Task:
        ///  Normal - don't do anything
        ///  PerformanceMode - turn on the High Performance CPU settings for this type
        /// </summary>
        public enum BackgroundTaskMode
        {
            Normal,
            PerformanceMode
        }


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

        private LongRunningTaskStatus _taskLongRunningProcessStatus = LongRunningTaskStatus.Running;
        public LongRunningTaskStatus TaskLongRunningProcessStatus
        {
            get => _taskLongRunningProcessStatus;
            set
            {
                _taskLongRunningProcessStatus = value;
                OnPropertyChanged();
            }
        }

        private BackgroundTaskMode _backgroundTaskType = BackgroundTaskMode.Normal;
        public BackgroundTaskMode BackgroundTaskType
        {
            get => _backgroundTaskType;
            set
            {
                _backgroundTaskType = value;
                OnPropertyChanged();
            }
        }

        private Type _backgroundTaskSource;
        public Type BackgroundTaskSource
        {
            get => _backgroundTaskSource;
            set
            {
                _backgroundTaskSource = value;
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

        private string? _description = "";
        public string? Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string? _errorMessage = "";
        public string? ErrorMessage
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
