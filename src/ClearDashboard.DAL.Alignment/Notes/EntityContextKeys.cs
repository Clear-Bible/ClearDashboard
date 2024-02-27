namespace ClearDashboard.DAL.Alignment.Notes
{
    public static class EntityContextKeys
    {
        public static class Corpus
        {
            public const string DisplayName = "Corpus.DisplayName";
        }

        public static class TokenizedCorpus
        {
            public const string DisplayName = "TokenizedCorpus.DisplayName";
        }

        public static class SourceTokenizedCorpus
        {
            public const string DisplayName = "SourceTokenizedCorpus.DisplayName";
        }

        public static class TargetTokenizedCorpus
        {
            public const string DisplayName = "TargetTokenizedCorpus.DisplayName";
        }

        public static class TokenId
        {
            public const string BookId = "TokenId.BookId";
            public const string ChapterNumber = "TokenId.ChapterNumber";
            public const string VerseNumber = "TokenId.VerseNumber";
            public const string WordNumber = "TokenId.WordNumber";
            public const string SubwordNumber = "TokenId.SubwordNumber";
            public const string SurfaceText = "TokenId.SurfaceText";
        }

        public static class AlignmentSet
        {
            public const string DisplayName = "AlignmentSet.DisplayName";
        }

        public static class TranslationSet
        {
            public const string DisplayName = "TranslationSet.DisplayName";
        }

        public static class User
        {
            public const string DisplayName = "User.DisplayName";
        }

        public static class Note
        {
            public const string Text = "Note.DisplayName";
        }

        public const string Uri = "URI";
    }
}
