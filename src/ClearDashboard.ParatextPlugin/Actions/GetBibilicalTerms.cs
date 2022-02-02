using ClearDashboard.NamedPipes.Models;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

    public class GetBibilicalTerms
    {
        public IBiblicalTermList BiblicalTermList { get; set; }

        public GetBibilicalTerms(ListType listType, IProject project, IWindowPluginHost host)
        {
            IBiblicalTermList biblicalTermList;

            if (listType.isProject)
            {
                BiblicalTermList = project.BiblicalTermList;
            }
            else
            {
                BiblicalTermList = host.GetBiblicalTermList(listType.type);
            }
        }

        internal List<BiblicalTermsData> ProcessBiblicalTerms(IProject m_project)
        {
            var biblicalTermList = this.BiblicalTermList.ToList();

            List<BiblicalTermsData> btList = new List<BiblicalTermsData>();


            foreach (var biblicalTerm in biblicalTermList)
            {
                BiblicalTermsData bterm = new BiblicalTermsData();
                PropertyInfo[] properties = biblicalTerm.GetType().GetProperties();
                StringBuilder sb = new StringBuilder();

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
                                //foreach (var t in (List<string>)termProperty.GetValue(term, null))
                                //{
                                //    Debug.WriteLine("CategoryIds:" + t);
                                //}
                                break;
                            case "LocalGloss":
                                bterm.LocalGloss = termProperty.GetValue(term, null).ToString();
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
                foreach (var occurrence in biblicalTerm.Occurrences)
                {
                    VerseRef.Add(occurrence.BBBCCCVVV.ToString());
                }

                bterm.References = VerseRef;
                btList.Add(bterm);
            }

            return btList;
        }
    }
}
