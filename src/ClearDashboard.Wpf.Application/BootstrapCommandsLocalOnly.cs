using Autofac;
using Autofac.Features.AttributeFilters;
using ClearApi.Command.Alignment.CommandReceivers;
using ClearApi.Command.CommandReceivers;
using ClearApi.Command.Commands;
using ClearApi.Command.CQRS.CommandReceivers;
using ClearApi.Command.CQRS.Commands;
using ClearApi.Command.CQRS.JsonRpc.CommandReceiverProxy;
using ClearApi.Command.JsonRpc.CommandReceiverProxy;
using ClearApi.Command.JsonRpc.Network;
using ClearApi.Engine.Model;
using MessagePack.Resolvers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application
{
	internal class BootstrapCommandsLocalOnly
	{
		public static void LoadModules(ContainerBuilder builder)
		{
			BootstrapCommandsRemoteOnly.RegisterDataTransferTypes(builder);

			// Setting useCurrentProject to true will cause the dashboard project id to
			// be passed in ProjectCommand execute calls.  Using false tells the server
			// to use its own default project id (we're doing that for starters since we
			// don't have any initial project creation/selection functionality integrated)
			builder.RegisterType<CurrentProjectContextProvider>()
				.WithParameter("useCurrentProject", true)
				.As<IProjectCommandContextProvider>();

			builder.RegisterChainWithNullNextReceiver<DynamicCommandReceiver, ICommandReceiver<DynamicCommand, DynamicCommandResult>>();

			builder.RegisterGeneric(typeof(MediatorCommandQueryReceiver<,>)).WithNullNextReceiverAs(typeof(IMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(MediatorCommandCommandReceiver<,>)).WithNullNextReceiverAs(typeof(IMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(ProjectMediatorCommandQueryReceiver<,>)).WithNullNextReceiverAs(typeof(IProjectMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(ProjectMediatorCommandCommandReceiver<,>)).WithNullNextReceiverAs(typeof(IProjectMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(ProjectCommandReceiver<,>)).WithNullNextReceiverAs(typeof(IProjectCommandReceiver<,>));

			builder.RegisterType<CreateCorpusCommandReceiver>().As<ICommandReceiver<CreateCorpusCommand, string>>();
			builder.RegisterType<GetVerseRangeTokensCommandReceiver>().As<ICommandReceiver<GetVerseRangeTokensCommand, (IEnumerable<PaddedTokensTextRow> Rows, int IndexOfVerse)>>();

			//builder.RegisterType<TokenizeTextCorpusCommandReceiver>().As<ICommandReceiver<TokenizeTextCorpusCommand, TokensTextCorpus>>();
			//builder.RegisterType<AlignTokenizedCorporaCommandReceiver>().As<ICommandReceiver<AlignTokenizedCorporaCommand, TokensAlignment>>();
			builder.RegisterChain<TokenizeTextCorpusCommandReceiverProxy, TokenizeTextCorpusCommandReceiver, ICommandReceiver<TokenizeTextCorpusCommand, TokensTextCorpus>>();
			builder.RegisterChain<AlignTokenizedCorporaCommandReceiverProxy, AlignTokenizedCorporaCommandReceiver, ICommandReceiver<AlignTokenizedCorporaCommand, TokensAlignment>>();

			builder.RegisterInstance(new BootstrapConfiguration
			{
				BootstrapConfigurationType = BootstrapConfigurationType.LOCAL_REMOTE_ENGINE_ONLY
			});
		}
	}
}
