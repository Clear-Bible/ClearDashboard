using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;
using Xunit;
using Xunit.Abstractions;
using Autofac;

namespace ClearDashboard.DAL.Tests
{
    public class PopulateNotesTablesTest : TestBase
    {
        #nullable disable
        public PopulateNotesTablesTest(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public async Task NoteRecipientTest()
        {
            const string projectName = "NoteTest";
            SetupProjectDatabase(projectName, true);

            var userProvider = Container!.Resolve<IUserProvider>()!;
            Assert.NotNull(userProvider);

            try
            {
                var author = new User { FirstName = "Note", LastName = "Author" };
                userProvider.CurrentUser = author;
                var recipient1 = new User { FirstName = "Recipient",  LastName= "One" };
                var recipient2 = new User { FirstName = "Recipient", LastName="Two" };

                ProjectDbContext.Users.AddRange(author, recipient1, recipient2);
                await ProjectDbContext.SaveChangesAsync();

                var note = new Note
                {
                    User = author
                };

               
                note.NoteRecipients.Add(new NoteRecipient { User = recipient1, UserType = UserType.Recipient });
                note.NoteRecipients.Add(new NoteRecipient { User = recipient2, UserType = UserType.Recipient });

                ProjectDbContext.Notes.Add(note);
                await ProjectDbContext.SaveChangesAsync();

                var roundTrippedNote = await ProjectDbContext.Notes.FindAsync(note.Id);

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
                await DeleteDatabaseContext(projectName);
            }
        }

        [Fact]
        public async Task NoteRawContentTest()
        {
            const string projectName = "NoteTest";
            SetupProjectDatabase(projectName, true);

            var userProvider = Container!.Resolve<IUserProvider>()!;
            Assert.NotNull(userProvider);

            try
            {
                var user = new User { FirstName = "Joe", LastName="User" };
                userProvider.CurrentUser = user;
                ProjectDbContext.Users.Add(user);
                await ProjectDbContext.SaveChangesAsync();

                var note = new Note
                {
                    User = user
                };

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



                ProjectDbContext.Notes.Add(note);
                await ProjectDbContext.SaveChangesAsync();

                var roundTrippedNote = await ProjectDbContext.Notes.FindAsync(note.Id);
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
                await DeleteDatabaseContext(projectName);
            }
        }

       
    }
}
