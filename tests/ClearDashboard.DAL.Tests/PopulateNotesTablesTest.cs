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
        public async Task NoteRecipientTest()
        {
            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            const string projectName = "NoteTest";
            Assert.NotNull(factory);

            var assets = await factory?.Get(projectName)!;
            var context = assets.AlignmentContext;

            try
            {
                var author = new User() { ParatextUsername = "Note Author" };
                var recipient1 = new User() { ParatextUsername = "Recipient One" };
                var recipient2 = new User() { ParatextUsername = "Recipient Two" };

                context.Users.AddRange(author, recipient1, recipient2);
                await context.SaveChangesAsync();

                var note = new Note
                {
                    Author = author
                };

               
                note.NoteRecipients.Add(new NoteRecipient { User = recipient1, UserType = UserType.Recipient });
                note.NoteRecipients.Add(new NoteRecipient { User = recipient2, UserType = UserType.Recipient });

                context.Notes.Add(note);
                await context.SaveChangesAsync();

                var roundTrippedNote = await context.Notes.FindAsync(note.Id);

                Assert.NotNull(roundTrippedNote);
                Assert.Equal(2, roundTrippedNote.NoteRecipients.Count);

                Assert.Collection(roundTrippedNote.NoteRecipients,  
                    item=> Assert.Equal(recipient1.Id, item.UserId) ,
                                        item=> Assert.Equal(recipient2.Id, item.UserId)
                );

                Assert.Collection(roundTrippedNote.NoteRecipients,
                    item => Assert.Equal(UserType.Recipient, item.UserType),
                                        item => Assert.Equal(UserType.Recipient, item.UserType)
                );
            }
            finally
            {
                await context.Database.EnsureDeletedAsync();
                var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
                Directory.Delete(projectDirectory, true);
            }
        }

        [Fact]
        public async Task NoteRawContentTest()
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
                note.ContentCollection.Add(new StringContent
                {
                    Content = "Some string content"
                });

                var binaryContent = new BinaryContent
                {
                    Content = Encoding.Unicode.GetBytes("Just some fake data")
                };
                note.ContentCollection.Add(binaryContent);

                note.Author = user;

                context.Notes.Add(note);
                await context.SaveChangesAsync();

                var roundTrippedNote = await context.Notes.FindAsync(note.Id);
                Assert.NotNull(roundTrippedNote);
                Assert.NotNull(roundTrippedNote?.ContentCollection);
                
                var stringContentCollection = roundTrippedNote?.StringContentCollection.ToList();
                Assert.NotNull(stringContentCollection);
                Assert.True(stringContentCollection?.Count() == 1);
                var stringContent2 = stringContentCollection?.FirstOrDefault();
                Assert.NotNull(stringContent2);
                Assert.Equal(stringContent.Content, stringContent2?.Content);

                var binaryContentCollection = roundTrippedNote?.BinaryContentCollection.ToList();
                Assert.NotNull(binaryContentCollection);
                Assert.True(binaryContentCollection?.Count() == 1);
                var binaryContent2 = binaryContentCollection?.FirstOrDefault(item => item.ContentType == "BinaryContent");
                Assert.NotNull(binaryContent2);
                Assert.Equal(binaryContent.Content, binaryContent2?.Content);

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
