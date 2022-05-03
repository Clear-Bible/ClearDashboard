using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.Paratext.Plugin.Tests
{
    public class CleanUSFM_Test
    {
        private string _exportPath;

        public CleanUSFM_Test(ITestOutputHelper output)
        {
            _exportPath = AppDomain.CurrentDomain.BaseDirectory;
            _exportPath = Path.Combine(_exportPath, "MockData");
        }


        [Theory]
        [InlineData("0851c4dcaee3893d9ccdc7c59de9cdd661a586d6")]
        [InlineData("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f")]
        [InlineData("64f96631883240818dc01c345e58a1bff75e18e1")]
        [InlineData("80bbd0dbab94dbc97d5dc64d8c09fb687e066b26")]
        ///
        /// NOTE: the data in the \MockData\USFM\ directories are snippets from the export
        /// of USFM from the plugin.
        ///
        /// Data in the \MockData\VerseText\ directories are hand parsed USFM from copying
        /// from Paratext's UI and editing.  Horribily slow way to do things...
        /// 
        public void TestExportedUSFM(string dataDirectory)
        {
            var testUSFMDir = Path.Combine(_exportPath, "USFM", dataDirectory);
            var correctData = Path.Combine(_exportPath, "VerseText", dataDirectory);

            if (!Directory.Exists(testUSFMDir))
            {
                return;
            }

            // get the directories' files
            var usfmFiles = Directory.GetFiles(testUSFMDir);

            // loop through all the test data
            foreach (var usfmFile in usfmFiles)
            {
                FileInfo fi = new FileInfo(usfmFile);
                //check to see if there is a corresponding verse text file that matches this one
                var verseTextFile = Path.Combine(correctData, fi.Name);
                if (File.Exists(verseTextFile))
                {
                    var usfmLines = GetFileData(usfmFile);
                    var verseLines = GetFileData(verseTextFile);

                    for (int i = 0; i < usfmLines.Count; i++)
                    {
                        // check to see if the parser produced USFM data matches what we would expect 
                        // to see from the hand parsed USFM
                        Assert.Equal(verseLines[i].Trim(), usfmLines[i].Trim());
                    }
                }
            }
        }

        private List<string> GetFileData(string verseTextFile)
        {
            List<string> lines = new List<string>();
            using (StreamReader r = new StreamReader(verseTextFile))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}