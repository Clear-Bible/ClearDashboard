using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class BiblicalTermsData : ObservableObject
    {
        private string _id = String.Empty;
        [JsonProperty]
        public string Id
        {
            get => _id;
            set { SetProperty(ref _id, value); }
        }

        private string _lemma = String.Empty;
        [JsonProperty]
        public string Lemma
        {
            get => _lemma;
            set { SetProperty(ref _lemma, value); }
        }

        private string _transliteration = String.Empty;
        [JsonProperty]
        public string Transliteration
        {
            get => _transliteration;
            set { SetProperty(ref _transliteration, value); }
        }

        private string _semanticDomain = String.Empty;
        [JsonProperty]
        public string SemanticDomain
        {
            get => _semanticDomain;
            set
            {
                SetProperty(ref _semanticDomain, value);
            }
        }

        private string _localGloss = String.Empty;
        [JsonProperty]
        public string LocalGloss
        {
            get => _localGloss;
            set
            {
                SetProperty(ref _localGloss, value);
            }
        }

        private string _gloss = String.Empty;
        [JsonProperty]
        public string Gloss
        {
            get => _gloss;
            set
            {
                SetProperty(ref _gloss, value);
            }
        }

        private string _linkString = String.Empty;
        [JsonProperty]
        public string LinkString
        {
            get { return _linkString; }
            set
            {
                SetProperty(ref _linkString, value);
            }
        }


        private List<string> _referencesList = new List<string>();
        [JsonProperty]
        public List<string> References
        {
            get => _referencesList;
            set
            {
                SetProperty(ref _referencesList, value);
            }
        }

        private List<string> _referencesListLong = new List<string>();
        [JsonProperty]
        public List<string> ReferencesLong
        {
            get => _referencesListLong;
            set
            {
                SetProperty(ref _referencesListLong, value);
            }
        }

        private List<string> _referencesListText = new List<string>();
        [JsonProperty]
        public List<string> ReferencesListText
        {
            get => _referencesListText;
            set
            {
                SetProperty(ref _referencesListText, value);
            }
        }

        private List<string> _renderings = new List<string>();
        [JsonProperty]
        public List<string> Renderings
        {
            get => _renderings;
            set
            {
                SetProperty(ref _renderings, value);
            }
        }

        private string _renderingString;

        public string RenderingString
        {
            get { return _renderingString; }
            set
            {
                SetProperty(ref _renderingString, value);
            }
        }

    }

}
