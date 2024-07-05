using System.Text.Json.Serialization;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System.IO;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Serializer;

namespace ClearDashboard.Collaboration;

/*
 * \Project_1e71b87a-fadf-424f-a3e0-e427a37d5d7a [Folder]
 *      \Properties [File]
 *          - Prop 1
 *          - Prop 2
 *      \Corpora [Folder]
 *          \Corpus-0 [Folder]
 *              \Properties [File]
 *                  - Prop1
 *                  - Prop2
 *              \TokenizedCorpora [Folder]
 *                  \TokenizedCorpus-0 [Folder]
 *                      \Properties [File]
 *                          - Prop1
 *                          - Prop2
 *                          - BookNumbers
 *                              "001"
 *                              "002"
 *                      \VerseRowsByBook
 *                          \VerseRow_001 [File]
 *                          \VerseRow_002 [File]
 *                          \VerseRow_003 [File]
 *                      \CompositeTokens [Folder]
 *                          \TC1 [File]
 *                              - Prop 1
 *                              - Prop 2
 *                          \TC2 [File]
 *                  - TokenizedCorpus-1 [Folder]
 *       \ParallelCorpora [Folder]
 *           \ParallelCorpus-0 [Folder]
 *               \Properties [File]
 *               \CompositeTokens [Folder]
 *       \Notes [Folder]
 *           \Note_0734CA26-0AA0-4846-A171-9F306DE630CB [Folder]
 *              \Properties
 *                  - Prop1
 *                  - Prop2
 *              \Replies
 *              \NoteModelRefs
 *                      
 *       \Labels [Folder]
 */

public class ProjectSnapshot
{
    private readonly GeneralModel<Models.Project> _projectGeneralModel;
    private readonly Dictionary<Type, Dictionary<object, GeneralModel>> _generalModelsByTypeThenId = new();

    public ProjectSnapshot(GeneralModel<Models.Project> project)
    {
        _projectGeneralModel = project;
    }

    public IModelSnapshot<Models.Project> Project { get => _projectGeneralModel; }
    public IEnumerable<IModelSnapshot<Models.User>> Users => GetGeneralModelList<Models.User>();
    public IEnumerable<IModelSnapshot<Models.Lexicon_Lexeme>> LexiconLexemes => GetGeneralModelList<Models.Lexicon_Lexeme>();
    public IEnumerable<IModelSnapshot<Models.Lexicon_SemanticDomain>> LexiconSemanticDomains => GetGeneralModelList<Models.Lexicon_SemanticDomain>();
	public IEnumerable<IModelSnapshot<Models.Grammar>> Grammar => GetGeneralModelList<Models.Grammar>();
	public IEnumerable<IModelSnapshot<Models.Corpus>> Corpora => GetGeneralModelList<Models.Corpus>();
    public IEnumerable<IModelSnapshot<Models.TokenizedCorpus>> TokenizedCorpora => GetGeneralModelList<Models.TokenizedCorpus>();
    public IEnumerable<IModelSnapshot<Models.ParallelCorpus>> ParallelCorpora => GetGeneralModelList<Models.ParallelCorpus>();
    public IEnumerable<IModelSnapshot<Models.AlignmentSet>> AlignmentSets => GetGeneralModelList<Models.AlignmentSet>();
    public IEnumerable<IModelSnapshot<Models.TranslationSet>> TranslationSets => GetGeneralModelList<Models.TranslationSet>();
    public IEnumerable<IModelSnapshot<Models.Note>> Notes => GetGeneralModelList<Models.Note>();
    public IEnumerable<IModelSnapshot<Models.Label>> Labels => GetGeneralModelList<Models.Label>();
    public IEnumerable<IModelSnapshot<Models.LabelGroup>> LabelGroups => GetGeneralModelList<Models.LabelGroup>();

    public void AddGeneralModelList<T>(IEnumerable<GeneralModel<T>> snapshotList) where T: notnull
    {
        _generalModelsByTypeThenId.Add(typeof(T), snapshotList.ToDictionary(e => e.GetId(), e => (GeneralModel)e));
    }

    public GeneralModel<Models.Project> GetGeneralModelProject()
    {
        return _projectGeneralModel;
    }

    public IEnumerable<GeneralModel<T>> GetGeneralModelList<T>() where T : notnull
    {
        if (_generalModelsByTypeThenId.TryGetValue(typeof(T), out var snapshotsById))
        {
            return snapshotsById.Values.Cast<GeneralModel<T>>().ToList();
        }
        else
        {
            //throw new ArgumentException($"ProjectSnapshot does not contain a GeneralModel of type {typeof(T).ShortDisplayName()}");
            return Enumerable.Empty<GeneralModel<T>>();
        }
    }
}