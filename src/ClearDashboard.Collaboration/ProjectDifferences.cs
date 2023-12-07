using System.Text.Json.Serialization;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using ClearDashboard.DataAccessLayer.Data;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System.IO;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Globalization;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Serializer;

namespace ClearDashboard.Collaboration;

public class ProjectDifferences
{
    public IModelDifference<IModelSnapshot<Models.Project>> Project { get; private set; }
    public IListDifference<IModelSnapshot<Models.User>> Users { get; private set; }
    public IListDifference<IModelSnapshot<Models.Lexicon_Lexeme>> LexiconLexemes { get; private set; }
    public IListDifference<IModelSnapshot<Models.Lexicon_SemanticDomain>> LexiconSemanticDomains { get; private set; }
    public IListDifference<IModelSnapshot<Models.Corpus>> Corpora { get; private set; }
    public IListDifference<IModelSnapshot<Models.TokenizedCorpus>> TokenizedCorpora { get; private set; }
    public IListDifference<IModelSnapshot<Models.ParallelCorpus>> ParallelCorpora { get; private set; }
    public IListDifference<IModelSnapshot<Models.AlignmentSet>> AlignmentSets { get; private set; }
    public IListDifference<IModelSnapshot<Models.TranslationSet>> TranslationSets { get; private set; }
    public IListDifference<IModelSnapshot<Models.Note>> Notes { get; private set; }
    public IListDifference<IModelSnapshot<Models.Label>> Labels { get; private set; }
    public IListDifference<IModelSnapshot<Models.LabelGroup>> LabelGroups { get; private set; }

    public bool HasDifferences { get; init; }

    public ProjectDifferences(ProjectSnapshot snapshot1, ProjectSnapshot snapshot2, CancellationToken cancellationToken = default) {

        Project = ((IModelDistinguishable<IModelSnapshot<Models.Project>>)snapshot1.Project).GetModelDifference(snapshot2.Project);
        cancellationToken.ThrowIfCancellationRequested();

        Users = snapshot1.Users.GetListDifference(snapshot2.Users);
        cancellationToken.ThrowIfCancellationRequested();

        LexiconLexemes = snapshot1.LexiconLexemes.GetListDifference(snapshot2.LexiconLexemes);
        cancellationToken.ThrowIfCancellationRequested();

        LexiconSemanticDomains = snapshot1.LexiconSemanticDomains.GetListDifference(snapshot2.LexiconSemanticDomains);
        cancellationToken.ThrowIfCancellationRequested();

        Corpora = snapshot1.Corpora.GetListDifference(snapshot2.Corpora);
        cancellationToken.ThrowIfCancellationRequested();

        TokenizedCorpora = snapshot1.TokenizedCorpora.GetListDifference(snapshot2.TokenizedCorpora);
        cancellationToken.ThrowIfCancellationRequested();

        ParallelCorpora = snapshot1.ParallelCorpora.GetListDifference(snapshot2.ParallelCorpora);
        cancellationToken.ThrowIfCancellationRequested();

        AlignmentSets = snapshot1.AlignmentSets.GetListDifference(snapshot2.AlignmentSets);
        cancellationToken.ThrowIfCancellationRequested();

        TranslationSets = snapshot1.TranslationSets.GetListDifference(snapshot2.TranslationSets);
        cancellationToken.ThrowIfCancellationRequested();

        Notes = snapshot1.Notes.GetListDifference(snapshot2.Notes);
        cancellationToken.ThrowIfCancellationRequested();

        Labels = snapshot1.Labels.GetListDifference(snapshot2.Labels);
        cancellationToken.ThrowIfCancellationRequested();

        LabelGroups = snapshot1.LabelGroups.GetListDifference(snapshot2.LabelGroups);
        cancellationToken.ThrowIfCancellationRequested();

        HasDifferences =
            Project.HasDifferences ||
            Users.HasDifferences ||
            LexiconLexemes.HasDifferences ||
            LexiconSemanticDomains.HasDifferences ||
            Corpora.HasDifferences ||
            TokenizedCorpora.HasDifferences ||
            ParallelCorpora.HasDifferences ||
            AlignmentSets.HasDifferences ||
            TranslationSets.HasDifferences ||
            Notes.HasDifferences ||
            Labels.HasDifferences ||
            LabelGroups.HasDifferences;
    }

    // For diagnostics:
    public void Serialize(string path)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            IncludeFields = true,
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver(),
            Converters = {
                        new JsonStringEnumConverter(),
                        new GeneralModelJsonConverter(),
                        new VerseRowModelListJsonConverter(),
                        new SystemTypeJsonConverter(),
                        new DifferenceJsonConverter()
                    }
        };

        Directory.CreateDirectory(path);

        var serializedProjectDifferences = JsonSerializer.Serialize(Project, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_ProjectDiffs"), serializedProjectDifferences);

        var serializedUserDifferences = JsonSerializer.Serialize(Users, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_UserDiffs"), serializedUserDifferences);

        var serializedLexiconLexemeDifferences = JsonSerializer.Serialize(LexiconLexemes, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_LexiconLexemeDiffs"), serializedLexiconLexemeDifferences);

        var serializedLexiconSemanticDomainDifferences = JsonSerializer.Serialize(LexiconSemanticDomains, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_LexiconSemanticDomainDiffs"), serializedLexiconSemanticDomainDifferences);

        var serializedCorporaDifferences = JsonSerializer.Serialize(Corpora, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_CorporaDiffs"), serializedCorporaDifferences);

        var serializedTokenizedCorporaDifferences = JsonSerializer.Serialize(TokenizedCorpora, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_TokenizedCorporaDiffs"), serializedTokenizedCorporaDifferences);

        var serializedParallelCorporaDifferences = JsonSerializer.Serialize(ParallelCorpora, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_ParallelCorporaDiffs"), serializedParallelCorporaDifferences);

        var serializedAlignmentSetDifferences = JsonSerializer.Serialize(AlignmentSets, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_AlignmentSetDiffs"), serializedAlignmentSetDifferences);

        var serializedTranslationSetDifferences = JsonSerializer.Serialize(TranslationSets, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_TranslationSetDiffs"), serializedTranslationSetDifferences);

        var serializedNoteDifferences = JsonSerializer.Serialize(Notes, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_NoteDiffs"), serializedNoteDifferences);

        var serializedLabelDifferences = JsonSerializer.Serialize(Labels, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_LabelDiffs"), serializedLabelDifferences);

        var serializedLabelGroupDifferences = JsonSerializer.Serialize(LabelGroups, jsonSerializerOptions);
        File.WriteAllText(Path.Combine(path, "_LabelGroupDiffs"), serializedLabelGroupDifferences);
    }
}