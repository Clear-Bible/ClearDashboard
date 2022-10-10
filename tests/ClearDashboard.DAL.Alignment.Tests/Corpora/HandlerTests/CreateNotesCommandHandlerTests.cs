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

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class CreateNotesCommandHandlerTests : TestBase
{
    public CreateNotesCommandHandlerTests(ITestOutputHelper output) : base(output)
    {
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

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - EngineAlignRows", sw.Elapsed);
            sw.Restart();
            #endregion

            var parallelCorpus = await parallelTextCorpus.Create("notes test pc", Mediator!);

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create ParallelCorpus", sw.Elapsed);
            sw.Restart();
            #endregion

            var sourceTokens = ProjectDbContext!.Tokens
                .Where(t => t.TokenizationId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(10)
                .Select(t => ModelHelper.BuildToken(t)).ToList();

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Retrieving Tokens (to get TokenIds to assocate Notes with)", sw.Elapsed);
            sw.Restart();
            #endregion

            var label = await new Label { Text = "boo label full of label-ness" }.CreateOrUpdate(Mediator!);

            var note = await new Note { Text = "a boo note", AbbreviatedText = "not sure", NoteStatus = "Resolved" }.CreateOrUpdate(Mediator!);
            var resolvedNoteId = note.NoteId;
            await note.AssociateLabel(Mediator!, label);
            var label2 = await note.CreateAssociateLabel(Mediator!, "boo label created in context of boo note");

            Assert.Equal(2, note.Labels.Count());
            Assert.Equal(Models.NoteStatus.Resolved.ToString(), note.NoteStatus);

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
            var openNoteId = note2.NoteId;

            var labels = await Label.Get(Mediator!, "boo lab");
            Assert.True(labels.Count() == 2);
            await note2.AssociateLabel(Mediator!, labels.First());
            await note2.CreateAssociateLabel(Mediator!, "super third");
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
                    Output.WriteLine($"\tLabel - Text: '{l.Text}', Id: '{l.LabelId!.Id}'");
                    allNoteLabelIds.Add(l.LabelId);
                }
                foreach (var nd in n.DomainEntityIds)
                {
                    Output.WriteLine($"\tDomain Entity Id - Type: '{nd.GetType().GetGenericArguments().First().Name}', Id: '{nd}'");
                }
            }

            Output.WriteLine("");

            var domainEntityIdCount = 0;
            var domainEntityIdNotes = await Note.GetAllDomainEntityIdNotes(Mediator!);
            foreach (var domainEntityIdNote in domainEntityIdNotes)
            {
                Output.WriteLine($"Domain Entity Id - Type: '{domainEntityIdNote.Key.GetType().GetGenericArguments().First().Name}', Id: '{(domainEntityIdNote.Key as IId)!.Id}'");
                foreach (var n in domainEntityIdNote.Value)
                {
                    Output.WriteLine($"\tNote - Text: '{n.Text}', Id: '{n.NoteId!.Id}'");
                    foreach (var l in n.Labels)
                    {
                        Output.WriteLine($"\t\tLabel - Text: '{l.Text}', Id: '{l.LabelId!.Id}'");
                    }
                }
                domainEntityIdCount++;
            }

            ProjectDbContext.ChangeTracker.Clear();

            var resolvedNote = await Note.Get(Mediator!, resolvedNoteId!);
            Assert.Equal(note.Text, resolvedNote.Text);
            Assert.Equal(note.AbbreviatedText, resolvedNote.AbbreviatedText);
            Assert.Equal(note.NoteStatus, resolvedNote.NoteStatus);
            Assert.Equal(note.ThreadId, resolvedNote.ThreadId);

            var openNote = await Note.Get(Mediator!, openNoteId!);
            Assert.Equal(note2.Text, openNote.Text);
            Assert.Equal(note2.AbbreviatedText, openNote.AbbreviatedText);
            Assert.Equal(note2.NoteStatus, openNote.NoteStatus);
            Assert.Equal(note2.ThreadId, openNote.ThreadId);

            var allLabels = await Label.GetAll(Mediator!);
            Assert.Equal(3, ProjectDbContext.Labels.Count());
            Assert.Equal(3, allLabels.Count());
            Assert.Equal(3, allNoteLabelIds.Count);

            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(2, allNotes.Count());

            Assert.Equal(5, domainEntityIdCount);
            Assert.Equal(4, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(6, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test the LabelNoteAssociation and NoteDomainEntityAssociation Delete methods:
            await note.DetachLabel(Mediator!, labels.First());
            await note.DetachDomainEntity(Mediator!, sourceTokens[7].TokenId);

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(3, ProjectDbContext.Labels.Count());
            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(3, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(5, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test Label.Delete and make sure the cascade delete works:
            // (Leave one Label/LabelNoteAssociation so we can test Note
            // cascade delete)
            label.Delete(Mediator!);
            label2.Delete(Mediator!);

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, ProjectDbContext.Labels.Count());
            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(1, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(5, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test Note.Delete and make sure the cascade delete works:
            await note.Delete(Mediator!);
            await note2.Delete(Mediator!);

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, ProjectDbContext.Labels.Count());
            Assert.False(ProjectDbContext.Notes.Any());
            Assert.False(ProjectDbContext.LabelNoteAssociations.Any());
            Assert.False(ProjectDbContext.NoteDomainEntityAssociations.Any());

            allLabels = await Label.GetAll(Mediator!);
            Assert.Single(allLabels);
            allLabels.First().Delete(Mediator!);

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
    public async void Notes__Thread()
    {
        try
        {
            var leadingNote = await new Note { Text = "leading note" }.CreateOrUpdate(Mediator!);
            var replyNote1 = await new Note(leadingNote) { Text = "reply note 1"  }.CreateOrUpdate(Mediator!);
            var replyNote2 = await new Note(leadingNote) { Text = "reply note 2" }.CreateOrUpdate(Mediator!);
            var replyNote3 = await new Note(replyNote1) { Text = "reply note 3" }.CreateOrUpdate(Mediator!);

            var standaloneNote1 = await new Note { Text = "standalone note 1" }.CreateOrUpdate(Mediator!);
            var replyNote4 = await new Note(replyNote1) { Text = "reply note 4" }.CreateOrUpdate(Mediator!);

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
            Assert.Null(leadingNote.ThreadId);
            Assert.True(leadingNote.NoteId!.IdEquals(replyNote1.ThreadId));
            Assert.True(leadingNote.NoteId!.IdEquals(replyNote4.ThreadId));

            var allNotesInThread1 = await Note.GetNotesInThread(new EntityId<NoteId>() { Id = leadingNote.NoteId!.Id }, Mediator!);
            Assert.Equal(5, allNotesInThread1.Count());

            var allNotesInThread2 = await Note.GetNotesInThread(new EntityId<NoteId>() { Id = replyNote3.ThreadId!.Id }, Mediator!);
            Assert.Equal(5, allNotesInThread2.Count());

            var position = 0;
            Output.WriteLine("Notes in thread:");
            foreach (var n in allNotesInThread2)
            {
                Assert.Equal((position == 0) ? "leading note" : $"reply note {position}", n.Text);
                Output.WriteLine($"\t{n.Text}");

                position++;
            }
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}