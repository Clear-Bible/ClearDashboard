using ClearBible.Engine.Corpora;
using Models = ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Features;

namespace ClearDashboard.DAL.Alignment.Corpora;

internal class SourceTextIdToVerseMappingsFromDatabase : SourceTextIdToVerseMappings
{
    private readonly IMediator _mediator;
    private readonly ParallelCorpusId _parallelCorpusId;

    public SourceTextIdToVerseMappingsFromDatabase(IMediator mediator, ParallelCorpusId parallelCorpusId)
    {
        _mediator = mediator;
        _parallelCorpusId = parallelCorpusId;
    }

    public override IEnumerable<VerseMapping> GetVerseMappings()
    {
        var command = new GetVerseMappingsByParallelCorpusIdAndBookIdQuery(_parallelCorpusId, null);

        var result = _mediator.Send(command, CancellationToken.None).GetAwaiter().GetResult();
        result.ThrowIfCanceledOrFailed(true);

        return result.Data!;
    }

    public override IEnumerable<VerseMapping> this[string sourceTextId]
    {
        get
        {
            var command = new GetVerseMappingsByParallelCorpusIdAndBookIdQuery(_parallelCorpusId, sourceTextId);

            var result = _mediator.Send(command, CancellationToken.None).GetAwaiter().GetResult();
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
    }
}
