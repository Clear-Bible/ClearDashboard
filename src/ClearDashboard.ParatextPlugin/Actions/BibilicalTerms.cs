using System;
using ClearDashboard.Pipes_Shared.Models;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public List<BiblicalTermsData> ProcessBiblicalTerms(IProject m_project)
        {
            var biblicalTermList = this.BiblicalTermList.ToList();

            List<BiblicalTermsData> btList = new List<BiblicalTermsData>();
            int recordNum = 0;

            foreach (var biblicalTerm in biblicalTermList)
            {
                BiblicalTermsData bterm = new BiblicalTermsData();
                PropertyInfo[] properties = biblicalTerm.GetType().GetProperties();

                recordNum++;
                foreach (PropertyInfo pi in properties)
                {
                    var term = pi.GetValue(biblicalTerm, null);

                    PropertyInfo[] property = term.GetType().GetProperties();

                    foreach (PropertyInfo termProperty in property)
                    {
                        switch (termProperty.Name)
                        {
                            case "Id":
                                bterm.Id = termProperty.GetValue(term, null).ToString();
                                //Debug.WriteLine(recordNum + " " +bterm.Id);
                                //if (recordNum == 14)
                                //{
                                //    Console.WriteLine();
                                //}

                                break;
                            case "Lemma":
                                bterm.Lemma = termProperty.GetValue(term, null).ToString();
                                break;
                            case "Transliteration":
                                bterm.Transliteration = termProperty.GetValue(term, null).ToString();
                                break;
                            case "SemanticDomain":
                                if (termProperty.GetValue(term, null) != null)
                                {
                                    bterm.SemanticDomain = termProperty.GetValue(term, null).ToString();
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
                                    bterm.LocalGloss = termProperty.GetValue(term, null).ToString();
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                }
                                break;
                            case "Gloss":
                                bterm.Gloss = termProperty.GetValue(term, null).ToString();
                                break;
                            case "LinkString":
                                if (termProperty.GetValue(term, null) != null)
                                {
                                    bterm.LinkString = termProperty.GetValue(term, null).ToString();
                                }
                                break;
                        }

                    }
                }

                // get the renderings
                IBiblicalTerm termRendering = (IBiblicalTerm)biblicalTerm;
                var renderings = m_project.GetBiblicalTermRenderings(termRendering, false);
                var renderingArray = renderings.Renderings.ToArray();

                foreach (var biblicalTermRendering in renderingArray)
                {
                    bterm.Renderings.Add(biblicalTermRendering);
                }


                List<string> VerseRef = new List<string>();
                List<string> VerseRefLong = new List<string>();
                List<string> VerseRefText = new List<string>();
                foreach (var occurrence in biblicalTerm.Occurrences)
                {
                    VerseRef.Add(occurrence.BBBCCCVVV.ToString());
                    VerseRefLong.Add($"{occurrence.BookCode} {occurrence.ChapterNum}:{occurrence.VerseNum}");

                    VerseRefText.Add(LookupVerseText(m_project, occurrence.BookNum, occurrence.ChapterNum, occurrence.VerseNum));
                }

                bterm.References = VerseRef;
                bterm.ReferencesLong = VerseRefLong;
                bterm.ReferencesListText = VerseRefText;


                // check the renderings to see if they are completed
                List<BiblicalTermsCount> Counts = new List<BiblicalTermsCount> {};
                foreach (var verseRef in VerseRef)
                {
                    Counts.Add(new BiblicalTermsCount
                    {
                        VerseID = verseRef,
                        Found = false
                    });
                }
                // loop through each text testing to see if any rendering matches
                for (int i = 0; i < VerseRefText.Count; i++)
                {
                    foreach (var render in renderingArray)
                    {
                        if (VerseRefText[i].IndexOf(render) > -1)
                        {
                            Counts[i].Found = true;
                        }
                    }
                }
                var count = Counts.Where(c => c.Found == true);
                bterm.RenderingCount = count.Count();

                btList.Add(bterm);
            }

            return btList;
        }

        private string LookupVerseText(IProject mProject, int BookNum, int ChapterNum, int VerseNum)
        {
            _lastBookCode = "";
            _lastChapterNum = 0;
            _lastScript = new List<string>();

            IEnumerable<IUSFMToken> tokens = mProject.GetUSFMTokens(BookNum, ChapterNum);
            if (tokens is null)
            {
                return "";
            }

            List<string> lines = new List<string>();
            foreach (var token in tokens)
            {
                if (token is IUSFMMarkerToken marker)
                {
                    if (marker.Type == MarkerType.Verse)
                    {
                        Debug.WriteLine($"Verse: {marker.Data}");
                        // skip if the verses are beyond what we are looking for
                        int i = 0;
                        bool result = int.TryParse(marker.Data, out i);

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
                            string[] nums = marker.Data.Split('-');
                            if (nums.Length > 1)
                            {
                                if (int.TryParse(nums[0], out i))
                                {
                                    if (int.TryParse(nums[1], out i))
                                    {
                                        int start = Convert.ToInt16(nums[0]);
                                        int end = Convert.ToInt16(nums[1]);
                                        for (int j = start; j < end + 1; j++)
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
                    lines.Add("Text Token: " + textToken.Text);
                }
            }

            string sText = "";

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains($"Verse [{VerseNum}]"))
                {
                    // get the next lines until the next verse
                    for (int j = i; j < lines.Count; j++)
                    {
                        if (lines[j].StartsWith("Text Token: "))
                        {
                            sText += lines[j].Substring(12);
                        }
                    }
                    break;
                }
            }

            return sText;
        }
    }
}
