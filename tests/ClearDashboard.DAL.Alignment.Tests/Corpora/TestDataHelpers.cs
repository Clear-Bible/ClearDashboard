using System;
using System.IO;


namespace ClearBible.Alignment.DataServices.Tests.Corpora
{
    internal static class TestDataHelpers
    {
        public static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "Corpora", "data");
        public static readonly string UsfmTestProjectPath = Path.Combine(TestDataPath, "usfm", "Tes");
    }
}
