﻿using ClearDashboard.DAL.CQRS;
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
            var projectSnapshot = LoadSnapshot(builderContext, cancellationToken);

            return new RequestResult<ProjectSnapshot>(projectSnapshot);

        }
        catch (Exception ex)
        {
            return new RequestResult<ProjectSnapshot>(
                success: false,
                message: ex.Message,
                canceled: ex is OperationCanceledException
            );
        }
    }

    internal static ProjectSnapshot LoadSnapshot(BuilderContext builderContext, CancellationToken cancellationToken = default)
    {
        var projectSnapshot = new ProjectSnapshot(ProjectBuilder.BuildModelSnapshot(builderContext));

        projectSnapshot.AddGeneralModelList(GeneralModelBuilder.GetModelBuilder<Models.Corpus>().BuildModelSnapshots(builderContext));
        cancellationToken.ThrowIfCancellationRequested();

        projectSnapshot.AddGeneralModelList(GeneralModelBuilder.GetModelBuilder<Models.TokenizedCorpus>().BuildModelSnapshots(builderContext));
        cancellationToken.ThrowIfCancellationRequested();

        projectSnapshot.AddGeneralModelList(GeneralModelBuilder.GetModelBuilder<Models.ParallelCorpus>().BuildModelSnapshots(builderContext));
        cancellationToken.ThrowIfCancellationRequested();

        projectSnapshot.AddGeneralModelList(GeneralModelBuilder.GetModelBuilder<Models.AlignmentSet>().BuildModelSnapshots(builderContext));
        cancellationToken.ThrowIfCancellationRequested();

        projectSnapshot.AddGeneralModelList(GeneralModelBuilder.GetModelBuilder<Models.TranslationSet>().BuildModelSnapshots(builderContext));
        cancellationToken.ThrowIfCancellationRequested();

        projectSnapshot.AddGeneralModelList(GeneralModelBuilder.GetModelBuilder<Models.User>().BuildModelSnapshots(builderContext));
        cancellationToken.ThrowIfCancellationRequested();

        // Notes has to come after any other model type
        // it might reference:
        var notes = new List<GeneralModel<Models.Note>>();
        var noteBuilder = (NoteBuilder)GeneralModelBuilder.GetModelBuilder<Models.Note>();

        noteBuilder.GetNotes(builderContext.ProjectDbContext).ToList().ForEach(n =>
        {
            notes.Add(noteBuilder.BuildModelSnapshot(n, builderContext));
        });

        projectSnapshot.AddGeneralModelList(notes);
        projectSnapshot.AddGeneralModelList(GeneralModelBuilder.GetModelBuilder<Models.Label>().BuildModelSnapshots(builderContext));

        return projectSnapshot;
    }
}