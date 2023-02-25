using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClearBible.Engine.Corpora;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.DAL.Alignment.Features;
using ClearBible.Engine.Utils;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DAL.Alignment.Translation;
using System.Diagnostics.Metrics;
using System.CodeDom;
using Microsoft.EntityFrameworkCore;
using ClearDashboard.DAL.Alignment.Exceptions;
using SIL.Machine.Translation;
using ClearBible.Engine.SyntaxTree.Aligner.Legacy;
using ClearDashboard.DAL.Alignment.BackgroundServices;
using Autofac;
using System.Threading;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer;
using Microsoft.Extensions.Configuration;
using ClearDashboard.Collaboration.Services;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;


public class CollaborationTests : TestBase
{
    public CollaborationTests(ITestOutputHelper output) : base(output)
    {
    }


    [Fact]
    public async void ConfigurationTest()
    {
        try
        {
            var collaborationManager = Container!.Resolve<CollaborationManager>();
            
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}