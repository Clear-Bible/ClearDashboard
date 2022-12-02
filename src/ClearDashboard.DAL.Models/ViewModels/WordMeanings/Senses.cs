using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings
{
    public class Senses : INotifyPropertyChanged
    {
        private string _manuscriptWord;
        public string ManuscriptWord
        {
            get => _manuscriptWord;
            set
            {
                _manuscriptWord = value;
                RaisePropertyChangeEvent("ManuscriptWord");
            }
        }


        private String _sense = "";
        public String Sense
        {
            get => _sense;
            set
            {
                _sense = value;
                RaisePropertyChangeEvent("Sense");
            }
        }

        private double _verseTotal;
        public double VerseTotal
        {
            get => _verseTotal;
            set
            {
                _verseTotal = value;
                RaisePropertyChangeEvent("VerseTotal");
            }
        }

        private double _versePercent;
        public double VersePercent
        {
            get => _versePercent;
            set
            {
                _versePercent = value;
                RaisePropertyChangeEvent("VersePercent");
            }
        }

        public string VerseTotalPercent => $"{VerseTotal} ({Math.Round(VersePercent, 1)}%)";

        private string _descriptionLong = "";
        public string DescriptionLong
        {
            get => _descriptionLong;
            set
            {
                _descriptionLong = value;
                RaisePropertyChangeEvent("DescriptionLong");
            }
        }

        private string _descriptionShort = "";
        public string DescriptionShort
        {
            get => _descriptionShort;
            set
            {
                _descriptionShort = value;
                RaisePropertyChangeEvent("DescriptionShort");
            }
        }

        private List<string> _glosses = new();
        public List<string> Glosses
        {
            get => _glosses;
            set
            {
                _glosses = value;
                RaisePropertyChangeEvent("Glosses");
            }
        }

        private List<string> _strongCodes = new();
        public List<string> StrongCodes
        {
            get => _strongCodes;
            set
            {
                _strongCodes = value;
                RaisePropertyChangeEvent("StrongCodes");
            }
        }

        private List<CoupleOfStrings> _verses = new();
        public List<CoupleOfStrings> Verses
        {
            get => _verses;
            set
            {
                _verses = value;
                RaisePropertyChangeEvent("Verses");
            }
        }

        private List<string> _domains;
        public List<string> Domains
        {
            get => _domains;
            set
            {
                _domains = value;
                RaisePropertyChangeEvent("Domains");
            }
        }

        private List<string> _subDomains;
        public List<string> SubDomains
        {
            get => _subDomains;
            set
            {
                _subDomains = value;
                RaisePropertyChangeEvent("SubDomains");
            }
        }

        private List<string> _coreDomains;
        public List<string> CoreDomains
        {
            get => _coreDomains;
            set
            {
                _coreDomains = value;
                RaisePropertyChangeEvent("CoreDomains");
            }
        }

        private List<string> _synonyms;
        public List<string> Synonyms
        {
            get => _synonyms;
            set
            {
                _synonyms = value;
                RaisePropertyChangeEvent("Synonyms");
            }
        }

        private List<string> _antonyms;
        public List<string> Antonyms
        {
            get => _antonyms;
            set
            {
                _antonyms = value;
                RaisePropertyChangeEvent("Antonyms");
            }
        }

        private List<string> _collocations;
        public List<string> Collocations
        {
            get => _collocations;
            set
            {
                _collocations = value;
                RaisePropertyChangeEvent("Collocations");
            }
        }

        private List<string> _valencies;
        public List<string> Valencies
        {
            get => _valencies;
            set
            {
                _valencies = value;
                RaisePropertyChangeEvent("Valencies");
            }
        }


        // the treeview list shown on the left pane on the View
        private ObservableCollection<TreeNode> _alphabetTreeNodes = new ObservableCollection<TreeNode>();
        public ObservableCollection<TreeNode> AlphabetTreeNodes
        {
            get => _alphabetTreeNodes;
            set
            {
                _alphabetTreeNodes = value;
                RaisePropertyChangeEvent("AlphabetTreeNodes");
            }
        }



        private List<string> _relatedLemmas;
        public List<string> RelatedLemmas
        {
            get => _relatedLemmas;
            set
            {
                _relatedLemmas = value;
                RaisePropertyChangeEvent("RelatedLemmas");
            }
        }

        private List<string> _partsOfSpeech;
        public List<string> PartsOfSpeech
        {
            get => _partsOfSpeech;
            set
            {
                _partsOfSpeech = value;
                RaisePropertyChangeEvent("PartsOfSpeech");
            }
        }

        private List<RelatedLemma> _relatedLemmaList;
        public List<RelatedLemma> RelatedLemmaList
        {
            get => _relatedLemmaList;
            set
            {
                _relatedLemmaList = value;
                RaisePropertyChangeEvent("RelatedLemmaList");
            }
        }

        //private List<LexicalLink> _lexicalLinks;
        //public List<LexicalLink> LexicalLinks
        //{
        //    get => _lexicalLinks;
        //    set
        //    {
        //        _lexicalLinks = value;
        //        RaisePropertyChangeEvent("LexicalLinks");
        //    }
        //}

        //private LexicalLink _selectedLexicalLink;
        //public LexicalLink SelectedLexicalLink
        //{
        //    get => _selectedLexicalLink;
        //    set
        //    {
        //        _selectedLexicalLink = value;
        //        RaisePropertyChangeEvent("SelectedLexicalLink");
        //    }
        //}



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangeEvent(String propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        }

        #endregion
    }
}
