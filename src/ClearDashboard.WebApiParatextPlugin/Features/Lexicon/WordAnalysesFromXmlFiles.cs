using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using WordAnalysesFileModel = ClearDashboard.DataAccessLayer.Models.WordAnalyses;
using Microsoft.Extensions.Logging;
using System.Media;
using System.Reflection;
using SIL.WritingSystems;

namespace ClearDashboard.WebApiParatextPlugin.Features.Lexicon
{
    public class WordAnalysesFromXmlFiles : IWordAnalysesObtainable
    {
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

        public WordAnalysesFromXmlFiles(ILogger<WordAnalysesFromXmlFiles> logger, Func<string, ParatextProjectMetadata> getParatextProjectMetadata, string paratextAppPath) 
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

        public IEnumerable<Lexicon_WordAnalysis> GetWordAnalyses(string projectId)
        {
            var wordAnalysesModel = new List<Lexicon_WordAnalysis>();
            var paratextProjectMetadata = _getParatextProjectMetadata(projectId);

            // 1.  [currentParatextProjectPath]\WordAnalyses.xml (if there is a Paratext project loaded)
            if (paratextProjectMetadata != null && paratextProjectMetadata.ProjectPath != null && File.Exists(Path.Combine(paratextProjectMetadata.ProjectPath, "WordAnalyses.xml")))
            {
				var projectLanguageInfo = SuggestLanguage(paratextProjectMetadata.LanguageName);

				LoadIntoWordAnalysesModel(
					wordAnalysesModel,
					projectLanguageInfo.LanguageTag,
                    LoadIntoFileModel<WordAnalysesFileModel>(Path.Combine(paratextProjectMetadata.ProjectPath, "WordAnalyses.xml")));
            }

            return wordAnalysesModel;
        }

        protected void LoadIntoWordAnalysesModel(List<Lexicon_WordAnalysis> wordAnalysesModel, string projectLanguage, WordAnalysesFileModel wordAnalysesFileModel)
        {
            var existingLexemes = wordAnalysesModel
                .SelectMany(e => e.Lexemes.Where(l => l.Language == projectLanguage))
                .ToDictionary(e => (e.Type.ToLower(), e.Lemma.ToLower()), e => e);

            foreach (var entry in wordAnalysesFileModel.Entries)
            {
                var wordAnalysisModel = new Lexicon_WordAnalysis
                {
                    Id = Guid.NewGuid(),
                    Word = entry.Word,
                    Language = projectLanguage
                };

                if (!entry.Analyses.Any())
                {
                    wordAnalysesModel.Add(wordAnalysisModel);
                    continue;
                }

                if (entry.Analyses.Count() > 1)
                {
                    throw new Exception($"Model problem!  One or more WordAnalyses.xml entries has multiple 'analysis' elements {entry.Word}");
                }    

                foreach (var lexeme in entry.Analyses.First().Lexemes)
                {
                    var parts = lexeme.Split(':');
                    if (parts.Length != 2)
                    {
                        throw new Exception($"Word {entry.Word} has lexeme {lexeme} that does not have format xx:yy");
                    }

                    var type = parts[0];
                    var lemma = parts[1];

					if (!existingLexemes.TryGetValue((type.ToLower(), lemma.ToLower()), out var lexemeModel))
                    {
						lexemeModel = new Lexicon_Lexeme
						{
							Id = Guid.NewGuid(),
							Type = parts[0],
							Lemma = parts[1],
							Language = projectLanguage
						};

                        existingLexemes.Add((lexemeModel.Type.ToLower(), lexemeModel.Lemma.ToLower()), lexemeModel);
					}


					wordAnalysisModel.Lexemes.Add(lexemeModel);
				}

				wordAnalysesModel.Add(wordAnalysisModel);
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
