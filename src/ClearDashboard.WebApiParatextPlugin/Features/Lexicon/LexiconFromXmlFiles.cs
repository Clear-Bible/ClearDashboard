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
using System.Media;
using System.Reflection;
using SIL.WritingSystems;

namespace ClearDashboard.WebApiParatextPlugin.Features.Lexicon
{
    public class LexiconFromXmlFiles : ILexiconObtainable
    {
        private static readonly Dictionary<string, string> _languageNamePreLookupMappings = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "aramaic", "Official Aramaic" }
        };

        private readonly LanguageLookup _lookup;
        private readonly Dictionary<string, LanguageInfo> _suggestLanguagesCache = new();

        private readonly Func<string, ParatextProjectMetadata> _getParatextProjectMetadata;
        private readonly ILogger _logger;
        private readonly string _paratextAppPath;
        private JsonSerializerOptions _jsonSerializerOptions = new()
        {
            IncludeFields = true,
            WriteIndented = false
        };

        public LexiconFromXmlFiles(ILogger<LexiconFromXmlFiles> logger, Func<string, ParatextProjectMetadata> getParatextProjectMetadata, string paratextAppPath) 
        {
            _logger = logger;
            _getParatextProjectMetadata = getParatextProjectMetadata;
            _paratextAppPath = paratextAppPath;

            if (Sldr.IsInitialized)
            {
                Sldr.Cleanup();
            }
            Sldr.Initialize();
            _lookup = new();
        }

        private LanguageInfo? SuggestLanguage(string searchString)
        {
            if (_suggestLanguagesCache.TryGetValue(searchString, out var languageInfo))
            {
                return languageInfo;
            }
            else
            {
                languageInfo = _lookup.SuggestLanguages(searchString)?.FirstOrDefault();
                if (languageInfo is not null)
                {
                    _suggestLanguagesCache.Add(searchString, languageInfo);
                }

                return languageInfo;
            }
        }

        public Lexicon_Lexicon GetLexicon(string projectId)
        {
            var lexiconModel = new Lexicon_Lexicon();
            var paratextProjectMetadata = _getParatextProjectMetadata(projectId);

            // 1.  [currentParatextProjectPath]\Lexicon.xml (if there is a Paratext project loaded)
            if (paratextProjectMetadata != null && paratextProjectMetadata.ProjectPath != null && File.Exists(Path.Combine(paratextProjectMetadata.ProjectPath, "Lexicon.xml")))
            {
                LoadIntoLexiconModel(
                    lexiconModel,
                    LoadIntoFileModel<LexiconFileModel>(Path.Combine(paratextProjectMetadata.ProjectPath, "Lexicon.xml")));
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

            if (paratextProjectMetadata != null && paratextProjectMetadata.ProjectPath != null && File.Exists(Path.Combine(paratextProjectMetadata.ProjectPath, "ProjectBiblicalTerms.xml")))
            {
                LoadIntoLexiconModel(
                    lexiconModel,
                    biblicalTermsById,
                    LoadIntoFileModel<BiblicalTermsListFileModel>(Path.Combine(paratextProjectMetadata.ProjectPath, "ProjectBiblicalTerms.xml")));
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
            if (paratextProjectMetadata != null && paratextProjectMetadata.ProjectPath != null && File.Exists(Path.Combine(paratextProjectMetadata.ProjectPath, "TermRenderings.xml")))
            {
                var projectLanguageInfo = SuggestLanguage(paratextProjectMetadata.LanguageName);

                if (projectLanguageInfo is not null)
                {
                    LoadIntoLexiconModel(
                        lexiconModel,
                        biblicalTermsById,
                        projectLanguageInfo.LanguageTag,
                        LoadIntoFileModel<TermRenderingsListFileModel>(Path.Combine(paratextProjectMetadata.ProjectPath, "TermRenderings.xml")));
                }
                else
                {
                    _logger.LogWarning($"Unable to GetLanguageNameFromCode using ParatextProjectMetadata.LanguageName: '{paratextProjectMetadata.LanguageName}' to determine Term Renderings meaning language");
                }
            }

            return lexiconModel;
        }

        protected void LoadIntoLexiconModel(Lexicon_Lexicon lexiconModel, LexiconFileModel lexiconFileModel)
        {
            var lexiconLanguageInfo = _lookup.GetLanguageFromCode(lexiconFileModel.Language);
            if (lexiconLanguageInfo is null)
            {
                _logger.LogWarning($"Unable to load Lexicon file data into Lexicon model due to GetLanguageNameFromCode returning null for language: '{lexiconFileModel.Language}'");
                return;
            }

            foreach (var item in lexiconFileModel.Entries.Item)
            {
                var lexemeModel = lexiconModel.Lexemes
                    .Where(e => e.Language == lexiconLanguageInfo.LanguageTag)
                    .Where(e => e.Type == item.Lexeme.Type)
                    .Where(e => e.Lemma == item.Lexeme.Form || e.Forms.Select(f => f.Text).Contains(item.Lexeme.Form))
                    .FirstOrDefault();

                var lexemeModelCreated = false;
                if (lexemeModel is null)
                {
                    lexemeModel = new Lexicon_Lexeme
                    {
                        Id = Guid.NewGuid(),
                        Lemma = item.Lexeme.Form,
                        Type = item.Lexeme.Type,
                        Language = lexiconLanguageInfo.LanguageTag,
                    };

                    lexemeModelCreated = true;
                }

                foreach (var sense in item.Entry.Sense)
                {
                    var senseLanguageInfo = _lookup.GetLanguageFromCode(sense.Gloss.Language);
                    if (senseLanguageInfo is null)
                    {
                        _logger.LogInformation($"Unable to load Lexicon file sense/meaning entry into Lexicon model due to GetLanguageNameFromCode returning null for language: '{sense.Gloss.Language}'");
                        continue;
                    }

                    var meaningModel = lexemeModel.Meanings
                        .Where(e => e.Language == senseLanguageInfo.LanguageTag)
                        .Where(e => e.Text == string.Empty)
                        .FirstOrDefault();

                    if (meaningModel is null)
                    {
                        meaningModel = new Lexicon_Meaning
                        {
                            Id = Guid.NewGuid(),
                            Text = string.Empty, // FIXME?
                            Language = senseLanguageInfo.LanguageTag,
                            LexemeId = lexemeModel.Id
                        };

                        lexemeModel.Meanings.Add(meaningModel);
                    }

                    var translationOriginatedFrom = JsonSerializer.Serialize(
                            OriginatedFromLexiconItem.FromLexiconItemFileModel(item.Lexeme, sense),
                            _jsonSerializerOptions
                        );

                    var translationModel = meaningModel.Translations
                        .Where(e => e.Text == sense.Gloss.Text)
                        //.Where(e => e.OriginatedFrom == translationOriginatedFrom)
                        .FirstOrDefault();

                    if (translationModel is null)
                    {
                        meaningModel.Translations.Add(new Lexicon_Translation
                        {
                            Id = Guid.NewGuid(),
                            Text = sense.Gloss.Text,
                            MeaningId = meaningModel.Id,
                            OriginatedFrom = translationOriginatedFrom
                        });
                    }
                }

                if (lexemeModelCreated && 
                    lexemeModel.Meanings.Any(e => e.Translations.Any()))
                {
                    lexiconModel.Lexemes.Add(lexemeModel);
                }
            }
        }

        protected void LoadIntoLexiconModel(Lexicon_Lexicon lexiconModel, Dictionary<string, BiblicalTermFileModel> biblicalTermsById, BiblicalTermsListFileModel biblicalTermsFileModel)
        {
            var lookup = new LanguageLookup();
            var termIdsAlreadyImported = biblicalTermsById.Keys;
            foreach (var term in biblicalTermsFileModel.Term
                .Where(e => e.Gloss != null && !termIdsAlreadyImported.Contains(e.Id)))
            {
                var termLanguageInfo = (_languageNamePreLookupMappings.ContainsKey(term.Language))
                    ? SuggestLanguage(_languageNamePreLookupMappings[term.Language])
                    : SuggestLanguage(term.Language);
                if (termLanguageInfo is null)
                {
                    _logger.LogInformation($"Unable to load Biblical Term into Lexicon model due to SuggestLanguages returning null for language: '{term.Language}'");
                    continue;
                }
				
                var lexemeModel = lexiconModel.Lexemes
                    .Where(e => e.Language == termLanguageInfo.LanguageTag)
                    .Where(e => e.Type == null)
                    .Where(e => e.Lemma == term.Id || e.Forms.Select(f => f.Text).Contains(term.Id))
                    .FirstOrDefault();

                var lexemeModelCreated = false;
                if (lexemeModel is null)
                {
                    lexemeModel = new Lexicon_Lexeme
                    {
                        Id = Guid.NewGuid(),
                        Lemma = term.Id,
                        Type = null,  // FIXME:  does this have a type?
                        Language = termLanguageInfo.LanguageTag,
                    };

                    lexemeModelCreated = true;
                }

                var meaningModel = lexemeModel.Meanings
                    .Where(e => e.Language == "en")
                    .Where(e => e.Text == string.Empty)
                    .FirstOrDefault();

                if (meaningModel is null)
                {
                    meaningModel = new Lexicon_Meaning
                    {
                        Id = Guid.NewGuid(),
                        Text = string.Empty, // FIXME?
                        Language = "en",  // Per spec:  the Gloss element is always English
                        LexemeId = lexemeModel.Id
                    };

                    lexemeModel.Meanings.Add(meaningModel);
                }

                term.Gloss.Split(';')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList()
                    .ForEach(e =>
                {
                    var translationOriginatedFrom = JsonSerializer.Serialize(
                            OriginatedFromBiblicalTerm.FromBiblicalTermFileModel(term),
                            _jsonSerializerOptions
                        );

                    var translationModel = meaningModel.Translations
                        .Where(t => t.Text == e)
                        //.Where(t => t.OriginatedFrom == translationOriginatedFrom)
                        .FirstOrDefault();

                    if (translationModel is null)
                    {
                        meaningModel.Translations.Add(new Lexicon_Translation
                        {
                            Id = Guid.NewGuid(),
                            Text = e,
                            MeaningId = meaningModel.Id,
                            OriginatedFrom = translationOriginatedFrom
                        });
                    }
                });

                if (lexemeModelCreated && 
                    lexemeModel.Meanings.Any(e => e.Translations.Any()))
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
            var lookup = new LanguageLookup();
            foreach (var termRendering in termRenderingsFileModel.TermRendering
                .Where(e => e.Renderings != null && e.Guess == false && biblicalTermsById.ContainsKey(e.Id)))
            {
                var biblicalTerm = biblicalTermsById[termRendering.Id];

                var termLanguageInfo = (_languageNamePreLookupMappings.ContainsKey(biblicalTerm.Language))
                    ? SuggestLanguage(_languageNamePreLookupMappings[biblicalTerm.Language])
                    : SuggestLanguage(biblicalTerm.Language);
                if (termLanguageInfo is null)
                {
                    _logger.LogInformation($"Unable to load Term Rendering into Lexicon model due to SuggestLanguages returning null for language: '{biblicalTerm.Language}'");
                    continue;
                }
				
                var lexemeModel = lexiconModel.Lexemes
                    .Where(e => e.Language == termLanguageInfo.LanguageTag)
                    .Where(e => e.Type == null)
                    .Where(e => e.Lemma == termRendering.Id || e.Forms.Select(f => f.Text).Contains(termRendering.Id))
                    .FirstOrDefault();

                var lexemeModelCreated = false;
                if (lexemeModel is null)
                {
                    lexemeModel = new Lexicon_Lexeme
                    {
                        Id = Guid.NewGuid(),
                        Lemma = termRendering.Id,
                        Type = null,   // FIXME:  does this have a type?
                        Language = termLanguageInfo.LanguageTag,
                    };

                    lexemeModelCreated = true;
                }

                var meaningModel = lexemeModel.Meanings
                    .Where(e => e.Language == meaningLanguage)
                    .Where(e => e.Text == string.Empty)
                    .FirstOrDefault();

                if (meaningModel is null)
                {
                    meaningModel = new Lexicon_Meaning
                    {
                        Id = Guid.NewGuid(),
                        Text = string.Empty, // FIXME?
                        Language = meaningLanguage,  // Per spec:  term renderings are in the project language
                        LexemeId = lexemeModel.Id
                    };

                    lexemeModel.Meanings.Add(meaningModel);
                }

                termRendering.Renderings.Split(new string[] { "||" }, StringSplitOptions.None)
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList()
                    .ForEach(e =>
                {
                    var translationOriginatedFrom = JsonSerializer.Serialize(
                            OriginatedFromTermRendering.FromTermRenderingFileModel(termRendering),
                            _jsonSerializerOptions
                        );

                    var translationModel = meaningModel.Translations
                        .Where(t => t.Text == e)
                        //.Where(t => t.OriginatedFrom == translationOriginatedFrom)
                        .FirstOrDefault();

                    if (translationModel is null)
                    {
                        meaningModel.Translations.Add(new Lexicon_Translation
                        {
                            Id = Guid.NewGuid(),
                            Text = e,
                            MeaningId = meaningModel.Id,
                            OriginatedFrom = translationOriginatedFrom
                        });
                    }
                });

                if (lexemeModelCreated && 
                    lexemeModel.Meanings.Any(e => e.Translations.Any()))
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
