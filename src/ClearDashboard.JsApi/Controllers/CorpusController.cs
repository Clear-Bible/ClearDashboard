using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboardJsApi.Controllers;

public partial class CorpusController
{
    private readonly ILogger<CorpusController> _logger;
    private readonly IMediator _mediator;

    public CorpusController(ILogger<CorpusController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<Corpus> GetCorpus(Guid id)
    {
        var corpus = await Corpus.Get(_mediator, new CorpusId(id));
        return corpus;
    }
}

