using Autofac;
using ClearApi.Command.CQRS.Commands;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Corpora;

public class SourceTextIdToVerseMappingsFromDatabase : SourceTextIdToVerseMappings
{
    private readonly IComponentContext _context;
    private readonly ParallelCorpusId _parallelCorpusId;
    private Dictionary<string, IEnumerable<VerseMapping>>? _textIdToVerseMappings = null;
    private SourceTextIdToVerseMappingsFromVerseMappings? _sourceTextIdToVerseMappingsFromVerseMappings = null;

    public SourceTextIdToVerseMappingsFromDatabase(IComponentContext context, ParallelCorpusId parallelCorpusId)
    {
		_context = context;
        _parallelCorpusId = parallelCorpusId;
    }

    public override IEnumerable<VerseMapping> GetVerseMappings()
    {
        // Use SourceTextIdToVerseMappingsFromVerseMappings implementation as cache:
        if (_sourceTextIdToVerseMappingsFromVerseMappings is null)
		{
            // TODO:  Why isn't this method async or using JoinableTaskFactory?
            var result = new GetVerseMappingsByParallelCorpusIdAndBookIdQuery(_parallelCorpusId, null)
                .ExecuteAsProjectCommandAsync(_context, CancellationToken.None).Result;

            _sourceTextIdToVerseMappingsFromVerseMappings = new(result);
        }

        return _sourceTextIdToVerseMappingsFromVerseMappings.GetVerseMappings();
    }

    public override IEnumerable<VerseMapping> this[string sourceTextId]
    {
        get
        {
            // First check if book-specific cache entry exists:
            if (_textIdToVerseMappings is not null && _textIdToVerseMappings.TryGetValue(sourceTextId, out var value))
            {
                return value;
            }
            // Next check if GetVerseMappings (all mappings) cache exists:
            else if (_sourceTextIdToVerseMappingsFromVerseMappings is not null)
            {
                return _sourceTextIdToVerseMappingsFromVerseMappings[sourceTextId];
            }
            else
            {
                _textIdToVerseMappings ??= new();

				// TODO:  Why isn't this method async or using JoinableTaskFactory?
				var result = new GetVerseMappingsByParallelCorpusIdAndBookIdQuery(_parallelCorpusId, sourceTextId)
                    .ExecuteAsProjectCommandAsync(_context, CancellationToken.None).Result;

                _textIdToVerseMappings[sourceTextId] = result;

                return result;
            }
        }
    }
}
