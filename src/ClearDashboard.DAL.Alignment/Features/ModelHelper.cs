﻿using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features
{
    public static class ModelHelper
    {
        public static CompositeToken BuildCompositeToken(Models.TokenComposite tokenComposite, IEnumerable<Models.Token> tokens)
        {
            var ct = new CompositeToken(tokens.Select(t => BuildToken(t)));
            ct.TokenId.Id = tokenComposite.Id;
            ct.ExtendedProperties = tokenComposite.ExtendedProperties;

            return ct;
        }

        public static Token BuildToken(Models.TokenComponent tokenComponent)
        {
            if (tokenComponent is Models.TokenComposite)
            {
                var tokenComposite = (tokenComponent as Models.TokenComposite)!;

                var ct = new CompositeToken(tokenComposite.Tokens.Select(t => BuildToken(t)));
                ct.TokenId.Id = tokenComponent.Id;
                ct.ExtendedProperties = tokenComponent.ExtendedProperties;

                return ct;
            }
            else
            {
                var token = (tokenComponent as Models.Token)!;
                return new Token(
                    ModelHelper.BuildTokenId(token),
                    token.SurfaceText ?? string.Empty,
                    token.TrainingText ?? string.Empty)
                {
                    ExtendedProperties = token.ExtendedProperties
                };
            }
        }
        public static TokenId BuildTokenId(Models.TokenComponent tokenComponent)
        {
            if (tokenComponent is Models.TokenComposite)
            {
                var tokenComposite = (tokenComponent as Models.TokenComposite)!;
                return new CompositeTokenId(tokenComposite.Tokens.Select(t => BuildToken(t)))
                {
                    Id = tokenComponent.Id,
                };
            }
            else
            {
                var token = (tokenComponent as Models.Token)!;
                return new TokenId(
                    token.BookNumber,
                    token.ChapterNumber,
                    token.VerseNumber,
                    token.WordNumber,
                    token.SubwordNumber)
                {
                    Id = tokenComponent.Id
                };
            }
        }
        public static bool IsTokenIdLocationMatch(TokenId tokenId, Models.Token dbToken)
        {
            return (dbToken.BookNumber == tokenId.BookNumber &&
                    dbToken.ChapterNumber == tokenId.ChapterNumber &&
                    dbToken.VerseNumber == tokenId.VerseNumber &&
                    dbToken.WordNumber == tokenId.WordNumber &&
                    dbToken.SubwordNumber == tokenId.SubWordNumber);
        }

        public static string BuildTokenLocationRef(Models.Token token)
        {
            return $"{token.BookNumber:000}{token.ChapterNumber:000}{token.VerseNumber:000}{token.WordNumber:000}{token.SubwordNumber:000}";
        }

        public static IQueryable<Models.Corpus> AddIdIncludesCorpaQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.Corpa
                .Include(e => e.User);
        }

        public static CorpusId BuildCorpusId(Models.Corpus corpus)
        {
            if (corpus.User == null)
            {
                throw new MediatorErrorEngineException("DB Corpus passed to BuildCorpusId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
            }

            return BuildCorpusId(corpus, corpus.User);
        }

        public static CorpusId BuildCorpusId(Models.Corpus corpus, Models.User user)
        {
            return new CorpusId(
                corpus.Id,
                corpus.IsRtl,
                corpus.FontFamily,
                corpus.Name,
                corpus.DisplayName,
                corpus.Language,
                corpus.ParatextGuid,
                corpus.CorpusType.ToString(),
                corpus.Metadata,
                corpus.Created,
                BuildUserId(user));
        }
        public static CorpusId BuildCorpusId(Models.TokenizedCorpus tokenizedCorpus)
        {
            if (tokenizedCorpus.Corpus == null)
            {
                throw new MediatorErrorEngineException("DB TokenizedCorpus passed to BuildCorpusId does not contain a Corpus.  Please ensure the necessary EFCore/Linq Include() method is called");
            }
            return BuildCorpusId(tokenizedCorpus.Corpus);
        }

        public static UserId BuildUserId(Models.User user)
        {
            return new UserId(user.Id, user.FullName ?? string.Empty);
        }

        public static IQueryable<Models.TokenizedCorpus> AddIdIncludesTokenizedCorpaQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.TokenizedCorpora
                .Include(e => e.User)
                .Include(e => e.Corpus)
                    .ThenInclude(c => c!.User);
        }

        public static TokenizedTextCorpusId BuildTokenizedTextCorpusId(Models.TokenizedCorpus tokenizedCorpus)
        {
            if (tokenizedCorpus.Corpus == null)
            {
                throw new MediatorErrorEngineException("DB TokenizedCorpus passed to BuildTokenizedTextCorpusId does not contain a Corpus.  Please ensure the necessary EFCore/Linq Include() method is called");
            }

            if (tokenizedCorpus.User == null)
            {
                throw new MediatorErrorEngineException("DB TokenizedCorpus passed to BuildTokenizedTextCorpusId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
            }
            return BuildTokenizedTextCorpusId(tokenizedCorpus, tokenizedCorpus.Corpus, tokenizedCorpus.User);
        }

        public static TokenizedTextCorpusId BuildTokenizedTextCorpusId(Models.TokenizedCorpus tokenizedCorpus, Models.Corpus corpus, Models.User user)
        {
            return new TokenizedTextCorpusId(
                tokenizedCorpus.Id,
                BuildCorpusId(corpus),
                tokenizedCorpus.DisplayName,
                tokenizedCorpus.TokenizationFunction,
                tokenizedCorpus.Metadata,
                tokenizedCorpus.Created,
                BuildUserId(user));
        }

        public static IQueryable<Models.ParallelCorpus> AddIdIncludesParallelCorpaQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.ParallelCorpa
                .Include(e => e.User)
                .Include(e => e.SourceTokenizedCorpus)
                    .ThenInclude(tc => tc!.User)
                .Include(e => e.SourceTokenizedCorpus)
                    .ThenInclude(tc => tc!.Corpus)
                        .ThenInclude(c => c!.User)
                .Include(e => e.TargetTokenizedCorpus)
                    .ThenInclude(tc => tc!.User)
                .Include(e => e.TargetTokenizedCorpus)
                    .ThenInclude(tc => tc!.Corpus)
                        .ThenInclude(c => c!.User);
        }

        public static ParallelCorpusId BuildParallelCorpusId(Models.ParallelCorpus parallelCorpus)
        {
            if (parallelCorpus.SourceTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB ParallelCorpus passed to BuildParallelCorpusId does not contain a SourceTokenizedCorpus.  Please ensure the necessary EFCore/Linq Include() method is called");
            }
            if (parallelCorpus.TargetTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB ParallelCorpus passed to BuildParallelCorpusId does not contain a TargetTokenizedCorpus.  Please ensure all necessary EFCore/Linq Include() method is called");
            }
            if (parallelCorpus.User == null)
            {
                throw new MediatorErrorEngineException("DB ParallelCorpus passed to BuildParallelCorpusId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
            }

            return BuildParallelCorpusId(parallelCorpus, parallelCorpus.SourceTokenizedCorpus, parallelCorpus.TargetTokenizedCorpus, parallelCorpus.User);
        }

        public static ParallelCorpusId BuildParallelCorpusId(Models.ParallelCorpus parallelCorpus, Models.TokenizedCorpus sourceTokenizedCorpus, Models.TokenizedCorpus targetTokenizedCorpus, Models.User user)
        {
            return new ParallelCorpusId(
                parallelCorpus.Id,
                BuildTokenizedTextCorpusId(sourceTokenizedCorpus),
                BuildTokenizedTextCorpusId(targetTokenizedCorpus),
                parallelCorpus.DisplayName,
                parallelCorpus.Metadata,
                parallelCorpus.Created,
                BuildUserId(user));
        }

        public static IQueryable<Models.AlignmentSet> AddIdIncludesAlignmentSetsQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.AlignmentSets
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                        .ThenInclude(tc => tc!.Corpus)
                            .ThenInclude(c => c!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                        .ThenInclude(tc => tc!.Corpus)
                            .ThenInclude(c => c!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.User)
                .Include(ast => ast.User);
        }

        public static AlignmentSetId BuildAlignmentSetId(Models.AlignmentSet alignmentSet)
        {
            if (alignmentSet.ParallelCorpus == null)
            {
                throw new MediatorErrorEngineException("DB AlignmentSet passed to BuildAlignmentSetId does not contain a ParallelCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (alignmentSet.User == null)
            {
                throw new MediatorErrorEngineException("DB AlignmentSet passed to BuildAlignmentSetId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
            }

            return BuildAlignmentSetId(alignmentSet, BuildParallelCorpusId(alignmentSet.ParallelCorpus), alignmentSet.User);
        }

        public static AlignmentSetId BuildAlignmentSetId(Models.AlignmentSet alignmentSet, ParallelCorpusId parallelCorpusId, Models.User user)
        {
            return new AlignmentSetId(
                alignmentSet.Id,
                parallelCorpusId,
                alignmentSet.DisplayName,
                alignmentSet.SmtModel,
                alignmentSet.IsSyntaxTreeAlignerRefined,
                alignmentSet.Metadata,
                alignmentSet.Created,
                BuildUserId(user));
        }

        public static IQueryable<Models.Alignment> AddIdIncludesAlignmentsQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.Alignments
                .Include(e => e.SourceTokenComponent)
                .Include(e => e.AlignmentSet)
                    .ThenInclude(e => e!.ParallelCorpus)
                        .ThenInclude(e => e!.SourceTokenizedCorpus)
                .Include(e => e.AlignmentSet)
                    .ThenInclude(e => e!.ParallelCorpus)
                        .ThenInclude(e => e!.TargetTokenizedCorpus);
        }

        public static AlignmentId BuildAlignmentId(Models.Alignment alignment)
        {
            if (alignment.SourceTokenComponent == null)
            {
                throw new MediatorErrorEngineException("DB Alignment passed to BuildAlignmentId does not contain a SourceTokenComponent.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (alignment.AlignmentSet?.ParallelCorpus == null)
            {
                throw new MediatorErrorEngineException("DB Alignment passed to BuildAlignmentId does not contain a parent AlignmentSet with a ParallelCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (alignment.AlignmentSet?.ParallelCorpus.SourceTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB Alignment passed to BuildAlignmentId does not contain a parent AlignmentSet with a ParallelCorpus that has a SourceTokenizedCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (alignment.AlignmentSet?.ParallelCorpus.TargetTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB Alignment passed to BuildAlignmentId does not contain a parent AlignmentSet with a ParallelCorpus that has a TargetTokenizedCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }

            return BuildAlignmentId(
                alignment,
                alignment.AlignmentSet.ParallelCorpus.SourceTokenizedCorpus,
                alignment.AlignmentSet.ParallelCorpus.TargetTokenizedCorpus,
                alignment.SourceTokenComponent);
        }

        public static AlignmentId BuildAlignmentId(Models.Alignment alignment, Models.TokenizedCorpus sourceTokenizedCorpus, Models.TokenizedCorpus targetTokenizedCorpus, Models.TokenComponent sourceTokenComponent)
        {
            return new AlignmentId(
                alignment.Id,
                sourceTokenizedCorpus.DisplayName ?? string.Empty,
                targetTokenizedCorpus.DisplayName ?? string.Empty,
                ModelHelper.BuildTokenId(sourceTokenComponent));
        }

        public static IQueryable<Models.TranslationSet> AddIdIncludesTranslationSetsQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.TranslationSets
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                        .ThenInclude(tc => tc!.Corpus)
                            .ThenInclude(c => c!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                        .ThenInclude(tc => tc!.Corpus)
                            .ThenInclude(c => c!.User)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.User)
                .Include(ast => ast.User);
        }

        public static TranslationSetId BuildTranslationSetId(Models.TranslationSet translationSet)
        {
            if (translationSet.ParallelCorpus == null)
            {
                throw new MediatorErrorEngineException("DB TranslationSet passed to BuildTranslationSetId does not contain a ParallelCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (translationSet.User == null)
            {
                throw new MediatorErrorEngineException("DB TranslationSet passed to BuildTranslationSetId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
            }

            return BuildTranslationSetId(translationSet, BuildParallelCorpusId(translationSet.ParallelCorpus), translationSet.User);
        }

        public static TranslationSetId BuildTranslationSetId(Models.TranslationSet translationSet, ParallelCorpusId parallelCorpusId, Models.User user)
        {
            return new TranslationSetId(
                translationSet.Id,
                parallelCorpusId,
                translationSet.DisplayName,
                //translationSet.SmtModel,
                translationSet.Metadata,
                translationSet.Created,
                BuildUserId(user));
        }

        public static IQueryable<Models.Translation> AddIdIncludesTranslationsQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.Translations
                .Include(e => e.SourceTokenComponent)
                .Include(e => e.TranslationSet)
                    .ThenInclude(e => e!.ParallelCorpus)
                        .ThenInclude(e => e!.SourceTokenizedCorpus)
                .Include(e => e.TranslationSet)
                    .ThenInclude(e => e!.ParallelCorpus)
                        .ThenInclude(e => e!.TargetTokenizedCorpus);
        }

        public static TranslationId BuildTranslationId(Models.Translation translation)
        {
            if (translation.SourceTokenComponent == null)
            {
                throw new MediatorErrorEngineException("DB Translation passed to BuildTranslationId does not contain a SourceTokenComponent.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (translation.TranslationSet?.ParallelCorpus == null)
            {
                throw new MediatorErrorEngineException("DB Translation passed to BuildTranslationId does not contain a parent TranslationSet with a ParallelCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (translation.TranslationSet?.ParallelCorpus.SourceTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB Translation passed to BuildTranslationId does not contain a parent TranslationSet with a ParallelCorpus that has a SourceTokenizedCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (translation.TranslationSet?.ParallelCorpus.TargetTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB Translation passed to BuildTranslationId does not contain a parent TranslationSet with a ParallelCorpus that has a TargetTokenizedCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }

            return BuildTranslationId(
                translation, 
                translation.TranslationSet.ParallelCorpus.SourceTokenizedCorpus,
                translation.TranslationSet.ParallelCorpus.TargetTokenizedCorpus,
                translation.SourceTokenComponent);
        }

        public static TranslationId BuildTranslationId(Models.Translation translation, Models.TokenizedCorpus sourceTokenizedCorpus, Models.TokenizedCorpus targetTokenizedCorpus, Models.TokenComponent sourceTokenComponent)
        {
            return new TranslationId(
                translation.Id,
                sourceTokenizedCorpus.DisplayName ?? string.Empty,
                targetTokenizedCorpus.DisplayName ?? string.Empty,
                ModelHelper.BuildTokenId(sourceTokenComponent));
        }

        public static IQueryable<Models.Note> AddIdIncludesNotesQuery(ProjectDbContext projectDbContext)
        {
            return projectDbContext.Notes.Include(n => n!.User);
        }

        public static NoteId BuildNoteId(Models.Note note)
        {
            if (note.User == null)
            {
                throw new MediatorErrorEngineException("DB Note passed to BuildNoteId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
            }

            return new NoteId(
                note.Id,
                note.Created,
                note.Modified,
                BuildUserId(note.User!));
        }

        public static Note BuildNote(Models.Note note)
        {
            return new Note(
                BuildNoteId(note),
                note.Text!,
                note.AbbreviatedText,
                note.NoteStatus,
                (note.ThreadId is not null) ? new EntityId<NoteId>() { Id = note.ThreadId.Value } : null,
                note.LabelNoteAssociations
                    .Select(ln => new Label(new LabelId(ln.Label!.Id), ln.Label!.Text ?? string.Empty)).ToHashSet(),
                note.NoteDomainEntityAssociations
                    .Select(nd => nd.DomainEntityIdName!.CreateInstanceByNameAndSetId((Guid)nd.DomainEntityIdGuid!)).ToHashSet()
            );
        }

        public static Type? FindEntityIdGenericType(this Type givenType)
        {
            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == typeof(EntityId<>))
            {
                var genericArgs = givenType.GetGenericArguments();
                if (genericArgs.Length == 1) return genericArgs[0];
            }

            Type? baseType = givenType.BaseType;
            if (baseType == null) return null;

            return baseType.FindEntityIdGenericType();
        }

    }
}
