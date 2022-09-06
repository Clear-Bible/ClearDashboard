using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.WebApiParatextPlugin.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;


namespace ClearDashboard.WebApiParatextPlugin.Features.BiblicalTerms
{
    public class GetBiblicalTermsByTypeQueryHandler : IRequestHandler<GetBiblicalTermsByTypeQuery, RequestResult<List<BiblicalTermsData>>>
    {
        private readonly IPluginHost _host;
        private readonly IProject _project;
        private readonly ILogger<GetBiblicalTermsByTypeQueryHandler> _logger;

        public GetBiblicalTermsByTypeQueryHandler(IPluginHost host, IProject project, ILogger<GetBiblicalTermsByTypeQueryHandler> logger)
        {
            _host = host;
            _project = project;
            _logger = logger;
        }
        public Task<RequestResult<List<BiblicalTermsData>>> Handle(GetBiblicalTermsByTypeQuery request, CancellationToken cancellationToken)
        {
            // in 3
            var biblicalTermList = request.BiblicalTermsType == BiblicalTermsType.All
                ? _host.GetBiblicalTermList(BiblicalTermListType.All)
                : _project.BiblicalTermList;

            var queryResult = new RequestResult<List<BiblicalTermsData>>(new List<BiblicalTermsData>());
            try
            {
                queryResult.Data = ProcessBiblicalTerms(_project, biblicalTermList, cancellationToken);
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }

            return Task.FromResult(queryResult);
        }

        public List<BiblicalTermsData> ProcessBiblicalTerms(IProject project, IBiblicalTermList biblicalTermList, CancellationToken cancellationToken)
        {
            var biblicalTerms = new List<BiblicalTermsData>();
            foreach (var biblicalTerm in biblicalTermList)
            {
                var biblicalTermsData = new BiblicalTermsData();
                var properties = biblicalTerm.GetType().GetProperties();

                foreach (var pi in properties)
                {
                    var term = pi.GetValue(biblicalTerm, null);

                    var property = term.GetType().GetProperties();

                    foreach (var termProperty in property)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        switch (termProperty.Name)
                        {
                            case "Id":
                                biblicalTermsData.Id = termProperty.GetValue(term, null)?.ToString();
                                break;
                            case "Lemma":
                                biblicalTermsData.Lemma = termProperty.GetValue(term, null)?.ToString();
                                break;
                            case "Transliteration":
                                biblicalTermsData.Transliteration = termProperty.GetValue(term, null)?.ToString();
                                break;
                            case "SemanticDomain":
                                if (termProperty.GetValue(term, null) != null)
                                {
                                    biblicalTermsData.SemanticDomain = termProperty.GetValue(term, null)?.ToString();
                                }
                                break;
                            case "CategoryIds":
                                foreach (var t in (List<string>)termProperty.GetValue(term, null))
                                {
                                    //Debug.WriteLine("CategoryIds:" + t);
                                }
                                break;
                            case "LocalGloss":
                                try
                                {
                                    biblicalTermsData.LocalGloss = termProperty.GetValue(term, null)?.ToString();
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Unexpected error occurred while getting LocalGloss");
                                }
                                break;
                            case "Gloss":
                                biblicalTermsData.Gloss = termProperty.GetValue(term, null)?.ToString();
                                break;
                            case "LinkString":
                                if (termProperty.GetValue(term, null) != null)
                                {
                                    biblicalTermsData.LinkString = termProperty.GetValue(term, null)?.ToString();
                                }
                                break;
                        }

                    }
                }

                // get the renderings
                var renderings = project.GetBiblicalTermRenderings(biblicalTerm, false);
                var renderingArray = renderings.Renderings.ToArray();

                foreach (var biblicalTermRendering in renderingArray)
                {
                    biblicalTermsData.Renderings.Add(biblicalTermRendering);
                    cancellationToken.ThrowIfCancellationRequested();
                }


                var verseRefs = new List<string>();
                var longVerseRefs = new List<string>();
                var verseRefTexts = new List<string>();
                foreach (var occurrence in biblicalTerm.Occurrences)
                {
                    verseRefs.Add(occurrence.BBBCCCVVV.ToString());
                    longVerseRefs.Add($"{occurrence.BookCode} {occurrence.ChapterNum}:{occurrence.VerseNum}");

                    verseRefTexts.Add(LookupVerseText(project, occurrence.BookNum, occurrence.ChapterNum, occurrence.VerseNum, cancellationToken));
                    cancellationToken.ThrowIfCancellationRequested();
                }

                biblicalTermsData.References = verseRefs;
                biblicalTermsData.ReferencesLong = longVerseRefs;
                biblicalTermsData.ReferencesListText = verseRefTexts;


                // check the renderings to see if they are completed
                var counts = new List<BiblicalTermsCount>();
                foreach (var verseRef in verseRefs)
                {
                    counts.Add(new BiblicalTermsCount
                    {
                        VerseID = verseRef,
                        Found = false
                    });
                    cancellationToken.ThrowIfCancellationRequested();
                }
                // loop through each text testing to see if any rendering matches
                for (var i = 0; i < verseRefTexts.Count; i++)
                {
                    foreach (var render in renderingArray)
                    {
                        if (verseRefTexts[i].IndexOf(render, StringComparison.Ordinal) > -1)
                        {
                            counts[i].Found = true;
                        }
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
                var count = counts.Where(c => c.Found);
                biblicalTermsData.RenderingCount = count.Count();

                biblicalTerms.Add(biblicalTermsData);
            }

            return biblicalTerms;
        }

        private string LookupVerseText(IProject mProject, int bookNum, int chapterNum, int verseNum, CancellationToken cancellationToken)
        {
            var tokens = mProject.GetUSFMTokens(bookNum, chapterNum);
            if (tokens is null)
            {
                return "";
            }

            var lines = new List<string>();
            foreach (var token in tokens)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (token is IUSFMMarkerToken marker)
                {
                    var i = 0;
                    if (marker.Type == MarkerType.Verse)
                    {
                        // skip if the verses are beyond what we are looking for
                        var result = int.TryParse(marker.Data, out i);

                        if (result)
                        {
                            if (Convert.ToInt16(marker.Data) > verseNum)
                            {
                                break;
                            }
                            lines.Add($"Verse [{marker.Data}]");
                        }
                        else
                        {
                            // verse span so bust up the verse span
                            var parts = marker.Data.Split('-');
                            if (parts.Length > 1)
                            {
                                if (int.TryParse(parts[0], out i))
                                {
                                    if (int.TryParse(parts[1], out i))
                                    {
                                        int start = Convert.ToInt16(parts[0]);
                                        int end = Convert.ToInt16(parts[1]);
                                        for (var j = start; j < end + 1; j++)
                                        {
                                            lines.Add($"Verse [{j}]");
                                        }
                                    }
                                }
                            }
                        }


                    }
                }
                else if (token is IUSFMTextToken textToken)
                {
                    if (token.IsScripture)
                    {
                        lines.Add("Text Token: " + textToken.Text);
                    }
                }
            }

            var text = "";
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains($"Verse [{verseNum}]"))
                {
                    // get the next lines until the next verse
                    for (var j = i; j < lines.Count; j++)
                    {
                        if (lines[j].StartsWith("Text Token: "))
                        {
                            text += lines[j].Substring(12);
                        }
                    }
                    break;
                }
            }

            return text;
        }
    }
}
