
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;

using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView
{

    public class BulkAlignment : PropertyChangedBase
    {
        private IEnumerable<Token> _sourceVerseTokens;
        private uint sourceVerseTokensIndex;
        private IEnumerable<Token> _targetVerseTokens;
        private uint _targetVerseTokensIndex;

        public Alignment? _alignment;
        public Alignment? Alignment
        {
            get => _alignment;
            set => Set(ref _alignment, value);
        }


        public IEnumerable<Token> SourceVerseTokens
        {
            get => _sourceVerseTokens;
            set => Set(ref _sourceVerseTokens, value);
        }

        public uint SourceVerseTokensIndex
        {
            get => sourceVerseTokensIndex;
            set => Set(ref sourceVerseTokensIndex, value);
        }

        public IEnumerable<Token> TargetVerseTokens
        {
            get => _targetVerseTokens;
            set => Set(ref _targetVerseTokens, value);
        }

        public uint TargetVerseTokensIndex
        {
            get => _targetVerseTokensIndex;
            set => Set(ref _targetVerseTokensIndex, value);
        }

    }

    public class Book
    {
        public int Number { get; set; }
        public string? Code { get; set; }
    }

    public class BookComparer : IEqualityComparer<Book>
    {
        public bool Equals(Book x, Book y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Number == y.Number && x.Code == y.Code;
        }

        public int GetHashCode(Book obj)
        {
            return HashCode.Combine(obj.Number, obj.Code);
        }
    }

    public class BulkAlignmentVerseRow : PropertyChangedBase
    {
        private bool _isSelected;
      

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        private Alignment? _alignment;
        private string? _type;
        private BulkAlignmentDisplayViewModel _bulkAlignmentDisplayViewModel;


        public BulkAlignmentDisplayViewModel BulkAlignmentDisplayViewModel
        {
            get => _bulkAlignmentDisplayViewModel;
            set => Set(ref _bulkAlignmentDisplayViewModel, value);
        }


        public Alignment? Alignment
        {
            get => _alignment;
            set
            {
                Set(ref _alignment, value);
                NotifyOfPropertyChange(nameof(SourceRef));
            }
        }

       public string BBBCCCVVV => Alignment.AlignmentId.SourceTokenId.BookNumber.ToString().PadLeft(3, '0') + Alignment.AlignmentId.SourceTokenId.ChapterNumber.ToString().PadLeft(3, '0') + Alignment.AlignmentId.SourceTokenId.VerseNumber.ToString().PadLeft(3, '0');

        public string? Type
        {
            get => _type;
            set => Set(ref _type, value);
        }

        public string? SourceRef => (Alignment != null && Alignment.AlignmentId != null && Alignment.AlignmentId.SourceTokenId != null) ?
            $"{VerseHelper.BookNames[Alignment.AlignmentId.SourceTokenId.BookNumber].code} {Alignment.AlignmentId.SourceTokenId.ChapterNumber}:{Alignment.AlignmentId.SourceTokenId.VerseNumber}" : "*** Alignment not set!";

        public string BookCode => VerseHelper.BookNames[Alignment.AlignmentId.SourceTokenId.BookNumber].code;
    }
    public class PivotWord
    {
       
        public string? Word { get; set; }
        public int Count { get; set; }
       
    }

    //public class AlignedWord
    //{
    //    public string? Source { get; set; }

    //    public string? Target { get; set; }
    //}

    public class AlignedWord 
    {
        public AlignedWord()
        {
            RelevantBooks = new List<Book>();
        }
        public long Count { get; set; }

        public PivotWord? PivotWord { get; set; }

        public string? Source { get; set; }

        public string? Target { get; set; }

        public List<Book> RelevantBooks = new List<Book>();

    }

    public static class BulkAlignmentReviewTags
    {
        public const string CountsByTrainingText = "CountByTrainingText";
        public const string CountBySurfaceText = "CountBySurfaceText";
        public const string Source = "Source";
        public const string Target = "Target";
        public const string Machine = "Machine";
        public const string NeedsReview = "NeedsReview";
        public const string Disapproved = "Disapproved";
        public const string Approved = "Approved";
        public const string ApproveSelected = "ApproveSelected";
        public const string DisapproveSelected = "DisapproveSelected";
        public const string MarkSelectedAsNeedsReview = "MarkSelectedAsNeedsReview";
    }
}
