using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class CreateLexicalItemCommandHandler : ProjectDbContextCommandHandler<CreateLexicalItemCommand,
        RequestResult<LexicalItemId>, LexicalItemId>
    {
        private readonly IMediator _mediator;

        public CreateLexicalItemCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateLexicalItemCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LexicalItemId>> SaveDataAsync(CreateLexicalItemCommand request,
            CancellationToken cancellationToken)
        {
            var lexicalItem = new Models.LexicalItem
            {
                Id = Guid.NewGuid(),
                Text = request.Text,
                Type = request.Type,
                Language = request.Language
            };

            ProjectDbContext.LexicalItems.Add(lexicalItem);

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            lexicalItem = ProjectDbContext.LexicalItems.Include(n => n.User).First(li => li.Id == lexicalItem.Id);

            return new RequestResult<LexicalItemId>(ModelHelper.BuildLexicalItemId(lexicalItem));
        }
    }
}