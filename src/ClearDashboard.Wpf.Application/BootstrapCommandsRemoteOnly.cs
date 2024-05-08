using Autofac;
using Autofac.Builder;
using Autofac.Features.AttributeFilters;
using ClearApi.Command.Alignment.JsonRpc.DataTransfer.Converters;
using ClearApi.Command.CommandReceivers;
using ClearApi.Command.Commands;
using ClearApi.Command.CQRS.CommandReceivers;
using ClearApi.Command.CQRS.Commands;
using ClearApi.Command.CQRS.JsonRpc.CommandReceiverProxy;
using ClearApi.Command.JsonRpc.CommandReceiverProxy;
using ClearApi.Command.JsonRpc.Network;
using ClearApi.Command.JsonRpc.Serialization;
using ClearApi.Command.Serialization;
using ClearApi.DataTransfer.Utils;
using ClearApi.Engine.Model;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application
{
	internal static class BootstrapCommandsRemoteOnly
	{
		public static void LoadModules(ContainerBuilder builder)
		{
			RegisterDataTransferTypes(builder);

			// Setting useCurrentProject to true will cause the dashboard project id to
			// be passed in ProjectCommand execute calls.  Using false tells the server
			// to use its own default project id (we're doing that for starters since we
			// don't have any initial project creation/selection functionality integrated)
			builder.RegisterType<CurrentProjectContextProvider>()
				.WithParameter("useCurrentProject", false)
				.As<IProjectCommandContextProvider>();

			builder.RegisterType<TokenizedTextCorpusDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Corpora.TokenizedTextCorpus>>();
			builder.RegisterType<ParallelCorpusDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus>>();
			builder.RegisterType<AlignmentSetDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Translation.AlignmentSet>>();
			builder.RegisterType<TranslationSetDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Translation.TranslationSet>>();

			// Just for fun, focus on remote calls only:
			builder.RegisterGeneric(typeof(MediatorCommandQueryReceiverProxy<,>)).WithNullNextReceiverAs(typeof(IMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(MediatorCommandCommandReceiverProxy<,>)).WithNullNextReceiverAs(typeof(IMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(ProjectMediatorCommandQueryReceiverProxy<,>)).WithNullNextReceiverAs(typeof(IProjectMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(ProjectMediatorCommandCommandReceiverProxy<,>)).WithNullNextReceiverAs(typeof(IProjectMediatorCommandReceiver<,>));
			builder.RegisterGeneric(typeof(ProjectCommandReceiverProxy<,>)).WithNullNextReceiverAs(typeof(IProjectCommandReceiver<,>));

			builder.RegisterChain<TokenizeTextCorpusCommandReceiverProxy, TokenizeTextCorpusCommandReceiver, ICommandReceiver<TokenizeTextCorpusCommand, TokensTextCorpus>>();
			builder.RegisterChain<AlignTokenizedCorporaCommandReceiverProxy, AlignTokenizedCorporaCommandReceiver, ICommandReceiver<AlignTokenizedCorporaCommand, TokensAlignment>>();

			builder.RegisterInstance(new BootstrapConfiguration
			{
				BootstrapConfigurationType = BootstrapConfigurationType.REMOTE
			});
		}

		public static void RegisterDataTransferTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ClearEngineClientWebSocket>()
				.WithParameter("host", "192.168.1.50:5173") // Home
				//.WithParameter("host", "172.20.10.5:5173")  // Cell hotspot
				//.WithParameter("host", "10.1.10.157:5173")  // TKD
				//.WithParameter("host", "3.145.52.38") // AWS
				.WithParameter("path", "/ws-ces")
				.AsSelf();

			builder.RegisterGeneric(typeof(JsonRpcProxy<>))
				.WithParameter(
					(pi, ctx) => pi.ParameterType == typeof(MessagePackSerializerOptions) && pi.Name == "serializerOptions",
					(pi, ctx) => ContractlessStandardResolver.Options)
				.As(typeof(IJsonRpcProxy<>))
				.Keyed(nameof(ContractlessStandardResolver), typeof(IJsonRpcProxy<>))
				.SingleInstance();

			builder.RegisterGeneric(typeof(JsonRpcProxy<>))
				.WithParameter(
					(pi, ctx) => pi.ParameterType == typeof(MessagePackSerializerOptions) && pi.Name == "serializerOptions",
					(pi, ctx) => MessagePackSerializerOptions.Standard)
				.As(typeof(IJsonRpcProxy<>))
				.Keyed(nameof(StandardResolver), typeof(IJsonRpcProxy<>))
				.SingleInstance();

			builder.RegisterType<DataTransferConverter>()
				.WithParameter(
					(pi, ctx) => pi.ParameterType == typeof(Func<System.Type, bool>),
					(pi, ctx) => (Func<System.Type, bool>)((type) => !DynamicCommandSerializer.IsMessagePackSupportedType(type))
				)
				.WithParameter("dataTransferModelNamespace", typeof(ClearApi.DataTransfer.MessagePack.Model.DynamicRequest).Namespace!)
				.WithParameter(
					(pi, ctx) => pi.ParameterType == typeof(string[]) && pi.Name == "extensionNamespaces",
					(pi, ctx) => new string[] {
						typeof(ClearApi.Command.DataTransfer.IAlignmentExtensions).Namespace!, 					// ToEngine extensions (data transfer interface to engine model)
					    typeof(ClearApi.Command.Alignment.DataTransfer.IAlignmentSetExtensions).Namespace!,		// ToEngine extensions (data transfer interface to engine model)
					    typeof(ClearApi.Command.JsonRpc.DataTransfer.NonGenericExtensions).Namespace!, 			// ToDataTransfer extensions (engine model to JsonRpc/MessagePack data transfer model)
					    typeof(ClearApi.Command.Alignment.JsonRpc.DataTransfer.NonGenericExtensions).Namespace! // ToDataTransfer extensions (engine model to JsonRpc/MessagePack data transfer model)
					})
				.WithParameter(
					(pi, ctx) => pi.ParameterType == typeof(DataTransferConverter.AnonymousType) && pi.Name == "anonymousEngineType",
					(pi, ctx) => DataTransferConverter.AnonymousType.ExpandoObject
				)
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<DataTransferTypeConstructor>().AsSelf().SingleInstance();
			builder.RegisterType<DynamicCommandSerializer>().As<IDynamicCommandSerializer>();
			builder.RegisterType<DynamicCommandReceiverProxy>()
				.WithParameter(
					(pi, ctx) => pi.ParameterType == typeof(ICommandReceiver<DynamicCommand, DynamicCommandResult>) && pi.Name == "nextReceiver",
					(pi, ctx) => null)
				.AsSelf()   // Required, if we are registering MediatorCommandReceiverProxy
				.As<ICommandReceiver<DynamicCommand, DynamicCommandResult>>()
				.WithAttributeFiltering();
		}

		public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> WithNullNextReceiverAs(this IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> registration, Type interfaceType)
		{
			return registration
				.WithParameter(
					(pi, ctx) => pi.ParameterType.IsAssignableToGenericType(interfaceType) && pi.Name == "nextReceiver",
					(pi, ctx) => null)  // If we don't explicitly set this, circular dependency error
				.As(interfaceType)
				.WithAttributeFiltering();
		}

		public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterChainWithNullNextReceiver<T,I>(this ContainerBuilder builder)
			where T : I
			where I : notnull
		{
			return
				builder.RegisterType<T>()
					.WithParameter(
						(pi, ctx) => pi.ParameterType == typeof(I) && pi.Name == "nextReceiver",
						(pi, ctx) => null)
					.As<I>()
					.WithAttributeFiltering();
		}

		public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterChain<T,U,I>(this ContainerBuilder builder)
			where T : I
			where U : I
			where I : notnull
		{
			builder.RegisterType<U>();

			return
				builder.RegisterType<T>()
					.WithParameter(
						(pi, ctx) => pi.ParameterType == typeof(I) && pi.Name == "nextReceiver",
						(pi, ctx) => ctx.Resolve<U>())
					.As<I>()
					.WithAttributeFiltering();
		}
	}
}
