using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClearBible.Engine.Corpora;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.DAL.Alignment.Features;
using ClearBible.Engine.Utils;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DAL.Alignment.Translation;
using System.Diagnostics.Metrics;
using System.CodeDom;
using Microsoft.EntityFrameworkCore;
using ClearDashboard.DAL.Alignment.Exceptions;
using SIL.Machine.Translation;
using ClearBible.Engine.SyntaxTree.Aligner.Legacy;
using ClearDashboard.DAL.Alignment.BackgroundServices;
using Autofac;
using System.Threading;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;


public class CreateNotesCommandHandlerTests : TestBase
{
    public CreateNotesCommandHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    public class BadId : IId
    {
        public Guid Id { get; set; }

        public int GetIdHashcode()
        {
            return Id.GetHashCode();
        }

        public bool IdEquals(object? other)
        {
            return other is IId id && id.Id.Equals(Id);
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__Create()
    {
        try
        {
            #region Stopwatch
            Stopwatch sw = new Stopwatch();

            sw.Start();
            #endregion

            var user1 = new Models.User
            {
                Id = Guid.NewGuid(),
                FirstName = "User1 First Name",
                LastName = "User1 Last Name",
            };
            var user2 = new Models.User
            {
                Id = Guid.NewGuid(),
                FirstName = "User2 First Name",
                LastName = "User2 Last Name",
            };
            var user3 = new Models.User
            {
                Id = Guid.NewGuid(),
                FirstName = "User3 First Name",
                LastName = "User3 Last Name",
            };

            ProjectDbContext!.Users.Add(user1);
            ProjectDbContext!.Users.Add(user2);
            ProjectDbContext!.Users.Add(user3);
            await ProjectDbContext.SaveChangesAsync();

            ProjectDbContext.ChangeTracker.Clear();

            var sourceCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "test", "tokenization");

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create sample text corpus", sw.Elapsed);
            sw.Restart();
            #endregion

            var targetCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleGreekCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "test", "tokenization");

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create sample Greek corpus", sw.Elapsed);
            sw.Restart();
            #endregion

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                new SourceTextIdToVerseMappingsFromVerseMappings(TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - EngineAlignRows", sw.Elapsed);
            sw.Restart();
            #endregion

            var parallelCorpus = await parallelTextCorpus.CreateAsync("notes test pc", Container!);

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create ParallelCorpus", sw.Elapsed);
            sw.Restart();
            #endregion

            var sourceTokens = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(10)
                .Select(t => ModelHelper.BuildToken(t)).ToList();

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Retrieving Tokens (to get TokenIds to assocate Notes with)", sw.Elapsed);
            sw.Restart();
            #endregion

            var label = await new Label { Text = "boo label full of label-ness", TemplateText = "Template text 1" }.CreateOrUpdate(Mediator!);

            var note = new Note { Text = "a boo note", AbbreviatedText = "not sure", NoteStatus = "Resolved" };
            note.SeenByUserIds.Add(user3.Id);
            await note.CreateOrUpdate(Mediator!);

            var resolvedNoteId = note.NoteId;
            await note.AssociateLabel(Mediator!, label);
            var label2 = await note.CreateAssociateLabel(Mediator!, "boo label created in context of boo note", null);

            var labelGroup = await new LabelGroup { Name = "LabelGroup1" }.CreateOrUpdate(Mediator!);
            var lgLabel1 = await labelGroup.CreateAssociateLabel(Mediator!, "baa label 1 created in label group", "Template text 2");
            var lgLabel2 = await labelGroup.CreateAssociateLabel(Mediator!, "baa label 2 created in label group", null);
            await labelGroup.AssociateLabel(Mediator!, label2);

            var labelGroup2 = await new LabelGroup { Name = "LabelGroup2" }.CreateOrUpdate(Mediator!);
            var lgLabel21 = await labelGroup2.CreateAssociateLabel(Mediator!, "baa label 2 created in label group 2", "Template text");
            await labelGroup2.AssociateLabel(Mediator!, label2);

            var user2Id = new UserId(user2.Id, user2.FirstName);
            labelGroup.PutAsUserDefault(Mediator!, user2Id);
            Assert.Equal(labelGroup.LabelGroupId!.Id, ProjectDbContext.Users.Where(e => e.Id == user2.Id).First().DefaultLabelGroupId);
            Assert.Equal(labelGroup.LabelGroupId, await LabelGroup.GetUserDefault(Mediator!, user2Id));
            await LabelGroup.PutUserDefault(Mediator!, null, user2Id);
            ProjectDbContext.ChangeTracker.Clear();
            Assert.Null(ProjectDbContext.Users.Where(e => e.Id == user2.Id).First().DefaultLabelGroupId);
            Assert.Null(await LabelGroup.GetUserDefault(Mediator!, user2Id));

            Assert.Equal(2, note.Labels.Count);
            Assert.Equal(Models.NoteStatus.Resolved.ToString(), note.NoteStatus);
            Assert.Equal(2, note.SeenByUserIds.Count);  // User3.Id + IUserProvider.CurrentUser.Id

            note.SeenByUserIds.Add(user2.Id);
            await note.CreateOrUpdate(Mediator!);
            Assert.Equal(3, note.SeenByUserIds.Count);  // User2.Id + User3.Id + IUserProvider.CurrentUser.Id

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create two labels, one note and associate", sw.Elapsed);
            sw.Restart();
            #endregion

            await note.AssociateDomainEntity(Mediator!, sourceTokens[4].TokenId);
            await note.AssociateDomainEntity(Mediator!, sourceTokens[7].TokenId);
            await note.AssociateDomainEntity(Mediator!, targetTokenizedTextCorpus.TokenizedTextCorpusId);
            await note.AssociateDomainEntity(Mediator!, parallelCorpus.ParallelCorpusId);

            Assert.Equal(4, note.DomainEntityIds.Count());

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Associate Note 1 with four domain entity ids", sw.Elapsed);
            sw.Restart();
            #endregion

            var note2 = await new Note { Text = "a baa note", AbbreviatedText = "not sure" }.CreateOrUpdate(Mediator!);
            Assert.Equal(Models.NoteStatus.Open.ToString(), note2.NoteStatus);
            Assert.Equal(1, note2.SeenByUserIds.Count);  // IUserProvider.CurrentUser.Id
            var openNoteId = note2.NoteId;

            var labels = await Label.Get(Mediator!, "boo lab");
            Assert.True(labels.Count() == 2);
            await note2.AssociateLabel(Mediator!, labels.First());
            await note2.CreateAssociateLabel(Mediator!, "super third", "super third template text");
            await note2.AssociateDomainEntity(Mediator!, sourceTokenizedTextCorpus.TokenizedTextCorpusId);
            await note2.AssociateDomainEntity(Mediator!, parallelCorpus.ParallelCorpusId);

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create one note, retrieve one label, create one label, create two label note associations and create one note domain entity id association", sw.Elapsed);
            #endregion


            ProjectDbContext.ChangeTracker.Clear();
            Output.WriteLine("");

            var labelsToCompare = new List<Label>();
            var allNoteLabelIds = new HashSet<LabelId>();
            var allNotes = await Note.GetAllNotes(Mediator!);
            foreach (var n in allNotes)
            {
                Output.WriteLine($"Note - Text: '{n.Text}', Id: '{n.NoteId!.Id}'");
                foreach (var l in n.Labels)
                {
                    Output.WriteLine($"\tLabel - Text: '{l.Text}', Template Text: '{l.TemplateText}', Id: '{l.LabelId!.Id}'");
                    allNoteLabelIds.Add(l.LabelId);
                }
                foreach (var nd in n.DomainEntityIds)
                {
                    Output.WriteLine($"\tDomain Entity Id - Type: '{nd.GetType().GetGenericArguments().First().Name}', Id: '{nd.Id}'");
                }
            }

            Output.WriteLine("");
            Output.WriteLine("GetAllDomainEntityNotes");

            var domainEntityNotes = await Note.GetDomainEntityNotesThreadsFlattened(Mediator!, null);
            foreach (var domainEntityNote in domainEntityNotes)
            {
                Output.WriteLine($"Domain Entity Id - Type: '{domainEntityNote.Key.GetType().GetGenericArguments().First().Name}', Id: '{(domainEntityNote.Key as IId)!.Id}'");
                foreach (var n in domainEntityNote.Value)
                {
                    Output.WriteLine($"\tNote - Text: '{n.Text}', Id: '{n.NoteId!.Id}'");
                    foreach (var l in n.Labels)
                    {
                        Output.WriteLine($"\t\tLabel - Text: '{l.Text}', Template Text: '{l.TemplateText}', Id: '{l.LabelId!.Id}'");
                    }
                }
            }

            Output.WriteLine("");
            Output.WriteLine("GetAllDomainEntityIdNoteIds");

            var domainEntityIdCount = 0;
            var domainEntityNoteIds = await Note.GetDomainEntityNoteIds(Mediator!, null);
            foreach (var domainEntityNoteId in domainEntityNoteIds)
            {
                Output.WriteLine($"Domain Entity Id - Type: '{domainEntityNoteId.Key.GetType().GetGenericArguments().First().Name}', Id: '{(domainEntityNoteId.Key as IId)!.Id}'");
                foreach (var n in domainEntityNoteId.Value)
                {
                    Output.WriteLine($"\tNote - Id: '{n.Id}'");
                }
                domainEntityIdCount++;
            }

            var filteredDomainEntityNoteIds = await Note.GetDomainEntityNoteIds(
                Mediator!,
                new List<IId>() { targetTokenizedTextCorpus.TokenizedTextCorpusId, sourceTokens[4].TokenId });

            ProjectDbContext.ChangeTracker.Clear();

            var resolvedNote = await Note.Get(Mediator!, resolvedNoteId!);
            Assert.Equal(note.Text, resolvedNote.Text);
            Assert.Equal(note.AbbreviatedText, resolvedNote.AbbreviatedText);
            Assert.Equal(note.NoteStatus, resolvedNote.NoteStatus);
            Assert.Equal(note.ThreadId, resolvedNote.ThreadId);

            Assert.False(resolvedNote.SeenByUserIds.Contains(user1.Id));
            Assert.True(resolvedNote.SeenByUserIds.Contains(user2.Id));
            Assert.True(resolvedNote.SeenByUserIds.Contains(user3.Id));

            resolvedNote.SeenByUserIds.Remove(user2.Id);
            await resolvedNote.CreateOrUpdate(Mediator!);

            ProjectDbContext!.ChangeTracker.Clear();
            resolvedNote = await Note.Get(Mediator!, resolvedNoteId!);

            Assert.False(resolvedNote.SeenByUserIds.Contains(user1.Id));
            Assert.False(resolvedNote.SeenByUserIds.Contains(user2.Id));
            Assert.True(resolvedNote.SeenByUserIds.Contains(user3.Id));

            var openNote = await Note.Get(Mediator!, openNoteId!);
            Assert.Equal(note2.Text, openNote.Text);
            Assert.Equal(note2.AbbreviatedText, openNote.AbbreviatedText);
            Assert.Equal(note2.NoteStatus, openNote.NoteStatus);
            Assert.Equal(note2.ThreadId, openNote.ThreadId);

            var allLabels = await Label.GetAll(Mediator!);
            var labelGroups = await LabelGroup.GetAll(Mediator!);

            Assert.Equal(6, ProjectDbContext.Labels.Count());
            Assert.Equal(6, allLabels.Count());
            Assert.Equal(3, allNoteLabelIds.Count);
            Assert.Equal(2, labelGroups.Count());

            var labelIdsInGroup = await labelGroups.First().GetLabelIds(Mediator!);
            Assert.Equal(3, labelIdsInGroup.Count());

            var labelGroupLabels = await Label.GetAll(Mediator!, labelGroups.First().LabelGroupId);
            Assert.Equal(3, labelGroupLabels.Count());

            Output.WriteLine($"\nLabel group labels");
            foreach (var l in labelGroupLabels)
            {
                Output.WriteLine($"\tLabel - Text: '{l.Text}', Template Text: '{l.TemplateText}', Id: '{l.LabelId!.Id}'");
            }

            var exportedLabelGroups = await LabelGroup.Export(Mediator!);
            Output.WriteLine("\nLabel Group Export:");
            Output.WriteLine(exportedLabelGroups);

            await labelGroups.First().DetachLabel(Mediator!, lgLabel2);
            labelIdsInGroup = await labelGroups.First().GetLabelIds(Mediator!);
            Assert.Equal(2, labelIdsInGroup.Count());

            labelGroups.ToList().ForEach(e => e.Delete(Mediator!));
            Assert.Equal(0, ProjectDbContext.LabelGroups.Count());

            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(2, allNotes.Count());

            Assert.Equal(5, domainEntityNoteIds.Count);
            Assert.Equal(2, filteredDomainEntityNoteIds.Count);
            Assert.Contains(targetTokenizedTextCorpus.TokenizedTextCorpusId.Id, filteredDomainEntityNoteIds.Keys.Select(i => i.Id));
            Assert.Contains(sourceTokens[4].TokenId.Id, filteredDomainEntityNoteIds.Keys.Select(i => i.Id));
            var distinctFilteredNoteIds = filteredDomainEntityNoteIds.SelectMany(kvp => kvp.Value.Select(noteId => noteId.Id)).Distinct();
            Assert.Single(distinctFilteredNoteIds);
            Assert.Equal(note.NoteId!.Id, distinctFilteredNoteIds.First());
            Assert.Equal(4, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(6, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test the LabelNoteAssociation and NoteDomainEntityAssociation Delete methods:
            await note.DetachLabel(Mediator!, labels.First());
            await note.DetachDomainEntity(Mediator!, sourceTokens[7].TokenId);

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(6, ProjectDbContext.Labels.Count());
            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(3, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(5, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test Label.Delete and make sure the cascade delete works:
            // (Leave one Label/LabelNoteAssociation so we can test Note
            // cascade delete)
            label.Delete(Mediator!);
            label2.Delete(Mediator!);

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(4, ProjectDbContext.Labels.Count());
            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(1, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(5, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test Note.Delete and make sure the cascade delete works:
            await note.Delete(Mediator!);
            await note2.Delete(Mediator!);

            lgLabel1.Delete(Mediator!);
            lgLabel2.Delete(Mediator!);

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(2, ProjectDbContext.Labels.Count());
            Assert.False(ProjectDbContext.Notes.Any());
            Assert.False(ProjectDbContext.LabelNoteAssociations.Any());
            Assert.False(ProjectDbContext.NoteDomainEntityAssociations.Any());

            allLabels = await Label.GetAll(Mediator!);
            Assert.Equal(2, allLabels.Count());
            allLabels.ToList().ForEach(e => e.Delete(Mediator!));

            ProjectDbContext.ChangeTracker.Clear();

            Assert.False(ProjectDbContext.Labels.Any());
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__LabelGroupExportImport()
    {
        try 
        {
            var labelGroup1 = await new LabelGroup { Name = "LabelGroup1" }.CreateOrUpdate(Mediator!);
            var lg1Label1 = await labelGroup1.CreateAssociateLabel(Mediator!, "LabelGroup1_Label1", "LabelGroup1_Label1_TemplateText");
            var lg1Label2 = await labelGroup1.CreateAssociateLabel(Mediator!, "LabelGroup1_Label2", null);

            var labelGroup2 = await new LabelGroup { Name = "LabelGroup2" }.CreateOrUpdate(Mediator!);
            var lg2Label1 = await labelGroup2.CreateAssociateLabel(Mediator!, "LabelGroup2_Label1", "LabelGroup2_Label1_TemplateText");
            var lg2Label2 = await labelGroup2.CreateAssociateLabel(Mediator!, "LabelGroup2_Label2", "LabelGroup2_Label2_TemplateText");
            var lg2Label3 = await labelGroup2.CreateAssociateLabel(Mediator!, "LabelGroup2_Label3", null);

            var labelGroup3 = await new LabelGroup { Name = "LabelGroup3" }.CreateOrUpdate(Mediator!);
            var lg3Label1 = await labelGroup3.CreateAssociateLabel(Mediator!, "LabelGroup3_Label1", null);

            var serializedLabelGroups = await LabelGroup.Export(Mediator!, new List<string> { "LabelGroup2", "LabelGroup3" });
            var extractedLabelGroups = LabelGroup.Extract(serializedLabelGroups);

            Assert.NotNull(extractedLabelGroups);
            Assert.DoesNotContain(labelGroup1.Name!, extractedLabelGroups);
            Assert.Contains(labelGroup2.Name!, extractedLabelGroups);
            Assert.Contains(labelGroup3.Name!, extractedLabelGroups);

            Assert.Contains((Text: "LabelGroup2_Label1", TemplateText: "LabelGroup2_Label1_TemplateText"), extractedLabelGroups[labelGroup2.Name!]);
            Assert.Contains((Text: "LabelGroup2_Label2", TemplateText: "LabelGroup2_Label2_TemplateText"), extractedLabelGroups[labelGroup2.Name!]);
            Assert.Contains((Text: "LabelGroup2_Label3", TemplateText: null), extractedLabelGroups[labelGroup2.Name!]);
            Assert.Contains((Text: "LabelGroup3_Label1", TemplateText: null), extractedLabelGroups[labelGroup3.Name!]);

            var labelGroupsToImport = new Dictionary<string, IEnumerable<(string Text, string? TemplateText)>>
            {
                {
                    "LabelGroup4",
                    new List<(string Text, string? TemplateText)> { 
                        ("LabelGroup4_Label1", null), 
                        ("LabelGroup4_Label2", "LabelGroup4_Label2_TemplateText"), 
                        ("LabelGroup1_Label1", null) }  // Should not alter existing TemplateText for this label (should ignore this TemplateText)
                },
                {
                    labelGroup3.Name!,
                    new List<(string Text, string? TemplateText)> { 
                        ("LabelGroup3_Label1", "LabelGroup3_Label1_TemplateText"), 
                        ("LabelGroup2_Label3", "LabelGroup2_Label3_TemplateText"),  // Should not alter existing TemplateText for this label (should ignore this TemplateText)
                        ("LabelGroup3_Label3", "LabelGroup3_Label3_TemplateText") }
                },

            };

            await LabelGroup.Import(Mediator!, labelGroupsToImport);
            serializedLabelGroups = await LabelGroup.Export(Mediator!, null);
            extractedLabelGroups = LabelGroup.Extract(serializedLabelGroups);

            Assert.NotNull(extractedLabelGroups);
            Assert.Contains(labelGroup1.Name!, extractedLabelGroups);
            Assert.Contains(labelGroup2.Name!, extractedLabelGroups);
            Assert.Contains(labelGroup3.Name!, extractedLabelGroups);
            Assert.Contains("LabelGroup4", extractedLabelGroups);

            Assert.Contains((Text: "LabelGroup4_Label1", TemplateText: null), extractedLabelGroups["LabelGroup4"]);
            Assert.Contains((Text: "LabelGroup4_Label2", TemplateText: "LabelGroup4_Label2_TemplateText"), extractedLabelGroups["LabelGroup4"]);
            Assert.Contains((Text: "LabelGroup1_Label1", TemplateText: "LabelGroup1_Label1_TemplateText"), extractedLabelGroups["LabelGroup4"]); // Makes sure it left TemplateText as it was originally
            Assert.Contains((Text: "LabelGroup1_Label1", TemplateText: "LabelGroup1_Label1_TemplateText"), extractedLabelGroups[labelGroup1.Name!]); // Makes sure it left TemplateText as it was originally

            Assert.Contains((Text: "LabelGroup3_Label1", TemplateText: null), extractedLabelGroups[labelGroup3.Name!]); // Makes sure it left TemplateText as it was originally
            Assert.Contains((Text: "LabelGroup2_Label3", TemplateText: null), extractedLabelGroups[labelGroup3.Name!]);
            Assert.Contains((Text: "LabelGroup3_Label3", TemplateText: "LabelGroup3_Label3_TemplateText"), extractedLabelGroups[labelGroup3.Name!]);

            ProjectDbContext!.ChangeTracker.Clear();

            Assert.Single(ProjectDbContext!.Labels.Where(e => e.Text == "LabelGroup1_Label1"));
            Assert.Single(ProjectDbContext!.Labels.Where(e => e.Text == "LabelGroup2_Label3"));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__Thread()
    {
        try
        {
            var sourceCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "test", "tokenization");

            var sourceTokens = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(3)
                .Select(t => ModelHelper.BuildToken(t)).ToList();

            var leadingNote = await new Note { Text = "leading note" }.CreateOrUpdate(Mediator!);
            await leadingNote.AssociateDomainEntity(Mediator!, sourceTokens![0].TokenId);

            var replyNote1 = await new Note(leadingNote) { Text = "reply note 1"  }.CreateOrUpdate(Mediator!);
            var replyNote2 = await new Note(leadingNote) { Text = "reply note 2" }.CreateOrUpdate(Mediator!);
            var replyNote3 = await new Note(replyNote1) { Text = "reply note 3" }.CreateOrUpdate(Mediator!);
            await replyNote3.AssociateDomainEntity(Mediator!, sourceTokens![1].TokenId);

            var standaloneNote1 = await new Note { Text = "standalone note 1" }.CreateOrUpdate(Mediator!);
            var standaloneNote2 = await new Note { Text = "standalone note 2" }.CreateOrUpdate(Mediator!);
            await standaloneNote2.AssociateDomainEntity(Mediator!, sourceTokens![2].TokenId);
            await standaloneNote2.AssociateDomainEntity(Mediator!, sourceTokens![1].TokenId);
            var standaloneNote3 = await new Note { Text = "standalone note 3" }.CreateOrUpdate(Mediator!);
            await standaloneNote3.AssociateDomainEntity(Mediator!, sourceTokens![0].TokenId);
            var replyNote4 = await new Note(replyNote1) { Text = "reply note 4" }.CreateOrUpdate(Mediator!);

            var leadingNote2 = await new Note { Text = "leading note2" }.CreateOrUpdate(Mediator!);
            var replyNote21 = await new Note(leadingNote2) { Text = "reply note 1 for leading note 2" }.CreateOrUpdate(Mediator!);

            ProjectDbContext!.ChangeTracker.Clear();

            leadingNote = await Note.Get(Mediator!, leadingNote.NoteId!);
            replyNote1 = await Note.Get(Mediator!, replyNote1.NoteId!);
            replyNote4 = await Note.Get(Mediator!, replyNote4.NoteId!);
            standaloneNote1 = await Note.Get(Mediator!, standaloneNote1.NoteId!);

            Assert.False(standaloneNote1.IsReply());
            Assert.False(leadingNote.IsReply());
            Assert.True(replyNote1.IsReply());
            Assert.True(replyNote4.IsReply());

            Assert.Null(standaloneNote1.ThreadId);
            Assert.NotNull(leadingNote.ThreadId);
            Assert.True(leadingNote.NoteId!.IdEquals(leadingNote.ThreadId));
            Assert.True(leadingNote.NoteId!.IdEquals(replyNote1.ThreadId));
            Assert.True(leadingNote.NoteId!.IdEquals(replyNote4.ThreadId));

            var allNotesInThread1 = await Note.GetNotesInThread(Mediator!, new EntityId<NoteId>() { Id = leadingNote.NoteId!.Id });
            Assert.Equal(5, allNotesInThread1.Count());

            var replyNotes1 = await leadingNote.GetReplyNotes(Mediator!);
            Assert.Equal(4, replyNotes1.Count());

            var allNotesInThread2 = await Note.GetNotesInThread(Mediator!, new EntityId<NoteId>() { Id = replyNote3.ThreadId!.Id });
            Assert.Equal(5, allNotesInThread2.Count());

            var position = 0;
            Output.WriteLine("Notes in thread:");
            foreach (var n in allNotesInThread2)
            {
                Assert.Equal((position == 0) ? "leading note" : $"reply note {position}", n.Text);
                Output.WriteLine($"\t{n.Text}");

                position++;
            }

            position = 0;
            Output.WriteLine("Leading note replies:");
            foreach (var n in replyNotes1)
            {
                Assert.Equal($"reply note {position + 1}", n.Text);
                Output.WriteLine($"\t{n.Text}");

                position++;
            }

            await replyNote1.Delete(Mediator!);

            var allNotesInThread3 = await Note.GetNotesInThread(Mediator!, new EntityId<NoteId>() { Id = leadingNote.NoteId!.Id });
            Assert.Equal(4, allNotesInThread3.Count());

            position = 0;
            Output.WriteLine("Notes in thread (after deleting '1'):");
            foreach (var n in allNotesInThread3)
            {
                Assert.Equal((position == 0) ? "leading note" : $"reply note {position+1}", n.Text);
                Output.WriteLine($"\t{n.Text}");

                position++;
            }

            var replyNotes2 = await leadingNote.GetReplyNotes(Mediator!);
            Assert.Equal(3, replyNotes2.Count());

            position = 0;
            Output.WriteLine("Leading note replies (after deleting '1'):");
            foreach (var n in replyNotes2)
            {
                Assert.Equal($"reply note {position+2}", n.Text);
                Output.WriteLine($"\t{n.Text}");

                position++;
            }


            Output.WriteLine($"\nOutput IId + Notes (hierarchical - Leading Notes containing reply Notes):");
            var entityNotesAndThreads = await Note.GetDomainEntityNotesThreads(Mediator!, null);
            foreach (var kvp in entityNotesAndThreads)
            {
                Output.WriteLine($"\tIId (TokenId): {kvp.Key.Id}");
                foreach (var kvp2 in kvp.Value)
                {
                    Output.WriteLine($"\t\tNote name: {kvp2.Key.Text}");
                    foreach (var kvp3 in kvp2.Value)
                    {
                        Output.WriteLine($"\t\t\tReply note name: {kvp3.Text}");
                    }
                }
            }

            Assert.True(entityNotesAndThreads.ContainsKey(sourceTokens![0].TokenId));
            Assert.True(entityNotesAndThreads.ContainsKey(sourceTokens![1].TokenId));
            Assert.True(entityNotesAndThreads.ContainsKey(sourceTokens![2].TokenId));

            Assert.Contains(leadingNote.NoteId, entityNotesAndThreads[sourceTokens![0].TokenId].Select(n => n.Key.NoteId));
            Assert.Contains(replyNote2.NoteId, entityNotesAndThreads[sourceTokens![0].TokenId].SelectMany(kvp => kvp.Value).Select(note => note.NoteId).Distinct());
            Assert.Contains(replyNote3.NoteId, entityNotesAndThreads[sourceTokens![0].TokenId].SelectMany(kvp => kvp.Value).Select(note => note.NoteId).Distinct());
            Assert.Contains(replyNote4.NoteId, entityNotesAndThreads[sourceTokens![0].TokenId].SelectMany(kvp => kvp.Value).Select(note => note.NoteId).Distinct());
            Assert.Contains(standaloneNote3.NoteId, entityNotesAndThreads[sourceTokens![0].TokenId].Select(n => n.Key.NoteId));

            Assert.Contains(replyNote3.NoteId, entityNotesAndThreads[sourceTokens![1].TokenId].Select(n => n.Key.NoteId));

            Assert.Contains(standaloneNote2.NoteId, entityNotesAndThreads[sourceTokens![2].TokenId].Select(n => n.Key.NoteId));

            Output.WriteLine($"\nOutput IId + Notes (flattened - Leading Notes and reply Notes listed together):");
            var entityNotes = await Note.GetDomainEntityNotesThreadsFlattened(Mediator!, null);
            foreach (var kvp in entityNotes)
            {
                Output.WriteLine($"\tIId (TokenId): {kvp.Key.Id}");
                foreach (var kvp2 in kvp.Value)
                {
                    Output.WriteLine($"\t\tNote name: {kvp2.Text}");
                }
            }

            Assert.True(entityNotes.ContainsKey(sourceTokens![0].TokenId));
            Assert.True(entityNotes.ContainsKey(sourceTokens![1].TokenId));
            Assert.True(entityNotes.ContainsKey(sourceTokens![2].TokenId));

            Assert.Contains(leadingNote.NoteId, entityNotes[sourceTokens![0].TokenId].Select(n => n.NoteId));
            Assert.Contains(replyNote2.NoteId, entityNotes[sourceTokens![0].TokenId].Select(n => n.NoteId));
            Assert.Contains(replyNote3.NoteId, entityNotes[sourceTokens![0].TokenId].Select(n => n.NoteId));
            Assert.Contains(replyNote4.NoteId, entityNotes[sourceTokens![0].TokenId].Select(n => n.NoteId));
            Assert.Contains(standaloneNote3.NoteId, entityNotes[sourceTokens![0].TokenId].Select(n => n.NoteId));

            Assert.Contains(replyNote3.NoteId, entityNotes[sourceTokens![1].TokenId].Select(n => n.NoteId));

            Assert.Contains(standaloneNote2.NoteId, entityNotes[sourceTokens![2].TokenId].Select(n => n.NoteId));

            var noteIdsInEntityNotes = entityNotes.SelectMany(kvp => kvp.Value).Select(note => note.NoteId).Distinct();
            Assert.Contains(leadingNote.NoteId, noteIdsInEntityNotes);
            Assert.DoesNotContain(leadingNote2.NoteId, noteIdsInEntityNotes);
            Assert.DoesNotContain(replyNote21.NoteId, noteIdsInEntityNotes);


            Output.WriteLine($"\nFiltered Output IId + Notes (flattened - Leading Notes and reply Notes listed together):");
            var filteredEntityNotes = await Note.GetDomainEntityNotesThreadsFlattened(
                Mediator!, 
                new List<IId>() { sourceTokens![1].TokenId });
            foreach (var kvp in filteredEntityNotes)
            {
                Output.WriteLine($"\tIId (TokenId): {kvp.Key.Id}");
                foreach (var kvp2 in kvp.Value)
                {
                    Output.WriteLine($"\t\tNote name: {kvp2.Text}");
                }
            }
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__GetDomainEntityContexts()
    {
        try
        {
            var translationCommandable = Container!.Resolve<TranslationCommands>();

            var sourceCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "test", "tokenization");
            var targetCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1.1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleGreekCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "test", "tokenization");
            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, 
                new SourceTextIdToVerseMappingsFromVerseMappings(TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));
            var parallelCorpus = await parallelTextCorpus.CreateAsync("notes test pc", Container!);
            using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                SmtModelType.FastAlign,
                parallelTextCorpus,
                null,
                SymmetrizationHeuristic.GrowDiagFinalAnd);
            var alignmentModel = translationCommandable.PredictAllAlignedTokenIdPairs(smtWordAlignmentModel, parallelTextCorpus).ToList();
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            var n = await new Note() { Text = "everything note!" }.CreateOrUpdate(Mediator!);

            await n.AssociateDomainEntity(Mediator!, sourceCorpus.CorpusId);
            await n.AssociateDomainEntity(Mediator!, sourceTokenizedTextCorpus.TokenizedTextCorpusId);
            await n.AssociateDomainEntity(Mediator!, targetTokenizedTextCorpus.TokenizedTextCorpusId);
            await n.AssociateDomainEntity(Mediator!, parallelCorpus.ParallelCorpusId);
            await n.AssociateDomainEntity(Mediator!, alignmentSet.AlignmentSetId);
            await n.AssociateDomainEntity(Mediator!, translationSet.TranslationSetId);

            var tokenIds = sourceTokenizedTextCorpus.GetRows(new List<string>() { "MAT" }).Cast<TokensTextRow>().First().Tokens.Take(2).Select(t => t.TokenId);

            await n.AssociateDomainEntity(Mediator!, tokenIds.First());
            await n.AssociateDomainEntity(Mediator!, tokenIds.Skip(1).First());

            var alignments = ProjectDbContext!.Alignments.Include(a => a.SourceTokenComponent).Where(a => a.AlignmentSetId == alignmentSet.AlignmentSetId.Id).Take(3);
            foreach (var a in alignments)
            {
                await n.AssociateDomainEntity(Mediator!, new AlignmentId(a.Id, "boo", "boo", ModelHelper.BuildTokenId(a.SourceTokenComponent!)));
            }

            var domainEntityContexts = await n.GetDomainEntityContexts(Mediator!);
            foreach (var kvp in domainEntityContexts)
            {
                Output.WriteLine("");
                Output.WriteLine($"EntityId<{kvp.Key.GetType().FindEntityIdGenericType()?.Name}>:  '{kvp.Key.Id}'");
                foreach (var kvp2 in kvp.Value)
                {
                    Output.WriteLine($"\t{kvp2.Key}: '{kvp2.Value}'");
                }
            }

            var entityIds = new List<IId>() { translationSet.TranslationSetId, parallelCorpus.ParallelCorpusId };
            entityIds.Add(new BadId() { Id = Guid.NewGuid() });

            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => Note.GetDomainEntityContexts(Mediator!, entityIds));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__GetFullEntityIds()
    {
        try
        {
            var translationCommandable = Container!.Resolve<TranslationCommands>();

            var sourceCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "test", "tokenization");
            var targetCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleGreekCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "test", "tokenization");
            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, 
                new SourceTextIdToVerseMappingsFromVerseMappings(TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));
            var parallelCorpus = await parallelTextCorpus.CreateAsync("notes test pc", Container!);
            using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                SmtModelType.FastAlign,
                parallelTextCorpus,
                null,
                SymmetrizationHeuristic.GrowDiagFinalAnd);
            var alignmentModel = translationCommandable.PredictAllAlignedTokenIdPairs(smtWordAlignmentModel, parallelTextCorpus).ToList();
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            var n = await new Note() { Text = "everything note!" }.CreateOrUpdate(Mediator!);

            await n.AssociateDomainEntity(Mediator!, sourceCorpus.CorpusId);
            await n.AssociateDomainEntity(Mediator!, sourceTokenizedTextCorpus.TokenizedTextCorpusId);
            await n.AssociateDomainEntity(Mediator!, targetTokenizedTextCorpus.TokenizedTextCorpusId);
            await n.AssociateDomainEntity(Mediator!, parallelCorpus.ParallelCorpusId);
            await n.AssociateDomainEntity(Mediator!, alignmentSet.AlignmentSetId);
            await n.AssociateDomainEntity(Mediator!, translationSet.TranslationSetId);

            var alignments = ProjectDbContext!.Alignments.Include(a => a.SourceTokenComponent).Where(a => a.AlignmentSetId == alignmentSet.AlignmentSetId.Id).Take(3);
            foreach (var a in alignments)
            {
                await n.AssociateDomainEntity(Mediator!, new AlignmentId(a.Id, "boo", "boo", ModelHelper.BuildTokenId(a.SourceTokenComponent!)));
            }

            var fullIds = await n.GetFullDomainEntityIds(Mediator!);
            foreach (var iid in fullIds)
            {
                Output.WriteLine($"{iid.GetType().Name}:  '{iid.Id}'");
            }

            var entityIds = new List<IId>() { translationSet.TranslationSetId, parallelCorpus.ParallelCorpusId };
            entityIds.Add(new BadId() { Id = Guid.NewGuid() });

            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => Note.GetFullDomainEntityIds(Mediator!, entityIds));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__ParatextGuid()
    {
        try
        {
            var paratextId = Guid.NewGuid().ToString();
            var sourceCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource",
                paratextId);
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "test", "tokenization");

            var verseRows = ProjectDbContext!.VerseRows
                .Include(v => v.TokenComponents)
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(2)
                .ToList();

            var verseRowOneTokens = verseRows.First().TokenComponents
                .Select(t => ModelHelper.BuildToken(t))
                .ToArray();

            var verseRowTwoTokens = verseRows.Skip(1).First().TokenComponents
                .Select(t => ModelHelper.BuildToken(t))
                .ToArray();

            var noteGood = await new Note { Text = "a good note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteGood.AssociateDomainEntity(Mediator!, verseRowOneTokens[3].TokenId);
            await noteGood.AssociateDomainEntity(Mediator!, verseRowOneTokens[2].TokenId);
            await noteGood.AssociateDomainEntity(Mediator!, verseRowOneTokens[4].TokenId);

            var noteGoodSingleToken = await new Note { Text = "a good note having a single token", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteGoodSingleToken.AssociateDomainEntity(Mediator!, verseRowOneTokens[3].TokenId);

            var noteNonContiguous = await new Note { Text = "a non contiguous token note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteNonContiguous.AssociateDomainEntity(Mediator!, verseRowOneTokens[1].TokenId);
            await noteNonContiguous.AssociateDomainEntity(Mediator!, verseRowOneTokens[3].TokenId);
            await noteNonContiguous.AssociateDomainEntity(Mediator!, verseRowOneTokens[4].TokenId);

            var noteMultipleVerseRows = await new Note { Text = "a multiple verse note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteMultipleVerseRows.AssociateDomainEntity(Mediator!, verseRowOneTokens[1].TokenId);
            await noteMultipleVerseRows.AssociateDomainEntity(Mediator!, verseRowOneTokens[2].TokenId);
            await noteMultipleVerseRows.AssociateDomainEntity(Mediator!, verseRowTwoTokens[0].TokenId);

            var noteMuiltipleDomainEntityTypes = await new Note { Text = "a multiple domain entity type note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteMuiltipleDomainEntityTypes.AssociateDomainEntity(Mediator!, sourceTokenizedTextCorpus.TokenizedTextCorpusId);
            await noteMuiltipleDomainEntityTypes.AssociateDomainEntity(Mediator!, verseRowTwoTokens[0].TokenId);
            await noteMuiltipleDomainEntityTypes.AssociateDomainEntity(Mediator!, verseRowTwoTokens[1].TokenId);

            var noteNoTokenAssociations = await new Note { Text = "a no token association note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteNoTokenAssociations.AssociateDomainEntity(Mediator!, sourceCorpus.CorpusId);
            await noteNoTokenAssociations.AssociateDomainEntity(Mediator!, sourceTokenizedTextCorpus.TokenizedTextCorpusId);

            var noteNoAssociations = await new Note { Text = "a no association note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);

            var goodResult = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteGood.NoteId!);
            Assert.NotNull(goodResult);

            Assert.Equal(paratextId, goodResult.Value.paratextId);
            Assert.Equal(sourceTokenizedTextCorpus.TokenizedTextCorpusId, goodResult.Value.tokenizedTextCorpusId);
            Assert.Equal(verseRowOneTokens.Length, goodResult.Value.verseTokens.Count());
            Assert.Equal<TokenId>(
                verseRowOneTokens.OrderBy(t => t.TokenId.ToString()).Select(t => t.TokenId), 
                goodResult.Value.verseTokens.OrderBy(t => t.TokenId.ToString()).Select(t => t.TokenId), 
                new IIdEqualityComparer());

            var goodResultSingleToken = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteGoodSingleToken.NoteId!);
            Assert.NotNull(goodResultSingleToken);

            Assert.Equal(paratextId, goodResultSingleToken.Value.paratextId);
            Assert.Equal(sourceTokenizedTextCorpus.TokenizedTextCorpusId, goodResultSingleToken.Value.tokenizedTextCorpusId);
            Assert.Equal(verseRowOneTokens.Length, goodResultSingleToken.Value.verseTokens.Count());
            Assert.Equal<TokenId>(
                verseRowOneTokens.OrderBy(t => t.TokenId.ToString()).Select(t => t.TokenId),
                goodResultSingleToken.Value.verseTokens.OrderBy(t => t.TokenId.ToString()).Select(t => t.TokenId),
                new IIdEqualityComparer());

            Assert.Null(await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteNonContiguous.NoteId!));
            Assert.Null(await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteMultipleVerseRows.NoteId!));
            Assert.Null(await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteMuiltipleDomainEntityTypes.NoteId!));
            Assert.Null(await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteNoTokenAssociations.NoteId!));
            Assert.Null(await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteNoAssociations.NoteId!));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]














































































    public async void Notes__ParatextGreekManuscriptGuid()
    {
        try
        {
            var sourceCorpusGreekManuscript = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource",
                ManuscriptIds.GreekManuscriptId);
            var sourceTokenizedTextCorpusGreekManuscript = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpusGreekManuscript.CorpusId, "test", "tokenization");

            var tokenGreekManuscript = ProjectDbContext!.TokenComponents
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpusGreekManuscript.TokenizedTextCorpusId.Id)
                .Take(1)
                .Select(t => ModelHelper.BuildToken(t))
                .First();

            var noteManuscript = await new Note { Text = "a manuscript note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteManuscript.AssociateDomainEntity(Mediator!, tokenGreekManuscript.TokenId);

            var nullResultManuscript = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteManuscript.NoteId!);
            Assert.Null(nullResultManuscript);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__ParatextHebrewManuscriptGuid()
    {
        try
        {
            var sourceCorpusGreekManuscript = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource",
                ManuscriptIds.HebrewManuscriptId);
            var sourceTokenizedTextCorpusGreekManuscript = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpusGreekManuscript.CorpusId, "test", "tokenization");

            var tokenGreekManuscript = ProjectDbContext!.TokenComponents
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpusGreekManuscript.TokenizedTextCorpusId.Id)
                .Take(1)
                .Select(t => ModelHelper.BuildToken(t))
                .First();

            var noteManuscript = await new Note { Text = "a manuscript note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteManuscript.AssociateDomainEntity(Mediator!, tokenGreekManuscript.TokenId);

            var nullResultManuscript = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteManuscript.NoteId!);
            Assert.Null(nullResultManuscript);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__ParatextZZSurGuid()
    {
        try
        {
            var paratextIdZZSur = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f";
            var sourceCorpusZZSur = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource",
                paratextIdZZSur);
            var sourceTokenizedTextCorpusZZSur = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpusZZSur.CorpusId, "test", "tokenization");

            var verseRowZZSur = ProjectDbContext!.VerseRows
                .Include(v => v.TokenComponents)
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpusZZSur.TokenizedTextCorpusId.Id)
                .First();

            var verseRowZZSurTokens = verseRowZZSur.TokenComponents
                .Select(t => ModelHelper.BuildToken(t))
                .ToArray();

            var noteZZSur = await new Note { Text = "a zz_sur note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteZZSur.AssociateDomainEntity(Mediator!, verseRowZZSurTokens[0].TokenId);

            var goodResultZZSur = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteZZSur.NoteId!);
            Assert.NotNull(goodResultZZSur);

            Assert.Equal(paratextIdZZSur, goodResultZZSur.Value.paratextId);
            Assert.Equal(sourceTokenizedTextCorpusZZSur.TokenizedTextCorpusId, goodResultZZSur.Value.tokenizedTextCorpusId);
            Assert.Equal(verseRowZZSurTokens.Length, goodResultZZSur.Value.verseTokens.Count());
            Assert.Equal<TokenId>(
                verseRowZZSurTokens.OrderBy(t => t.TokenId.ToString()).Select(t => t.TokenId),
                goodResultZZSur.Value.verseTokens.OrderBy(t => t.TokenId.ToString()).Select(t => t.TokenId),
                new IIdEqualityComparer());
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Notes__ParatextNoParatextGuid()
    {
        try
        {
            var sourceCorpusNoParatext = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource",
                string.Empty);
            var sourceTokenizedTextCorpusNoParatext = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpusNoParatext.CorpusId, "test", "tokenization");

            var tokenNoParatext = ProjectDbContext!.TokenComponents
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpusNoParatext.TokenizedTextCorpusId.Id)
                .Take(1)
                .Select(t => ModelHelper.BuildToken(t))
                .First();

            var noteNoParatext = await new Note { Text = "a no paratext note", AbbreviatedText = "not sure", NoteStatus = "Open" }.CreateOrUpdate(Mediator!);
            await noteNoParatext.AssociateDomainEntity(Mediator!, tokenNoParatext.TokenId);

            var nullResultNoParatext = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(Mediator!, noteNoParatext.NoteId!);
            Assert.Null(nullResultNoParatext);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}