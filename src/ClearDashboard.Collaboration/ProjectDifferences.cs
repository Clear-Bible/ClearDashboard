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
    public IListDifference<IModelSnapshot<Models.Corpus>> Corpora { get; private set; }
    public IListDifference<IModelSnapshot<Models.TokenizedCorpus>> TokenizedCorpora { get; private set; }
    public IListDifference<IModelSnapshot<Models.ParallelCorpus>> ParallelCorpora { get; private set; }
    public IListDifference<IModelSnapshot<Models.AlignmentSet>> AlignmentSets { get; private set; }
    public IListDifference<IModelSnapshot<Models.TranslationSet>> TranslationSets { get; private set; }
    public IListDifference<IModelSnapshot<Models.Note>> Notes { get; private set; }
    public IListDifference<IModelSnapshot<Models.Label>> Labels { get; private set; }

    public ProjectDifferences(ProjectSnapshot snapshot1, ProjectSnapshot snapshot2) {

        Project = ((IModelDistinguishable<IModelSnapshot<Models.Project>>)snapshot1.Project).GetModelDifference(snapshot2.Project);
        Corpora = snapshot1.Corpora.GetListDifference(snapshot2.Corpora);
        TokenizedCorpora = snapshot1.TokenizedCorpora.GetListDifference(snapshot2.TokenizedCorpora);
        ParallelCorpora = snapshot1.ParallelCorpora.GetListDifference(snapshot2.ParallelCorpora);
        AlignmentSets = snapshot1.AlignmentSets.GetListDifference(snapshot2.AlignmentSets);
        TranslationSets = snapshot1.TranslationSets.GetListDifference(snapshot2.TranslationSets);
        Notes = snapshot1.Notes.GetListDifference(snapshot2.Notes);
        Labels = snapshot1.Labels.GetListDifference(snapshot2.Labels);

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
    }
}