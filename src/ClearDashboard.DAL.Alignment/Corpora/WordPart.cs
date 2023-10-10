namespace ClearDashboard.DAL.Alignment.Corpora
{
    public enum WordPart
    {
        /// <summary>
        /// Matching property values equal the search string
        /// </summary>
        Full,
        /// <summary>
        /// Matching property values start with search string (i.e. search string is a prefix)
        /// </summary>
        First,
        /// <summary>
        /// Matching property values contain the search string
        /// </summary>
        Middle,
        /// <summary>
        /// Matching property values end with the search string (i.e. search string is a suffix)
        /// </summary>
        Last
    }
}
