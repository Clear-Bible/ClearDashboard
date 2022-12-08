using System.ComponentModel;

namespace ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings
{
    public class RelatedLemma
    {
        private string _Id;
        public string Id
        {
            get => _Id;
            set
            {
                _Id = value;
                RaisePropertyChangeEvent("Id");
            }
        }



        private string _lemma;
        public string Lemma
        {
            get => _lemma;
            set
            {
                _lemma = value;
                RaisePropertyChangeEvent("Name");
            }
        }

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                _isAvailable = value;
                RaisePropertyChangeEvent("IsAvailable");
            }
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangeEvent(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        }

        #endregion
    }
}
