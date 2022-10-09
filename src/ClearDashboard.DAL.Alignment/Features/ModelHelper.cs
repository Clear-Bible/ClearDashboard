using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features
{
    public static class ModelHelper
    {
        public static CompositeToken BuildCompositeToken(Guid tokenCompositeId, IEnumerable<Models.Token> tokens)
        {
            var ct = new CompositeToken(tokens.Select(t => BuildToken(t)));
            ct.TokenId.Id = tokenCompositeId;

            return ct;
        }

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
                    token.TrainingText ?? string.Empty)
                {
                    PropertiesJson = token.PropertiesJson
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

        public static UserId BuildUserId(Models.User user)
        {
            return new UserId(user.Id, user.FullName ?? string.Empty);
        }

        public static TokenizedTextCorpusId BuildTokenizedTextCorpusId(Models.TokenizedCorpus tokenizedCorpus)
        {
            if (tokenizedCorpus.User == null)
            {
                throw new MediatorErrorEngineException("DB TokenizedCorpus passed to BuildTokenizedTextCorpusId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
            }

            return new TokenizedTextCorpusId(
                tokenizedCorpus.Id,
                tokenizedCorpus.DisplayName,
                tokenizedCorpus.TokenizationFunction,
                tokenizedCorpus.Metadata,
                tokenizedCorpus.Created,
                BuildUserId(tokenizedCorpus.User));
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
                throw new MediatorErrorEngineException("DB ParallelCorpus passed to BuildTokenizedTextCorpusId does not contain a User.  Please ensure the necessary EFCore/Linq Include() method is called");
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
                new NoteId(
                    note.Id,
                    note.Created,
                    note.Modified,
                    ModelHelper.BuildUserId(note.User!)),
                note.Text!,
                note.AbbreviatedText,
                (note.ThreadId is not null) ? new EntityId<NoteId>() { Id = note.ThreadId.Value } : null,
                note.LabelNoteAssociations
                    .Select(ln => new Label(new LabelId(ln.Label!.Id), ln.Label!.Text ?? string.Empty)).ToHashSet(),
                note.NoteDomainEntityAssociations
                    .Select(nd => nd.DomainEntityIdName!.CreateInstanceByNameAndSetId((Guid)nd.DomainEntityIdGuid!)).ToHashSet()
            );
        }
    }
}
