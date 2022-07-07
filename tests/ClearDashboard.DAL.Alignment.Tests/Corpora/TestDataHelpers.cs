using System;
using System.IO;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    internal static class TestDataHelpers
    {
        public static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "Corpora", "data");

        public static readonly string UsfmTestProjectPath = Path.Combine(TestDataPath, "usfm", "Tes");

        public static readonly string GreekNTUsfmTestProjectPath =
            Path.Combine(TestDataPath, "usfm", "nestle1904");
    }
}