using System;
using Caliburn.Micro;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.Wpf.Application.Models.EnhancedView;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class NoteAssociationViewModel : PropertyChangedBase
    {
        private string _description = string.Empty;
        private string _book;
        private string _chapter;
        private string _verse;
        private string _word;
        private string _part;
        private string _corpusName;

        public IId AssociatedEntityId { get; set; } = new EmptyEntityId();

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                NotifyOfPropertyChange();
            }
        }

        public string Book
        {
            get => _book;
            set => Set(ref _book, value);
        }

        public string Chapter
        {
            get => _chapter;
            set => Set(ref _chapter, value);
        }

        public string Verse
        {
            get => _verse;
            set => Set(ref _verse, value);
        }

        public string Word
        {
            get => _word;
            set => Set(ref _word, value);
        }

        public string Part
        {
            get => _part;
            set => Set(ref _part, value);
        }

        public int SortOrder => CalculateSortOrder();

        public string CorpusName
        {
            get => _corpusName;
            set => Set(ref _corpusName, value);
        }

        private int CalculateSortOrder()
        {
            //if (string.IsNullOrEmpty(Book))
            //{
            //    Book = "0";
            //}

            if (string.IsNullOrEmpty(Chapter))
            {
                Chapter = "0";
            }

            if (string.IsNullOrEmpty(Verse))
            {
                Verse = "0";
            }

            if (string.IsNullOrEmpty(Word))
            {
                Word = "0";
            }

            if (string.IsNullOrEmpty(Part))
            {
                Part = "0";
            }

            //return Convert.ToInt32($"{Book}{Chapter}{Verse}{Word}{Part}");
            return Convert.ToInt32($"{Chapter}{Verse}{Word}{Part}");
        }
    }
}
