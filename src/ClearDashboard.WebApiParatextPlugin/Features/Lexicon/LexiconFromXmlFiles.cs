using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using LexiconFileModel = ClearDashboard.DataAccessLayer.Models.Lexicon;
using BiblicalTermFileModel = ClearDashboard.DataAccessLayer.Models.Term;
using BiblicalTermsListFileModel = ClearDashboard.DataAccessLayer.Models.BiblicalTermsList;
using TermRenderingsListFileModel = ClearDashboard.DataAccessLayer.Models.TermRenderingsList;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.WebApiParatextPlugin.Features.Lexicon
{
    public class LexiconFromXmlFiles : ILexiconObtainable
    {
        private static readonly Dictionary<string, string> _languageCodeMappings = new()
        {
            { "english", "en" },
            { "hebrew", "he" },
            { "greek", "el" },
            { "aramaic", "arc" },
            { "mwaghavul", "sur" }
        };

        private readonly ParatextProjectMetadata? _paratextProjectMetadata;
        private readonly ILogger _logger;
        private readonly string _paratextAppPath;
        private JsonSerializerOptions _jsonSerializerOptions = new()
        {
            IncludeFields = true,
            WriteIndented = false
        };

        public LexiconFromXmlFiles(ILogger<LexiconFromXmlFiles> logger, ParatextProjectMetadata? paratextProjectMetadata, string paratextAppPath) 
        {
            _logger = logger;
            _paratextProjectMetadata = paratextProjectMetadata;
            _paratextAppPath = paratextAppPath;
        }

        public Lexicon_Lexicon GetLexicon()
        {
            var lexiconModel = new Lexicon_Lexicon();

            // 1.  [currentParatextProjectPath]\Lexicon.xml (if there is a Paratext project loaded)
            if (_paratextProjectMetadata != null)
            {
                LoadIntoLexiconModel(
                    lexiconModel,
                    LoadIntoFileModel<LexiconFileModel>(Path.Combine(_paratextProjectMetadata.ProjectPath, "Lexicon.xml")));
            }

            //  2. [currentParatextProjectPath]\ProjectBiblicalTerms.xml (? - Milt doesn't use)
            //  3. [paratextAppPath]\Terms\Lists\BiblicalTerms.xml
            //  4. [paratextAppPath]\Terms\Lists\AllBiblicalTerms.xml
            //
            //  Looks like data is extracted from 2->4, where terms from lower number
            //  take precedence over higher numbers, e.g. for a term in both 2 and 3
            //  the entry in 2 is used.

            var biblicalTermsDirectory = Path.Combine(_paratextAppPath, "Terms", "Lists");
            var biblicalTermsById = new Dictionary<string, BiblicalTermFileModel>();

            if (_paratextProjectMetadata != null)
            {
                LoadIntoLexiconModel(
                    lexiconModel,
                    biblicalTermsById,
                    LoadIntoFileModel<BiblicalTermsListFileModel>(Path.Combine(_paratextProjectMetadata.ProjectPath, "ProjectBiblicalTerms.xml")));
            }

            LoadIntoLexiconModel(
                lexiconModel,
                biblicalTermsById,
                LoadIntoFileModel<BiblicalTermsListFileModel>(Path.Combine(biblicalTermsDirectory, "BiblicalTerms.xml")));

            LoadIntoLexiconModel(
                lexiconModel,
                biblicalTermsById,
                LoadIntoFileModel<BiblicalTermsListFileModel>(Path.Combine(biblicalTermsDirectory, "AllBiblicalTerms.xml")));

            // 5. [currentParatextProjectPath]\TermRenderings.xml (if there is a Paratext project loaded)
            //
            // Each TermRendering is related to a BiblicalTerm by TermRendering.Id == BiblicalTerm.Id
            // Determine language of each by finding related BiblicalTerm
            if (_paratextProjectMetadata != null)
            {
                // FIXME:  add language code to ParatextProjectMetadata and use that instead
                if (!_languageCodeMappings.TryGetValue(_paratextProjectMetadata.LanguageName.ToLower(), out var meaningLanguage))
                {
                    meaningLanguage = _paratextProjectMetadata.LanguageName;
                }

                LoadIntoLexiconModel(
                    lexiconModel,
                    biblicalTermsById,
                    meaningLanguage,
                    LoadIntoFileModel<TermRenderingsListFileModel>(Path.Combine(_paratextProjectMetadata.ProjectPath, "TermRenderings.xml")));
            }

            return lexiconModel;
        }

        protected void LoadIntoLexiconModel(Lexicon_Lexicon lexiconModel, LexiconFileModel lexiconFileModel)
        {
            foreach (var item in lexiconFileModel.Entries.Item)
            {
                if (!_languageCodeMappings.TryGetValue(lexiconFileModel.Language.ToLower(), out var lexemeLanguage))
                {
                    lexemeLanguage = lexiconFileModel.Language;
                }

                var lexemeModel = new Lexicon_Lexeme
                {
                    Id = Guid.NewGuid(),
                    Lemma = item.Lexeme.Form,
                    Type = item.Lexeme.Type,
                    Language = lexemeLanguage,
                };

                foreach (var sense in item.Entry.Sense)
                {
                    if (!_languageCodeMappings.TryGetValue(sense.Gloss.Language.ToLower(), out var meaningLanguage))
                    {
                        meaningLanguage = sense.Gloss.Language;
                    }

                    var meaningModel = new Lexicon_Meaning
                    {
                        Id = Guid.NewGuid(),
                        Text = string.Empty, // FIXME?
                        Language = meaningLanguage,
                        LexemeId = lexemeModel.Id
                    };

                    var translationModel = new Lexicon_Translation
                    {
                        Id = Guid.NewGuid(),
                        Text = sense.Gloss.Text,
                        MeaningId = meaningModel.Id,
                        OriginatedFrom = JsonSerializer.Serialize(
                            OriginatedFromLexiconItem.FromLexiconItemFileModel(item.Lexeme, sense),
                            _jsonSerializerOptions
                        )
                    };

                    meaningModel.Translations.Add(translationModel);
                    lexemeModel.Meanings.Add(meaningModel);
                }

                lexiconModel.Lexemes.Add(lexemeModel);
            }
        }

        protected void LoadIntoLexiconModel(Lexicon_Lexicon lexiconModel, Dictionary<string, BiblicalTermFileModel> biblicalTermsById, BiblicalTermsListFileModel biblicalTermsFileModel)
        {
            var termIdsAlreadyImported = biblicalTermsById.Keys;
            foreach (var term in biblicalTermsFileModel.Term
                .Where(e => e.Gloss != null && !termIdsAlreadyImported.Contains(e.Id)))
            {
                if (!_languageCodeMappings.TryGetValue(term.Language.ToLower(), out var lexemeLanguage))
                {
                    lexemeLanguage = term.Language;
                }

                var lexemeModel = new Lexicon_Lexeme
                {
                    Id = Guid.NewGuid(),
                    Lemma = term.Id,
                    Type = lexemeLanguage,
                    Language = term.Language,
                };

                var meaningModel = new Lexicon_Meaning
                {
                    Id = Guid.NewGuid(),
                    Text = string.Empty, // FIXME?
                    Language = "en",  // Per spec:  the Gloss element is always English
                    LexemeId = lexemeModel.Id
                };

                term.Gloss.Split(';')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList()
                    .ForEach(e =>
                {
                    var translationModel = new Lexicon_Translation
                    {
                        Id = Guid.NewGuid(),
                        Text = e,
                        MeaningId = meaningModel.Id,
                        OriginatedFrom = JsonSerializer.Serialize(
                            OriginatedFromBiblicalTerm.FromBiblicalTermFileModel(term),
                            _jsonSerializerOptions
                        )
                    };

                    meaningModel.Translations.Add(translationModel);
                    lexemeModel.Meanings.Add(meaningModel);
                });

                if (lexemeModel.Meanings.Any())
                {
                    lexiconModel.Lexemes.Add(lexemeModel);
                    biblicalTermsById.Add(term.Id, term);
                }
            }
        }

        protected void LoadIntoLexiconModel(
            Lexicon_Lexicon lexiconModel, 
            Dictionary<string, BiblicalTermFileModel> biblicalTermsById, 
            string meaningLanguage,
            TermRenderingsListFileModel termRenderingsFileModel)
        {
            foreach (var termRendering in termRenderingsFileModel.TermRendering
                .Where(e => e.Renderings != null && e.Guess == false && biblicalTermsById.ContainsKey(e.Id)))
            {
                var biblicalTerm = biblicalTermsById[termRendering.Id];

                if (!_languageCodeMappings.TryGetValue(biblicalTerm.Language.ToLower(), out var lexemeLanguage))
                {
                    lexemeLanguage = biblicalTerm.Language;
                }

                var lexemeModel = new Lexicon_Lexeme
                {
                    Id = Guid.NewGuid(),
                    Lemma = termRendering.Id,
                    Type = null,
                    Language = lexemeLanguage,
                };

                var meaningModel = new Lexicon_Meaning
                {
                    Id = Guid.NewGuid(),
                    Text = string.Empty, // FIXME?
                    Language = meaningLanguage,  // Per spec:  term renderings are in the project language
                    LexemeId = lexemeModel.Id
                };

                termRendering.Renderings.Split(new string[] { "||" }, StringSplitOptions.None)
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList()
                    .ForEach(e =>
                {
                    var translationModel = new Lexicon_Translation
                    {
                        Id = Guid.NewGuid(),
                        Text = e,
                        MeaningId = meaningModel.Id,
                        OriginatedFrom = JsonSerializer.Serialize(
                            OriginatedFromTermRendering.FromTermRenderingFileModel(termRendering),
                            _jsonSerializerOptions
                        )
                    };

                    meaningModel.Translations.Add(translationModel);
                    lexemeModel.Meanings.Add(meaningModel);
                });

                if (lexemeModel.Meanings.Any())
                {
                    lexiconModel.Lexemes.Add(lexemeModel);
                }
            }
        }

        protected T LoadIntoFileModel<T>(string filePath) where T : notnull
        {
            XmlDocument doc = new();
            doc.Load(filePath);

            using (XmlNodeReader reader = new(doc))
            {
                XmlSerializer serializer = new(typeof(T));
                try
                {
                    return (T)serializer.Deserialize(reader);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error in deserialization of '{typeof(T).Name}' at path '{filePath}': " + e.Message);
                    throw e;
                }
            }
        }
    }
}
