using ClearBible.Engine.Corpora;

using SIL.Machine.Corpora;

namespace ClearBible.Alignment.DataServices.Tests.Tokenization
{
    public class OnlyUpToCountTextRowProcessor : IRowFilter<TextRow>
    {
        private static int count_ = 1;
        public bool Process(TextRow textRow)
        {
            // perform filtering
            if (count_ > 0)
            {
                count_--;
                return true;
            }
            return false;
        }
        public static void Train(int count)
        {
            count_ = count;
        }
    }
}
