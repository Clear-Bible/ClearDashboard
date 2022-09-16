using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using VerseMapping = ClearBible.Engine.Corpora.VerseMapping;
using Verse = ClearBible.Engine.Corpora.Verse;
using SIL.Extensions;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using ClearDashboard.DAL.Alignment.Features;
using System.Threading.Tasks;

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

            var parallelCorpus = await parallelTextCorpus.Create(Mediator!);

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

            var label = await new Label(Mediator!, "boo label full of label-ness").CreateOrUpdate();
            var label2 = await new Label(Mediator!, "boo label NOT full of label-ness").CreateOrUpdate();
            var note = await new Note(Mediator!, "a boo note", "not sure").CreateOrUpdate();
            var lna11 = await new LabelNoteAssociation(Mediator!, label.LabelId!, note.NoteId!).Create();
            await new LabelNoteAssociation(Mediator!, label2.LabelId!, note.NoteId!).Create();

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create two labels, one note and two label note associations", sw.Elapsed);
            sw.Restart();
            #endregion

            var nda1 = await new NoteDomainEntityAssociation(Mediator!, note.NoteId!, sourceTokens[4].TokenId).Create();
            await new NoteDomainEntityAssociation(Mediator!, note.NoteId!, sourceTokens[7].TokenId).Create();
            await new NoteDomainEntityAssociation(Mediator!, note.NoteId!, targetTokenizedTextCorpus.TokenizedTextCorpusId).Create();
            await new NoteDomainEntityAssociation(Mediator!, note.NoteId!, parallelCorpus.ParallelCorpusId).Create();

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Associate Note 1 with four domain entity ids", sw.Elapsed);
            sw.Restart();
            #endregion

            var note2 = await new Note(Mediator!, "a baa note", "not sure").CreateOrUpdate();
            var labels = await Label.Get(Mediator!, "boo lab");
            Assert.True(labels.Count() == 2);
            var label3 = await new Label(Mediator!, "super third").CreateOrUpdate();
            await new LabelNoteAssociation(Mediator!, labels.First().LabelId!, note2.NoteId!).Create();
            await new LabelNoteAssociation(Mediator!, label3.LabelId!, note2.NoteId!).Create();

            await new NoteDomainEntityAssociation(Mediator!, note2.NoteId!, sourceTokenizedTextCorpus.TokenizedTextCorpusId).Create();

            #region Stopwatch
            sw.Stop();
            Output.WriteLine("Elapsed={0} - Create one note, retrieve one label, create one label, create two label note associations and create one note domain entity id association", sw.Elapsed);
            #endregion

            Output.WriteLine("");

            var allAssoc = await NoteDomainEntityAssociation.GetAll(Mediator!);
            foreach (var assoc in allAssoc)
            {
                var associatedNote = await Note.Get(Mediator!, assoc.NoteId);
                var associatedLabels = (await Task.WhenAll(
                    (await LabelNoteAssociation.GetAll(Mediator!, associatedNote.NoteId!))
                        .Select(ln => Label.Get(Mediator!, ln.LabelId)))).AsEnumerable();
                Output.WriteLine($"Assoc - Domain Id type: '{assoc.DomainEntityId.GetType().GetGenericArguments().First().Name}', Note Text: '{associatedNote.Text}', Domain Id: '{assoc.DomainEntityId.Id}', Note Id: '{assoc.NoteId.Id}'");
                foreach (var l in associatedLabels)
                {
                    Output.WriteLine($"\tAssociated Label - Text: '{l.Text}', Id: '{l.LabelId}'");
                }
                Output.WriteLine(string.Empty);
            }

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(3, ProjectDbContext.Labels.Count());
            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(4, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(5, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test the LabelNoteAssociation and NoteDomainEntityAssociation Delete methods:
            lna11.Delete();
            nda1.Delete();

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(3, ProjectDbContext.Labels.Count());
            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(3, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(4, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test Label.Delete and make sure the cascade delete works:
            // (Leave one Label/LabelNoteAssociation so we can test Note
            // cascade delete)
            label.Delete();
            label2.Delete();

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, ProjectDbContext.Labels.Count());
            Assert.Equal(2, ProjectDbContext.Notes.Count());
            Assert.Equal(1, ProjectDbContext.LabelNoteAssociations.Count());
            Assert.Equal(4, ProjectDbContext.NoteDomainEntityAssociations.Count());

            // Test Note.Delete and make sure the cascade delete works:
            note.Delete();
            note2.Delete();

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, ProjectDbContext.Labels.Count());
            Assert.False(ProjectDbContext.Notes.Any());
            Assert.False(ProjectDbContext.LabelNoteAssociations.Any());
            Assert.False(ProjectDbContext.NoteDomainEntityAssociations.Any());

            label3.Delete();

            ProjectDbContext.ChangeTracker.Clear();

            Assert.False(ProjectDbContext.Labels.Any());
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}