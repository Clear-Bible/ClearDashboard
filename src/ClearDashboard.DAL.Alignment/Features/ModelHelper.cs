using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features
{
    public static class ModelHelper
    {
        public static Token BuildToken(Models.TokenComponent tokenComponent)
        {
            if (tokenComponent is Models.TokenComposite)
            {
                var tokenComposite = (tokenComponent as Models.TokenComposite)!;

                var ct = new CompositeToken(tokenComposite.Tokens.Select(t => BuildToken(t)));
                ct.TokenId.Id = tokenComponent.Id;

                return ct;
            }
            else
            {
                var token = (tokenComponent as Models.Token)!;
                return new Token(
                    ModelHelper.BuildTokenId(token),
                    token.SurfaceText ?? string.Empty,
                    token.TrainingText ?? string.Empty);
            }
        }
        public static TokenId BuildTokenId(Models.TokenComponent tokenComponent)
        {
            if (tokenComponent is Models.TokenComposite)
            {
                var tokenComposite = (tokenComponent as Models.TokenComposite)!;
                return new CompositeTokenId(tokenComposite.Tokens.Select(t => BuildToken(t)))
                {
                    Id = tokenComponent.Id
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

        public static CorpusId BuildCorpusId(Models.Corpus corpus)
        {
            return new CorpusId(corpus.Id);
        }
        public static CorpusId BuildCorpusId(Models.TokenizedCorpus tokenizedCorpus)
        {
            if (tokenizedCorpus.Corpus == null)
            {
                throw new MediatorErrorEngineException("DB TokenizedCorpus passed to BuildCorpusId does not contain a Corpus.  Please ensure the necessary EFCore/Linq Include() method is called");
            }
            return BuildCorpusId(tokenizedCorpus.Corpus);
        }

        public static UserId BuildUserId(Models.IdentifiableEntity identifiableEntity)
        {
            return new UserId(identifiableEntity.Id);
        }

        public static TokenizedTextCorpusId BuildTokenizedTextCorpusId(Models.TokenizedCorpus tokenizedCorpus)
        {
            return new TokenizedTextCorpusId(
                tokenizedCorpus.Id,
                tokenizedCorpus.DisplayName,
                tokenizedCorpus.TokenizationFunction,
                tokenizedCorpus.Metadata,
                tokenizedCorpus.Created,
                BuildUserId(tokenizedCorpus));
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

            return BuildParallelCorpusId(parallelCorpus, parallelCorpus.SourceTokenizedCorpus, parallelCorpus.TargetTokenizedCorpus);
        }

        public static ParallelCorpusId BuildParallelCorpusId(Models.ParallelCorpus parallelCorpus, Models.TokenizedCorpus sourceTokenizedCorpus, Models.TokenizedCorpus targetTokenizedCorpus)
        {
            return new ParallelCorpusId(
                parallelCorpus.Id,
                BuildTokenizedTextCorpusId(sourceTokenizedCorpus),
                BuildTokenizedTextCorpusId(targetTokenizedCorpus),
                parallelCorpus.DisplayName,
                parallelCorpus.Metadata,
                parallelCorpus.Created,
                BuildUserId(parallelCorpus));
        }

        public static AlignmentSetId BuildAlignmentSetId(Models.AlignmentSet alignmentSet)
        {
            if (alignmentSet.ParallelCorpus == null)
            {
                throw new MediatorErrorEngineException("DB AlignmentSet passed to BuildAlignmentSetId does not contain a ParallelCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (alignmentSet.ParallelCorpus.SourceTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB AlignmentSet passed to BuildAlignmentSetId does not contain a ParallelCorpus.SourceTokenizedCorpus.  Please ensure all necessary EFCore/Linq Include() and ThenInclude methods are called");
            }
            if (alignmentSet.ParallelCorpus.TargetTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB AlignmentSet passed to BuildAlignmentSetId does not contain a ParallelCorpus.TargetTokenizedCorpus.  Please ensure all necessary EFCore/Linq Include() and ThenInclude methods are called");
            }

            return BuildAlignmentSetId(alignmentSet, alignmentSet.ParallelCorpus, alignmentSet.ParallelCorpus.SourceTokenizedCorpus, alignmentSet.ParallelCorpus.TargetTokenizedCorpus);
        }

        public static AlignmentSetId BuildAlignmentSetId(Models.AlignmentSet alignmentSet, Models.ParallelCorpus parallelCorpus, Models.TokenizedCorpus sourceTokenizedCorpus, Models.TokenizedCorpus targetTokenizedCorpus)
        {
            return new AlignmentSetId(
                alignmentSet.Id,
                BuildParallelCorpusId(parallelCorpus, sourceTokenizedCorpus, targetTokenizedCorpus),
                alignmentSet.DisplayName,
                alignmentSet.SmtModel,
                alignmentSet.IsSyntaxTreeAlignerRefined,
                alignmentSet.Metadata,
                alignmentSet.Created,
                BuildUserId(alignmentSet));
        }

        public static TranslationSetId BuildTranslationSetId(Models.TranslationSet translationSet)
        {
            if (translationSet.ParallelCorpus == null)
            {
                throw new MediatorErrorEngineException("DB TranslationSet passed to BuildTranslationSetId does not contain a ParallelCorpus.  Please ensure the necessary EFCore/Linq Include() method are called");
            }
            if (translationSet.ParallelCorpus.SourceTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB TranslationSet passed to BuildTranslationSetId does not contain a ParallelCorpus.SourceTokenizedCorpus.  Please ensure all necessary EFCore/Linq Include() and ThenInclude methods are called");
            }
            if (translationSet.ParallelCorpus.TargetTokenizedCorpus == null)
            {
                throw new MediatorErrorEngineException("DB TranslationSet passed to BuildTranslationSetId does not contain a ParallelCorpus.TargetTokenizedCorpus.  Please ensure all necessary EFCore/Linq Include() and ThenInclude methods are called");
            }

            return BuildTranslationSetId(translationSet, translationSet.ParallelCorpus, translationSet.ParallelCorpus.SourceTokenizedCorpus, translationSet.ParallelCorpus.TargetTokenizedCorpus);
        }

        public static TranslationSetId BuildTranslationSetId(Models.TranslationSet translationSet, Models.ParallelCorpus parallelCorpus, Models.TokenizedCorpus sourceTokenizedCorpus, Models.TokenizedCorpus targetTokenizedCorpus)
        {
            return new TranslationSetId(
                translationSet.Id,
                BuildParallelCorpusId(parallelCorpus, sourceTokenizedCorpus, targetTokenizedCorpus),
                translationSet.DisplayName,
                translationSet.SmtModel,
                translationSet.Metadata,
                translationSet.Created,
                BuildUserId(translationSet));
        }
    }
}
