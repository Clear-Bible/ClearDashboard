using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.TextCollections
{
    public class TextCollectionsSlice
    {
        public class GetTextCollectionsQueryHandler : IRequestHandler<GetTextCollectionsQuery, RequestResult<List<TextCollection>>>
        {
            private readonly IProject _project;
            private readonly ILogger<GetTextCollectionsQueryHandler> _logger;
            private readonly IPluginHost _host;
            private readonly IVerseRef _verseRef;

            public GetTextCollectionsQueryHandler(IProject project, ILogger<GetTextCollectionsQueryHandler> logger, IPluginHost host, IVerseRef verseRef)
            {
                _project = project;
                _logger = logger;
                _host = host;
                _verseRef = verseRef;
            }
            public Task<RequestResult<List<TextCollection>>> Handle(GetTextCollectionsQuery request, CancellationToken cancellationToken)
            {
                var textCollections = GetTextCollectionsData();
                var result = new RequestResult<List<TextCollection>>(textCollections);
                return Task.FromResult(result);
            }

            private List<TextCollection> GetTextCollectionsData()
            {
                // get the text collections
                List<TextCollection> textCollections = new();

                var windows = _host.AllOpenWindows;
                foreach (var window in windows)
                {
                    // check if window is text collection
                    if (window is ITextCollectionChildState tc)
                    {
                        // get the projects for this window
                        var projects = tc.AllProjects;
                        foreach (var proj in projects)
                        {
                            TextCollection textCollection = new();
                            if (proj != null)
                            {
                                IEnumerable<IUSFMToken> tokens = null;
                                try
                                {
                                    tokens = _project.GetUSFMTokens(_verseRef.BookNum, _verseRef.ChapterNum,
                                        _verseRef.VerseNum);
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, $"Cannot get USFM Tokens for {proj.ShortName} : {e.Message}");
                                }

                                if (tokens != null)
                                {
                                    textCollection.ReferenceShort = _project.ShortName;

                                    foreach (var token in tokens)
                                    {
                                        if (token is IUSFMMarkerToken marker)
                                        {
                                            if (marker.Type == MarkerType.Verse)
                                            {
                                                //skip
                                            }
                                            else if (marker.Type == MarkerType.Paragraph)
                                            {
                                                textCollection.Data += "/ ";
                                            }
                                            else
                                            {
                                                textCollection.Data += $"{marker.Type} Marker: {marker.Data}";
                                            }
                                        }
                                        else if (token is IUSFMTextToken textToken)
                                        {
                                            textCollection.Data += textToken.Text + " ";
                                        }
                                        else if (token is IUSFMAttributeToken)
                                        {
                                            textCollection.Data += "Attribute Token: " + token.ToString();
                                        }
                                        else
                                        {
                                            textCollection.Data += "Unexpected token type: " + token.ToString();
                                        }
                                    }

                                    // remove the last paragraph tag if at the end
                                    if (textCollection.Data.Length > 2)
                                    {
                                        if (textCollection.Data.EndsWith("/ "))
                                        {
                                            textCollection.Data = textCollection.Data.Substring(0, textCollection.Data.Length - 2);
                                        }
                                    }

                                    textCollections.Add(textCollection);
                                }
                            }
                        }
                    }
                }

                return textCollections;
            }
        }
    }
}
