﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.WebApiParatextPlugin.Models;
using Paratext.PluginInterfaces;

namespace ClearDashboard.WebApiParatextPlugin.Actions
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

                    VerseRefText.Add(Helpers.VerseText.LookupVerseText(m_project, occurrence.BookNum, occurrence.ChapterNum, occurrence.VerseNum));
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
    }
}
