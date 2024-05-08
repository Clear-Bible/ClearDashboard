Using the Upgrade assistant, upgrade the projects in the following order:

*Dashboard projects*
1. ClearDashboard.Wpf.Application
1. ClearDashboard.Wpf.Application.Abstractions
        
        a. updates ClearDashboard.DAL.Alignment
1. ClearDashboard.Wpf.Controls
1. ClearDashboard.DAL.ViewModels
1. ClearDashboard.DAL.Interfaces
1. ClearDashboard.DAL.Data
1. ClearDashboard.DAL.CQRS.Features
1. ClearDashboard.DAL
1. ClearDashboard.DAL.Collaboration

*Modules*
1. ClearDashboard.Aqua.Module
1. ClearDashboard.Aqua.Module.Tests
1. ClearDashboard.Sample.Module

*Tests*
1. ClearDashboard.DAL.Alignment.Tests
1. ClearDashboard.DAL.Tests
1. ClearDashboard.WebApiParatextPlugin.Tests
1. ClearDashboard.WPF.Application.Abstractions.Tests
1. ClearDashboard.WPF.Tests

*Tools*
1. GenerateLicenseKeyForDashboard
1. GenerateSemantiDomainLookupSqlite
1. PluginManager
1. ResetCurrentUser
1. SetVersionInfo


.Net Standard or .Net framework 4.8* Projects not upgraded:

*Dashboard projects*
1. ClearDashboard.DAL.Models
1. ClearDashboard.DAL.CQRS

*Plugin Projects*
1. ClearDashboard.WebApiParatextPlugin*
1. ClearDashboard.ParatextPlugin.CQRS

*Tools*
1. CompressTreeXML -  no longer used