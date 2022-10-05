namespace ClearDashboard.DAL.Alignment.Translation
{
    public enum TranslationActionType
    {
        /// <summary>
        /// The translation should propagate to other instances of the token.
        /// </summary>
        PutPropagate,

        /// <summary>
        /// The translation should apply to this token but not others.
        /// </summary>
        PutNoPropagate
    }

    public static class TranslationActionTypes
    {
        /// <summary>
        /// The translation should propagate to other instances of the token.
        /// </summary>
        public static string PutPropagate = "PutPropagate";

        /// <summary>
        /// The translation should apply to this token but not others.
        /// </summary>
        public static string PutNoPropagate = "PutNoPropagate";
    }
}
