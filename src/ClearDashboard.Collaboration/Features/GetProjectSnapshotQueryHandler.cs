using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ClearDashboard.Collaboration;
using ClearDashboard.Collaboration.Model;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.Alignment.Notes;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Autofac.Features.Indexed;
using ClearDashboard.DAL.Alignment.Corpora;
using System.Data.Common;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Features;

public class GetProjectSnapshotQueryHandler : ProjectDbContextQueryHandler<
    GetProjectSnapshotQuery,
    RequestResult<ProjectSnapshot>,
    ProjectSnapshot>
{
    private readonly IMediator _mediator;

    public GetProjectSnapshotQueryHandler(IMediator mediator,
        ProjectDbContextFactory projectNameDbContextFactory,
        IProjectProvider projectProvider,
        ILogger<GetProjectSnapshotQueryHandler> logger)
        : base(projectNameDbContextFactory, projectProvider, logger)
    {
        _mediator = mediator;
    }

    protected override async Task<RequestResult<ProjectSnapshot>> GetDataAsync(GetProjectSnapshotQuery request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        //var converter = new DateTimeOffsetToBinaryConverter();
        //var codeToDatabase = converter.ConvertToProviderExpression.Compile();
        //var databaseToCode = converter.ConvertFromProviderExpression.Compile();

        try
        {
            BuilderContext builderContext = new (ProjectDbContext);

            var projectSnapshot = new ProjectSnapshot(ProjectBuilder.BuildModelSnapshot(builderContext));

            projectSnapshot.AddGeneralModelList(CorpusBuilder.BuildModelSnapshot(builderContext));
            projectSnapshot.AddGeneralModelList(TokenizedCorpusBuilder.BuildModelSnapshot(builderContext));
            projectSnapshot.AddGeneralModelList(ParallelCorpusBuilder.BuildModelSnapshot(builderContext));
            projectSnapshot.AddGeneralModelList(AlignmentSetBuilder.BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(TranslationSetBuilder.BuildModelSnapshot(builderContext));

            // Notes has to come after any other model type
            // it might reference:
            var notes = new List<GeneralModel<Models.Note>>();
            var noteBuilder = new NoteBuilder(ProjectDbContext);

            NoteBuilder.GetNotes(ProjectDbContext).ToList().ForEach(n =>
            {
                notes.Add(noteBuilder.BuildModelSnapshot(n, builderContext));
            });

            projectSnapshot.AddGeneralModelList(notes);
            projectSnapshot.AddGeneralModelList(LabelBuilder.BuildModelSnapshot(builderContext));

            return new RequestResult<ProjectSnapshot>(projectSnapshot);

        }
        catch (Exception ex)
        {
            return new RequestResult<ProjectSnapshot>(
                success: false,
                message: ex.Message
            );
        }
    }
}
