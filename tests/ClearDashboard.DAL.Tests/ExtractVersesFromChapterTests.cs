﻿using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace ClearDashboard.DAL.Tests
{
    public class ExtractVersesFromChapterTests
    {
        [Fact(Skip= "Defer until we decide on which parser to use.")]
        public void RealUsfmTests()
        {
            // defer until we decide on which parser to use
            return;


            //string startupPath = Directory.GetCurrentDirectory();
            //var usfmFiles = Directory.EnumerateFiles(Path.Combine(startupPath, "MockData", "USFM"),"*.sfm");

            //var serviceProvider = new ServiceCollection()
            //    .AddLogging()
            //    .BuildServiceProvider();
            //var logger = serviceProvider.GetService<ILoggerFactory>()
            //    .CreateLogger<Program>();


            //foreach (var file in usfmFiles)
            //{
            //    FileInfo fileInfo = new FileInfo(file);

            //    ParatextProject project = new ParatextProject();
            //    project.BooksList = new List<ParatextBook>();
            //    project.BooksList.Add(new ParatextBook
            //    {
            //        Available = true,
            //        BookId = fileInfo.Name.Substring(0,3),
            //        FilePath = file,
            //    });


            //    Verse verse = new Verse
            //    {
            //         VerseBBBCCCVVV = fileInfo.Name.Substring(0, 2) + "0" + fileInfo.Name.Substring(2, 2) + "002",
            //    };

            //    var verseList = ExtractVersesFromChapter.ParseUSFM(project, verse);

            //    // get it corresponding verse text file
            //    string fileCheckPath = Path.Combine(startupPath, "MockData", "VerseText", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length) + ".txt");

            //    if (! File.Exists(fileCheckPath))
            //    {
            //        throw new Exception();
            //    }

            //    List<string> checkList = new List<string>();
            //    foreach (string line in System.IO.File.ReadLines(fileCheckPath))
            //    {
            //        checkList.Add(line);
            //    }

            //    for (int i = 0; i < verseList.Count; i++)
            //    {
            //        //Debug.WriteLine(verseList[i]);
            //        //Debug.WriteLine(checkList[i]);
            //        //Assert.Equal(verseList[i], checkList[i]);
            //    }

            //}

        }
    }
}
