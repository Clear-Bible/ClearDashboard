﻿using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class LexiconTranslationViewModel : PropertyChangedBase
    {
        public Translation Entity { get; }

        public MeaningViewModel? Meaning { get; set; }

        public TranslationId? TranslationId
        {
            get => Entity.TranslationId;
#if DEBUG
            set => Entity.TranslationId = value;
#endif
        }

        public string? Text
        {
            get => Entity.Text ?? string.Empty;
            set
            {
                if (Equals(value, Entity.Text)) return;
                Entity.Text = value;
                NotifyOfPropertyChange();
            }
        }

        public int Count { get; set; }

        public bool IsSelected { get; set; }
        public LexiconTranslationViewModel() : this(new Translation())
        {
        }

        public LexiconTranslationViewModel(string text, int count) : this(new Translation { Text = text })
        {
            Count = count;
        }

        public LexiconTranslationViewModel(Translation translation, MeaningViewModel? meaning = null)
        {
            Entity = translation;
            Meaning = meaning;
        }
    }
}