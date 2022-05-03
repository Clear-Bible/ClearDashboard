using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClearDashboard.Common.Models
{
    public class BiblicalTermsData 
    {

        private string _id = String.Empty;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
            }
        }


        private string _lemma = String.Empty;
        public string Lemma
        {
            get => _lemma;
            set
            {
                _lemma = value;
            }
        }


        private string _transliteration = String.Empty;
        public string Transliteration
        {
            get => _transliteration;
            set
            {
                _transliteration = value;
            }
        }


        private string _semanticDomain = String.Empty;
        public string SemanticDomain
        {
            get => _semanticDomain;
            set
            {
                _semanticDomain = value;
            }
        }


        private string _localGloss = String.Empty;
        public string LocalGloss
        {
            get => _localGloss;
            set
            {
                _localGloss = value;
            }
        }


        private string _gloss = String.Empty;
        public string Gloss
        {
            get => _gloss;
            set
            {
                _gloss = value;
            }
        }



        private string _linkString = String.Empty;
        public string LinkString
        {
            get { return _linkString; }
            set
            {
                _linkString = value;
            }
        }


        private List<string> _referencesList = new List<string>();
        public List<string> References
        {
            get => _referencesList;
            set
            {
                _referencesList = value;
            }
        }

        private List<string> _referencesLong = new List<string>();
        public List<string> ReferencesLong
        {
            get => _referencesLong;
            set
            {
                _referencesLong = value;
            }
        }



        private List<string> _referencesListText = new List<string>();
        public List<string> ReferencesListText
        {
            get => _referencesListText;
            set
            {
                _referencesListText = value;
            }
        }


        private List<string> _renderings = new List<string>();
        public List<string> Renderings
        {
            get => _renderings;
            set
            {
                _renderings = value;
            }
        }

        private string _renderingString;
        public string RenderingString
        {
            get { return _renderingString; }
            set
            {
                _renderingString = value;
            }
        }

        private int _renderingCount;
        public int RenderingCount
        {
            get { return _renderingCount; }
            set
            {
                _renderingCount = value;
            }
        }

    }
}
