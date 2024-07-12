using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClearDashboard.DAL.Alignment.Features.Common.DataUtil;
using ClearBible.Engine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Common;

public class SplitTokenDbCommands : IDisposable
{
	private bool disposedValue;

	public ProjectDbContext ProjectDbContext { get; init; }
	public IUserProvider UserProvider { get; init; }
	public ILogger Logger { get; init; }
	public DbConnection Connection { get; init; }
	public DbTransaction Transaction { get; init; }

	private readonly DbCommand _tokenComponentInsertCommand;
	private readonly DbCommand _tokenComponentDeleteCommand;
	private readonly DbCommand _tokenComponentUpdateSurfaceTrainingEngineTokenIdCommand;
	private readonly DbCommand _tokenCompositeTokenAssociationInsertCommand;
	private readonly DbCommand _tokenComponentSoftDeleteCommand;
	private readonly DbCommand _tokenComponentUpdateTypeCommand;
	private readonly DbCommand _tokenSubwordRenumberCommand;
	private readonly DbCommand _noteAssociationInsertCommand;
	private readonly DbCommand _noteAssociationDomainEntityIdSetCommand;
	private readonly DbCommand _alignmentSourceIdSetCommand;
	private readonly DbCommand _alignmentTargetIdSetCommand;
	private readonly DbCommand _translationSourceIdSetCommand;
	private readonly DbCommand _tvaTokenComponentIdSetCommand;
	private readonly DbCommand _alignmentInsertCommand;
	private readonly DbCommand _translationInsertCommand;
	private readonly DbCommand _tvaInsertCommand;

	private readonly List<Models.TokenComponent> _tokenComponentsToInsert = new();
	private readonly List<Models.TokenComponent> _tokenComponentsToDelete = new();
	private readonly List<Models.TokenComponent> _tokenComponentsToUpdateSurfaceTrainingEngineTokenId = new();
	private readonly List<Models.TokenCompositeTokenAssociation> _tokenCompositeTokenAssociationsToInsert = new();
	private readonly List<Models.TokenCompositeTokenAssociation> _tokenCompositeTokenAssociationsToRemove = new();
	private readonly List<Models.TokenComponent> _tokenComponentsToSplitSoftDelete = new();
	private readonly List<Models.TokenComponent> _tokenComponentsToUpdateType = new();
	private readonly List<Models.Token> _tokensToSubwordRenumber = new();
	private readonly List<Models.NoteDomainEntityAssociation> _noteAssociationsToInsert = new();
	private readonly List<(Token Token, IEnumerable<Models.NoteDomainEntityAssociation> NoteAssociations)> _tokenNoteAssociationsToSet = new();
	private readonly List<(Models.TokenComponent TokenComponent, IEnumerable<Models.NoteDomainEntityAssociation> NoteAssociations)> _tokenComponentNoteAssociationsToSet = new();
	private readonly List<(IEnumerable<Guid> AlignmentIds, Guid TokenComponentId)> _alignmentSourceIdsToSet = new();
	private readonly List<(IEnumerable<Guid> AlignmentIds, Guid TokenComponentId)> _alignmentTargetIdsToSet = new();
	private readonly List<(IEnumerable<Guid> TranslationIds, Guid TokenComponentId)> _translationSourceIdsToSet = new();
	private readonly List<(IEnumerable<Guid> TVAIds, Guid TokenComponentId)> _tvaTokenComponentIdsToSet = new();
	private readonly List<Models.Alignment> _alignmentsToInsert = new();
	private readonly List<Models.Translation> _translationsToInsert = new();
	private readonly List<Models.TokenVerseAssociation> _tvasToInsert = new();
	private readonly Dictionary<Guid, List<string>> _sourceTrainingTextsByAlignmentSetId = new();

	private AlignmentSetSourceTrainingTextsUpdatedEvent? _denormTriggerEvent = null;

	public static async Task<SplitTokenDbCommands> CreateAsync(ProjectDbContext projectDbContext, IUserProvider userProvider, ILogger logger, CancellationToken cancellationToken)
	{
		projectDbContext.Database.OpenConnection();

		var connection = projectDbContext.Database.GetDbConnection();
		var transaction = await connection.BeginTransactionAsync(cancellationToken);

		return new SplitTokenDbCommands(projectDbContext, userProvider, logger, connection, transaction);
	}

	private SplitTokenDbCommands(ProjectDbContext projectDbContext, IUserProvider userProvider, ILogger logger, DbConnection connection, DbTransaction transaction)
	{
		ProjectDbContext = projectDbContext;
		UserProvider = userProvider;
		Logger = logger;
		Connection = connection;
		Transaction = transaction;

		_tokenComponentInsertCommand = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(connection);
		_tokenComponentDeleteCommand = TokenizedCorpusDataBuilder.CreateTokenComponentDeleteCommand(connection);
		_tokenComponentUpdateSurfaceTrainingEngineTokenIdCommand = TokenizedCorpusDataBuilder.CreateTokenComponentUpdateSurfaceTrainingEngineTokenIdCommand(connection);
		_tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataBuilder.CreateTokenCompositeTokenAssociationInsertCommand(connection);
		_tokenComponentSoftDeleteCommand = TokenizedCorpusDataBuilder.CreateSoftDeleteMetadataUpdateByIdCommand(connection);
		_tokenComponentUpdateTypeCommand = TokenizedCorpusDataBuilder.CreateTokenComponentTypeUpdateCommand(connection);
		_tokenSubwordRenumberCommand = TokenizedCorpusDataBuilder.CreateTokenSubwordRenumberCommand(connection);
		_noteAssociationInsertCommand = CreateNoteAssociationInsertCommand(connection);
		_noteAssociationDomainEntityIdSetCommand = CreateNoteAssociationDomainEntityIdSetCommand(connection);
		_alignmentSourceIdSetCommand = AlignmentUtil.CreateAlignmentSourceOrTargetIdSetCommand(connection, true);
		_alignmentTargetIdSetCommand = AlignmentUtil.CreateAlignmentSourceOrTargetIdSetCommand(connection, false);
		_translationSourceIdSetCommand = AlignmentUtil.CreateTranslationSourceIdSetCommand(connection);
		_tvaTokenComponentIdSetCommand = AlignmentUtil.CreateTVATokenComponentIdSetCommand(connection);
		_alignmentInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);
		_translationInsertCommand = AlignmentUtil.CreateTranslationInsertCommand(connection);
		_tvaInsertCommand = AlignmentUtil.CreateTokenVerseAssociationInsertCommand(connection);
	}

	public bool HasAlignmentSetChangesToDenormalize()
	{
		return _sourceTrainingTextsByAlignmentSetId.Any(kvp => kvp.Value.Count != 0);
	}

	public async Task CommitTransactionAsync(IMediator mediator, CancellationToken cancellationToken)
	{
		try
		{
			await Transaction.CommitAsync(cancellationToken);
		}
		finally
		{
			Transaction.Dispose();
		}

		if (_denormTriggerEvent is not null)
		{
			Logger.LogInformation($"Firing alignment data denormalization event");
			await mediator.Publish(_denormTriggerEvent, cancellationToken);
			_denormTriggerEvent = null;
		}
	}

	public async Task ExecuteBulkOperationsAsync(CancellationToken cancellationToken)
	{
		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - inserting {_tokenComponentsToInsert.Count} token components");
		foreach (var tokenComponent in _tokenComponentsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (tokenComponent is Models.TokenComposite)
			{
				await TokenizedCorpusDataBuilder.InsertTokenCompositeAsync((tokenComponent as Models.TokenComposite)!, _tokenComponentInsertCommand, cancellationToken);
			}
			else
			{
				await TokenizedCorpusDataBuilder.InsertTokenAsync((tokenComponent as Models.Token)!, null, _tokenComponentInsertCommand, cancellationToken);
			}
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - deleting {_tokenComponentsToDelete.Count} token components");
		foreach (var tokenComponent in _tokenComponentsToDelete)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.DeleteTokenComponentAsync(tokenComponent, _tokenComponentDeleteCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - soft deleting {_tokenComponentsToSplitSoftDelete.Count} token components");
		foreach (var tc in _tokenComponentsToSplitSoftDelete)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.SoftDeleteMetadataUpdateTokenComponentAsync(tc, _tokenComponentSoftDeleteCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - updating surface / training text and engine token id for {_tokenComponentsToUpdateSurfaceTrainingEngineTokenId.Count} token components");
		foreach (var tc in _tokenComponentsToUpdateSurfaceTrainingEngineTokenId)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.UpdateTokenComponentSurfaceTrainingTextAsync(tc, _tokenComponentUpdateSurfaceTrainingEngineTokenIdCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - updating token type for {_tokenComponentsToUpdateType.Count} token components");
		foreach (var tc in _tokenComponentsToUpdateType)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.UpdateTypeTokenComponentAsync(tc, _tokenComponentUpdateTypeCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - updating subword numbering for {_tokensToSubwordRenumber.Count} token components");
		foreach (var t in _tokensToSubwordRenumber)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.SubwordRenumberTokenAsync(t, _tokenSubwordRenumberCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - inserting {_noteAssociationsToInsert.Count} note associations");
		foreach (var na in _noteAssociationsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await InsertNoteDomainEntityAssociationAsync(na, _noteAssociationInsertCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - setting {_tokenNoteAssociationsToSet.Count} token/composite + note associations");
		foreach (var tcn in _tokenNoteAssociationsToSet)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await SetNoteAssociationsDomainEntityIdAsync(tcn.Token, tcn.NoteAssociations, _noteAssociationDomainEntityIdSetCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - setting {_tokenComponentNoteAssociationsToSet.Count} token component + note associations");
		foreach (var tcn in _tokenComponentNoteAssociationsToSet)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await SetNoteAssociationsDomainEntityIdAsync(tcn.TokenComponent, tcn.NoteAssociations, _noteAssociationDomainEntityIdSetCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - deleting {_tokenCompositeTokenAssociationsToRemove.Count} token component + token associations");
		if (_tokenCompositeTokenAssociationsToRemove.Count != 0)
		{
			await DataUtil.DeleteIdentifiableEntityAsync(
				Connection,
				typeof(Models.TokenCompositeTokenAssociation),
				_tokenCompositeTokenAssociationsToRemove.Select(e => e.Id).ToArray(),
				cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - inserting {_tokenCompositeTokenAssociationsToInsert.Count} token component + token associations");
		foreach (var t in _tokenCompositeTokenAssociationsToInsert)
		{
			t.Id = await TokenizedCorpusDataBuilder.InsertTokenCompositeTokenAssociationAsync(t.TokenId, t.TokenCompositeId, _tokenCompositeTokenAssociationInsertCommand, cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - setting {_alignmentSourceIdsToSet.Count} alignment source ids");
		foreach (var (AlignmentIds, TokenComponentId) in _alignmentSourceIdsToSet)
		{
			foreach (var alignmentId in AlignmentIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetAlignmentSourceIdAsync(alignmentId, TokenComponentId, _alignmentSourceIdSetCommand, cancellationToken);
			}
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - setting {_alignmentTargetIdsToSet.Count} alignment target ids");
		foreach (var (AlignmentIds, TokenComponentId) in _alignmentTargetIdsToSet)
		{
			foreach (var alignmentId in AlignmentIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetAlignmentTargetIdAsync(alignmentId, TokenComponentId, _alignmentTargetIdSetCommand, cancellationToken);
			}
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - setting {_translationSourceIdsToSet.Count} translation/gloss source ids");
		foreach (var (TranslationIds, TokenComponentId) in _translationSourceIdsToSet)
		{
			foreach (var translationId in TranslationIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetTranslationSourceIdAsync(translationId, TokenComponentId, _translationSourceIdSetCommand, cancellationToken);
			}
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - setting {_tvaTokenComponentIdsToSet.Count} token verse association token component ids");
		foreach (var (TVAIds, TokenComponentId) in _tvaTokenComponentIdsToSet)
		{
			foreach (var tvaId in TVAIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetTVATokenComponentIdAsync(tvaId, TokenComponentId, _tvaTokenComponentIdSetCommand, cancellationToken);
			}
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - inserting {_alignmentsToInsert.Count} alignments");
		foreach (var alignment in _alignmentsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AlignmentUtil.InsertAlignmentAsync(
				alignment,
				alignment.AlignmentSetId,
				_alignmentInsertCommand,
				Guid.Empty != alignment.UserId ? alignment.UserId : UserProvider.CurrentUser!.Id,
				cancellationToken);
		}
		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - inserting {_translationsToInsert.Count} translations/glosses");
		foreach (var translation in _translationsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AlignmentUtil.InsertTranslationAsync(
				translation,
				translation.TranslationSetId,
				_translationInsertCommand,
				Guid.Empty != translation.UserId ? translation.UserId : UserProvider.CurrentUser!.Id,
				cancellationToken);
		}

		Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - inserting {_tvasToInsert.Count} token verse associations");
		foreach (var tva in _tvasToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AlignmentUtil.InsertTokenVerseAssociationAsync(
				tva,
				_tvaInsertCommand,
				Guid.Empty != tva.UserId ? tva.UserId : UserProvider.CurrentUser!.Id,
				cancellationToken);
		}

		if (HasAlignmentSetChangesToDenormalize())
		{
			Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - inserting alignment denormalization tasks");
			using (var denormalizationTaskInsertCommand = AlignmentUtil.CreateAlignmentDenormalizationTaskInsertCommand(Connection))
			{
				foreach (var kvp in _sourceTrainingTextsByAlignmentSetId)
				{
					var sourceTexts = kvp.Value.Distinct().ToList();
					if (sourceTexts.Count <= 20)
					{
						foreach (var sourceText in sourceTexts)
						{
							cancellationToken.ThrowIfCancellationRequested();

							await AlignmentUtil.InsertAlignmentDenormalizationTaskAsync(new Models.AlignmentSetDenormalizationTask
							{
								Id = Guid.NewGuid(),
								AlignmentSetId = kvp.Key,
								SourceText = sourceText,
							}, denormalizationTaskInsertCommand, cancellationToken);
						}
					}
					else
					{
						cancellationToken.ThrowIfCancellationRequested();

						await AlignmentUtil.InsertAlignmentDenormalizationTaskAsync(new Models.AlignmentSetDenormalizationTask
						{
							Id = Guid.NewGuid(),
							AlignmentSetId = kvp.Key,
							SourceText = null,
						}, denormalizationTaskInsertCommand, cancellationToken);

					}
				}
			}

			AddToDenormTriggerEvent(_sourceTrainingTextsByAlignmentSetId);
		}
		else
		{
			Logger.LogInformation($"{nameof(SplitTokenDbCommands)} - no alignment set data to denormalize");
		}

		_tokenComponentsToInsert.Clear();
		_tokenComponentsToDelete.Clear();
		_tokenComponentsToUpdateSurfaceTrainingEngineTokenId.Clear();
		_tokenComponentsToSplitSoftDelete.Clear();
		_tokenComponentsToUpdateType.Clear();
		_tokensToSubwordRenumber.Clear();
		_noteAssociationsToInsert.Clear();
		_tokenNoteAssociationsToSet.Clear();
		_tokenComponentNoteAssociationsToSet.Clear();
		_tokenCompositeTokenAssociationsToRemove.Clear();
		_tokenCompositeTokenAssociationsToInsert.Clear();
		_alignmentSourceIdsToSet.Clear();
		_alignmentTargetIdsToSet.Clear();
		_translationSourceIdsToSet.Clear();
		_tvaTokenComponentIdsToSet.Clear();
		_alignmentsToInsert.Clear();
		_translationsToInsert.Clear();
		_tvasToInsert.Clear();
		_sourceTrainingTextsByAlignmentSetId.Clear();
	}

	private void AddToDenormTriggerEvent(IDictionary<Guid, List<string>> source)
	{
		if (_denormTriggerEvent is null)
		{
			_denormTriggerEvent = new(source);
		}
		else
		{
			foreach (var kvp in source)
			{
				if (_denormTriggerEvent.SourceTrainingTextsByAlignmentSetId.TryGetValue(kvp.Key, out var sourceTrainingTexts))
				{
					foreach (var text in kvp.Value)
					{
						if (!sourceTrainingTexts.Contains(text))
						{
							sourceTrainingTexts.Add(text);
						}
					}
				}
				else
				{
					_denormTriggerEvent.SourceTrainingTextsByAlignmentSetId.Add(kvp.Key, new(kvp.Value));
				}
			}
		}
	}

	public void AddTokenComponentToInsert(Models.TokenComponent tokenComponent)
	{
		_tokenComponentsToInsert.Add(tokenComponent);
	}

	public void AddTokenCompositeChildrenToDelete(Models.TokenComposite tokenComposite)
	{
		_tokenComponentsToDelete.AddRange(tokenComposite.Tokens);
	}

	public void AddTokenComponentsToInsert(IEnumerable<Models.TokenComponent> tokenComponents)
	{
		_tokenComponentsToInsert.AddRange(tokenComponents);
	}

	public void AddTokenComponentToUpdateSurfaceTrainingEngineTokenId(Models.TokenComponent tokenComponent)
	{
		_tokenComponentsToUpdateSurfaceTrainingEngineTokenId.Add(tokenComponent);
	}

	public void AddTokenComponentToSplitSoftDelete(Models.TokenComponent tokenComponent)
	{
		_tokenComponentsToSplitSoftDelete.Add(tokenComponent);
	}

	public void AddTokenComponentToUpdateType(Models.TokenComponent tokenComponent)
	{
		_tokenComponentsToUpdateType.Add(tokenComponent);
	}

	public void AddTokenToSubwordRenumber(Models.Token token)
	{
		_tokensToSubwordRenumber.Add(token);
	}

	public void AddNoteAssociationToInsert(Models.NoteDomainEntityAssociation noteAssociation)
	{
		_noteAssociationsToInsert.Add(noteAssociation);
	}

	public void AddTokenNoteAssociationsToSet(Token token, IEnumerable<Models.NoteDomainEntityAssociation> noteAssociations)
	{
		_tokenNoteAssociationsToSet.Add((token, noteAssociations));
	}

	public void AddTokenComponentNoteAssociationsToSet(Models.TokenComponent tokenComponent, IEnumerable<Models.NoteDomainEntityAssociation> noteAssociations)
	{
		_tokenComponentNoteAssociationsToSet.Add((tokenComponent, noteAssociations));
	}

	public void AddTokenCompositeTokenAssociationsToInsert(IEnumerable<Models.TokenCompositeTokenAssociation> tokenAssociations)
	{
		_tokenCompositeTokenAssociationsToInsert.AddRange(tokenAssociations);
	}

	public void AddTokenCompositeTokenAssociationsToRemove(IEnumerable<Models.TokenCompositeTokenAssociation> tokenAssociations)
	{
		_tokenCompositeTokenAssociationsToRemove.AddRange(tokenAssociations);
	}

	public void AddAlignmentSourceIdsToSet(IEnumerable<Models.Alignment> alignments, Models.TokenComponent sourceTokenComponent)
	{
		_alignmentSourceIdsToSet.Add((alignments.Select(e => e.Id), sourceTokenComponent.Id));
	}

	public void AddAlignmentTargetIdsToSet(IEnumerable<Models.Alignment> alignments, Models.TokenComponent targetTokenComponent)
	{
		_alignmentTargetIdsToSet.Add((alignments.Select(e => e.Id), targetTokenComponent.Id));
	}

	public void AddTranslationSourceIdsToSet(IEnumerable<Models.Translation> translations, Models.TokenComponent sourceTokenComponent)
	{
		_translationSourceIdsToSet.Add((translations.Select(e => e.Id), sourceTokenComponent.Id));
	}

	public void AddTVATokenComponentIdsToSet(IEnumerable<Models.TokenVerseAssociation> tvas, Models.TokenComponent tokenComponent)
	{
		_tvaTokenComponentIdsToSet.Add((tvas.Select(e => e.Id), tokenComponent.Id));
	}

	public void AddAlignmentToInsert(Models.Alignment alignment)
	{
		_alignmentsToInsert.Add(alignment);
	}

	public void AddTranslationToInsert(Models.Translation translation)
	{
		_translationsToInsert.Add(translation);
	}

	public void AddTokenVerseAssociationToInsert(Models.TokenVerseAssociation tokenVerseAssociation)
	{
		_tvasToInsert.Add(tokenVerseAssociation);
	}

	public void AddAlignmentTrainingTextChange(IEnumerable<Models.Alignment> alignments, string? previousTrainingText, string? newTrainingText)
	{
		if (string.IsNullOrEmpty(previousTrainingText) && string.IsNullOrEmpty(newTrainingText))
		{
			return;
		}

		foreach (var e in alignments)
		{
			if (_sourceTrainingTextsByAlignmentSetId.TryGetValue(e.AlignmentSetId, out var sourceTrainingTexts))
			{
				if (!string.IsNullOrEmpty(previousTrainingText) && !sourceTrainingTexts.Contains(previousTrainingText)) sourceTrainingTexts.Add(previousTrainingText);
				if (!string.IsNullOrEmpty(newTrainingText) && !sourceTrainingTexts.Contains(newTrainingText)) sourceTrainingTexts.Add(newTrainingText);
			}
			else
			{
				sourceTrainingTexts = new();

				if (!string.IsNullOrEmpty(previousTrainingText) && !sourceTrainingTexts.Contains(previousTrainingText)) sourceTrainingTexts.Add(previousTrainingText);
				if (!string.IsNullOrEmpty(newTrainingText) && !sourceTrainingTexts.Contains(newTrainingText)) sourceTrainingTexts.Add(newTrainingText);

				_sourceTrainingTextsByAlignmentSetId.Add(e.AlignmentSetId, sourceTrainingTexts);
			}
		}
	}

	private static DbCommand CreateNoteAssociationInsertCommand(DbConnection connection)
	{
		var command = connection.CreateCommand();
		var columns = new string[] { "Id", "NoteId", "DomainEntityIdGuid", "DomainEntityIdName" };

		DataUtil.ApplyColumnsToInsertCommand(command, typeof(Models.NoteDomainEntityAssociation), columns);

		command.Prepare();

		return command;
	}

	private static async Task InsertNoteDomainEntityAssociationAsync(Models.NoteDomainEntityAssociation noteAssociation, DbCommand command, CancellationToken cancellationToken)
	{
		command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = noteAssociation.Id;
		command.Parameters[$"@{nameof(Models.NoteDomainEntityAssociation.NoteId)}"].Value = noteAssociation.NoteId;
		command.Parameters[$"@{nameof(Models.NoteDomainEntityAssociation.DomainEntityIdGuid)}"].Value = noteAssociation.DomainEntityIdGuid;
		command.Parameters[$"@{nameof(Models.NoteDomainEntityAssociation.DomainEntityIdName)}"].Value = noteAssociation.DomainEntityIdName;
		_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
	}

	private static DbCommand CreateNoteAssociationDomainEntityIdSetCommand(DbConnection connection)
	{
		var command = connection.CreateCommand();
		var columns = new string[] { nameof(Models.NoteDomainEntityAssociation.DomainEntityIdGuid) };
		var whereColumns = new (string, WhereEquality)[] { (nameof(Models.IdentifiableEntity.Id), WhereEquality.Equals) };

		DataUtil.ApplyColumnsToUpdateCommand(
			command,
			typeof(Models.NoteDomainEntityAssociation),
			columns,
			whereColumns,
			Array.Empty<(string, int)>());

		command.Prepare();

		return command;
	}

	private async Task SetNoteAssociationsDomainEntityIdAsync(Token token, IEnumerable<Models.NoteDomainEntityAssociation> noteAssociations, DbCommand command, CancellationToken cancellationToken)
	{
		foreach (var noteAssociation in noteAssociations)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// Keep DbContext model in sync:
			noteAssociation.DomainEntityIdGuid = token.TokenId.Id;

			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = noteAssociation.Id;
			command.Parameters[$"@{nameof(Models.NoteDomainEntityAssociation.DomainEntityIdGuid)}"].Value = token.TokenId.Id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	private async Task SetNoteAssociationsDomainEntityIdAsync(Models.TokenComponent tokenComponent, IEnumerable<Models.NoteDomainEntityAssociation> noteAssociations, DbCommand command, CancellationToken cancellationToken)
	{
		foreach (var noteAssociation in noteAssociations)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// Keep DbContext model in sync:
			noteAssociation.DomainEntityIdGuid = tokenComponent.Id;

			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = noteAssociation.Id;
			command.Parameters[$"@{nameof(Models.NoteDomainEntityAssociation.DomainEntityIdGuid)}"].Value = tokenComponent.Id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_tokenComponentInsertCommand.Dispose();
				_tokenComponentDeleteCommand.Dispose();
				_tokenComponentUpdateSurfaceTrainingEngineTokenIdCommand.Dispose();
				_tokenCompositeTokenAssociationInsertCommand.Dispose();
				_tokenComponentSoftDeleteCommand.Dispose();
				_tokenComponentUpdateTypeCommand.Dispose();
				_tokenSubwordRenumberCommand.Dispose();
				_noteAssociationInsertCommand.Dispose();
				_noteAssociationDomainEntityIdSetCommand.Dispose();
				_alignmentSourceIdSetCommand.Dispose();
				_alignmentTargetIdSetCommand.Dispose();
				_translationSourceIdSetCommand.Dispose();
				_tvaTokenComponentIdSetCommand.Dispose();
				_alignmentInsertCommand.Dispose();
				_translationInsertCommand.Dispose();
				_tvaInsertCommand.Dispose();

				Transaction.Dispose();

				ProjectDbContext.Database.CloseConnection();
				Connection.Dispose();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}