using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClearDashboard.Common.Models
{
    public class BiblicalTermsData : INotifyPropertyChanged
    {

        private string _id = String.Empty;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }


        private string _lemma = String.Empty;
        public string Lemma
        {
            get => _lemma;
            set
            {
                _lemma = value;
                OnPropertyChanged();
            }
        }


        private string _transliteration = String.Empty;
        public string Transliteration
        {
            get => _transliteration;
            set
            {
                _transliteration = value;
                OnPropertyChanged();
            }
        }


        private string _semanticDomain = String.Empty;
        public string SemanticDomain
        {
            get => _semanticDomain;
            set
            {
                _semanticDomain = value;
                OnPropertyChanged();
            }
        }


        private string _localGloss = String.Empty;
        public string LocalGloss
        {
            get => _localGloss;
            set
            {
                _localGloss = value;
                OnPropertyChanged();
            }
        }


        private string _gloss = String.Empty;
        public string Gloss
        {
            get => _gloss;
            set
            {
                _gloss = value;
                OnPropertyChanged();
            }
        }



        private string _linkString = String.Empty;
        public string LinkString
        {
            get { return _linkString; }
            set
            {
                _linkString = value;
                OnPropertyChanged();
            }
        }


        private List<string> _referencesList = new List<string>();
        public List<string> References
        {
            get => _referencesList;
            set
            {
                _referencesList = value;
                OnPropertyChanged();
            }
        }

        private List<string> _referencesLong = new List<string>();
        public List<string> ReferencesLong
        {
            get => _referencesLong;
            set
            {
                _referencesLong = value;
                OnPropertyChanged();
            }
        }



        private List<string> _referencesListText = new List<string>();
        public List<string> ReferencesListText
        {
            get => _referencesListText;
            set
            {
                _referencesListText = value;
                OnPropertyChanged();
            }
        }


        private List<string> _renderings = new List<string>();
        public List<string> Renderings
        {
            get => _renderings;
            set
            {
                _renderings = value;
                OnPropertyChanged();
            }
        }

        private string _renderingString;
        public string RenderingString
        {
            get { return _renderingString; }
            set
            {
                _renderingString = value;
                OnPropertyChanged();
            }
        }

        private int _renderingCount;
        public int RenderingCount
        {
            get { return _renderingCount; }
            set
            {
                _renderingCount = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
