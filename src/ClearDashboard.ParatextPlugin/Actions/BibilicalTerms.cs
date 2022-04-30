using System;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ClearDashboard.ParatextPlugin.Data.Models;
using ClearDashboard.ParatextPlugin.Models;

namespace ClearDashboard.ParatextPlugin.Actions
{
    public class ListType
    {
        public string label;
        public bool isProject;
        public BiblicalTermListType type;

        public ListType(string label, bool isProject, BiblicalTermListType type)
        {
            this.label = label;
            this.isProject = isProject;
            this.type = type;
        }

        public override string ToString() => label;
    }

    public class BibilicalTerms
    {
        private string _lastBookCode = "";
        private int _lastChapterNum;
        private List<string> _lastScript = new List<string>();

        public IBiblicalTermList BiblicalTermList { get; set; }

        public BibilicalTerms(ListType listType, IProject project, IWindowPluginHost host)
        {
            if (listType.isProject)
            {
                BiblicalTermList = project.BiblicalTermList;
            }
            else
            {
                BiblicalTermList = host.GetBiblicalTermList(listType.type);
            }
        }

        public List<BiblicalTermsData> ProcessBiblicalTerms(IProject project)
        {
            var biblicalTermList = this.BiblicalTermList.ToList();

            var btList = new List<BiblicalTermsData>();
            var recordNum = 0;

            foreach (var biblicalTerm in biblicalTermList)
            {
                var biblicalTermsData = new BiblicalTermsData();
                var properties = biblicalTerm.GetType().GetProperties();

                recordNum++;
                foreach (var pi in properties)
                {
                    var term = pi.GetValue(biblicalTerm, null);

                    var property = term.GetType().GetProperties();

                    foreach (var termProperty in property)
                    {
                        switch (termProperty.Name)
                        {
                            case "Id":
                                biblicalTermsData.Id = termProperty.GetValue(term, null).ToString();
                                break;
                            case "Lemma":
                                biblicalTermsData.Lemma = termProperty.GetValue(term, null).ToString();
                                break;
                            case "Transliteration":
                                biblicalTermsData.Transliteration = termProperty.GetValue(term, null).ToString();
                                break;
                            case "SemanticDomain":
                                if (termProperty.GetValue(term, null) != null)
                                {
                                    biblicalTermsData.SemanticDomain = termProperty.GetValue(term, null).ToString();
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
                                    biblicalTermsData.LocalGloss = termProperty.GetValue(term, null).ToString();
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                }
                                break;
                            case "Gloss":
                                biblicalTermsData.Gloss = termProperty.GetValue(term, null).ToString();
                                break;
                            case "LinkString":
                                if (termProperty.GetValue(term, null) != null)
                                {
                                    biblicalTermsData.LinkString = termProperty.GetValue(term, null).ToString();
                                }
                                break;
                        }

                    }
                }

                // get the renderings
                var termRendering = (IBiblicalTerm)biblicalTerm;
                var renderings = project.GetBiblicalTermRenderings(termRendering, false);
                var renderingArray = renderings.Renderings.ToArray();

                foreach (var biblicalTermRendering in renderingArray)
                {
                    biblicalTermsData.Renderings.Add(biblicalTermRendering);
                }


                var verseRefs = new List<string>();
                var longVerseRefs = new List<string>();
                var verseRefTexts = new List<string>();
                foreach (var occurrence in biblicalTerm.Occurrences)
                {
                    verseRefs.Add(occurrence.BBBCCCVVV.ToString());
                    longVerseRefs.Add($"{occurrence.BookCode} {occurrence.ChapterNum}:{occurrence.VerseNum}");

                    verseRefTexts.Add(LookupVerseText(project, occurrence.BookNum, occurrence.ChapterNum, occurrence.VerseNum));
                }

                biblicalTermsData.References = verseRefs;
                biblicalTermsData.ReferencesLong = longVerseRefs;
                biblicalTermsData.ReferencesListText = verseRefTexts;


                // check the renderings to see if they are completed
                var Counts = new List<BiblicalTermsCount> {};
                foreach (var verseRef in verseRefs)
                {
                    Counts.Add(new BiblicalTermsCount
                    {
                        VerseID = verseRef,
                        Found = false
                    });
                }
                // loop through each text testing to see if any rendering matches
                for (var i = 0; i < verseRefTexts.Count; i++)
                {
                    foreach (var render in renderingArray)
                    {
                        if (verseRefTexts[i].IndexOf(render, StringComparison.Ordinal) > -1)
                        {
                            Counts[i].Found = true;
                        }
                    }
                }
                var count = Counts.Where(c => c.Found == true);
                biblicalTermsData.RenderingCount = count.Count();

                btList.Add(biblicalTermsData);
            }

            return btList;
        }

        private string LookupVerseText(IProject mProject, int BookNum, int ChapterNum, int VerseNum)
        {
            _lastBookCode = "";
            _lastChapterNum = 0;
            _lastScript = new List<string>();

            var tokens = mProject.GetUSFMTokens(BookNum, ChapterNum);
            if (tokens is null)
            {
                return "";
            }

            var lines = new List<string>();
            foreach (var token in tokens)
            {
                if (token is IUSFMMarkerToken marker)
                {
                    if (marker.Type == MarkerType.Verse)
                    {
                        // skip if the verses are beyond what we are looking for
                        var i = 0;
                        var result = int.TryParse(marker.Data, out i);

                        if (result)
                        {
                            if (Convert.ToInt16(marker.Data) > VerseNum)
                            {
                                break;
                            }
                            lines.Add($"Verse [{marker.Data}]");
                        }
                        else
                        {
                            // verse span so bust up the verse span
                            var nums = marker.Data.Split('-');
                            if (nums.Length > 1)
                            {
                                if (int.TryParse(nums[0], out i))
                                {
                                    if (int.TryParse(nums[1], out i))
                                    {
                                        int start = Convert.ToInt16(nums[0]);
                                        int end = Convert.ToInt16(nums[1]);
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
                if (lines[i].Contains($"Verse [{VerseNum}]"))
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
