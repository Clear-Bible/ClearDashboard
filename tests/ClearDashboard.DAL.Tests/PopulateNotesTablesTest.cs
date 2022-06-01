using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIL.Reporting;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class PopulateNotesTablesTest : TestBase
        {
        public PopulateNotesTablesTest(ITestOutputHelper output) : base(output)
        {

        }


        [Fact]
        public async Task PopulateTable()
        {
            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            const string projectName = "NoteTest";
            Assert.NotNull(factory);

            var assets = await factory?.Get(projectName)!;
            var context = assets.AlignmentContext;

            try
            {
                var user = new User() { ParatextUsername = "Joe User" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var note = new Note();

                var stringContent = new StringContent
                {
                    Content = "Some string content"
                };
                note.ContentCollection.Add(stringContent);

                var binaryContent = new BinaryContent
                {
                    Content = Encoding.Unicode.GetBytes("Just some fake data")
                };
                note.ContentCollection.Add(binaryContent);

                //note.Author = new User { ParatextUsername = "Joe User" };
                note.Author = user;

                context.Notes.Add(note);
                await context.SaveChangesAsync();

                var roundTrippedNote = await context.Notes.FindAsync(note.Id);

                var content = roundTrippedNote.ContentCollection;
                Assert.NotNull(content);


                var stringContent2 = content.FirstOrDefault(item => item.ContentType == "StringContent");
                Assert.NotNull(stringContent2);
                Assert.Equal(stringContent.Content, ((StringContent)stringContent2).Content);


                var binaryContent2 = content.FirstOrDefault(item => item.ContentType == "BinaryContent");
                Assert.NotNull(binaryContent2);
                Assert.Equal(binaryContent.Content, ((BinaryContent)binaryContent2).Content);

            }
            finally
            {
                await context.Database.EnsureDeletedAsync();

                var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
                Directory.Delete(projectDirectory, true);
            }
        }

       
    }
}
