using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Corpora;

public class SourceTextIdToVerseMappingsFromDatabase : SourceTextIdToVerseMappings
{
    private readonly IMediator _mediator;
    private readonly ParallelCorpusId _parallelCorpusId;
    private Dictionary<string, IEnumerable<VerseMapping>>? _textIdToVerseMappings = null;
    private SourceTextIdToVerseMappingsFromVerseMappings? _sourceTextIdToVerseMappingsFromVerseMappings = null;

    public SourceTextIdToVerseMappingsFromDatabase(IMediator mediator, ParallelCorpusId parallelCorpusId)
    {
        _mediator = mediator;
        _parallelCorpusId = parallelCorpusId;
    }

    public override IEnumerable<VerseMapping> GetVerseMappings()
    {
        // Use SourceTextIdToVerseMappingsFromVerseMappings implementation as cache:
        if (_sourceTextIdToVerseMappingsFromVerseMappings is null)
        {
            var command = new GetVerseMappingsByParallelCorpusIdAndBookIdQuery(_parallelCorpusId, null);

            var result = _mediator.Send(command, CancellationToken.None).Result;
            result.ThrowIfCanceledOrFailed(true);

            _sourceTextIdToVerseMappingsFromVerseMappings = new(result.Data!);
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

                var command = new GetVerseMappingsByParallelCorpusIdAndBookIdQuery(_parallelCorpusId, sourceTextId);

                var result = _mediator.Send(command, CancellationToken.None).Result;
                result.ThrowIfCanceledOrFailed(true);

                _textIdToVerseMappings[sourceTextId] = result.Data!;

                return result.Data!;
            }
        }
    }
}
