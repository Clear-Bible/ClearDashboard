using System;
using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Paratext;
using SIL.ObjectModel;

namespace ClearDashboard.Wpf.ViewModels
{
    public class PinsViewModel : ToolViewModel
    {

        #region Member Variables

        private TermRenderingsList _termRenderingsList = new ();
        private BiblicalTermsList _biblicalTermsList = new ();
        private BiblicalTermsList _allBiblicalTermsList = new ();
        private SpellingStatus _spellingStatus = new ();
        private ObservableList<PinsDataTable> _thedata = new ();

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor

        public PinsViewModel()
        {
        }

        public PinsViewModel(INavigationService navigationService, ILogger<PinsViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator): base(navigationService,logger, projectManager, eventAggregator)
        {
            this.Title = "⍒ PINS";
            this.ContentId = "PINS";
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("");
            return base.OnActivateAsync(cancellationToken);
        }

        protected override async void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }


        protected override async void OnViewReady(object view)
        {
            // load in the TermRenderings.xml file
            await Task.Run(() =>
            {
                string xmlPath = Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath,
                    "TermRenderings.xml");

                if (File.Exists(xmlPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlPath);
                    XmlNodeReader reader = new XmlNodeReader(doc);

                    using (reader)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TermRenderingsList));
                        try
                        {
                            _termRenderingsList = (TermRenderingsList)serializer.Deserialize(reader);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Error in PINS deserialization of TermRenderings.xml: " + e.Message);
                        }
                    }
                }
                else
                {
                    Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                }
            }).ConfigureAwait(false);



            ParatextProxy paratextUtils = new ParatextProxy(Logger as ILogger<ParatextProxy>);
            string paratextInstallPath = "";
            if (paratextUtils.IsParatextInstalled())
            {
                paratextInstallPath = paratextUtils.ParatextInstallPath;

                // load in the BiblicalTerms.xml file
                await Task.Run(() =>
                {
                    string xmlPath = Path.Combine(paratextInstallPath, @"Terms\Lists\BiblicalTerms.xml");

                    if (File.Exists(xmlPath))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(xmlPath);
                        XmlNodeReader reader = new XmlNodeReader(doc);

                        using (reader)
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(BiblicalTermsList));
                            try
                            {
                                _biblicalTermsList = (BiblicalTermsList)serializer.Deserialize(reader);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Error in PINS deserialization of BibilicalTerms.xml: " + e.Message);
                            }

                        }
                    }
                    else
                    {
                        Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                    }
                }).ConfigureAwait(false);


                // load in the AllBiblicalTerms.xml file
                await Task.Run(() =>
                {
                    string xmlPath = Path.Combine(paratextInstallPath, @"Terms\Lists\AllBiblicalTerms.xml");

                    if (File.Exists(xmlPath))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(xmlPath);
                        XmlNodeReader reader = new XmlNodeReader(doc);

                        using (reader)
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(BiblicalTermsList));
                            try
                            {
                                _allBiblicalTermsList = (BiblicalTermsList)serializer.Deserialize(reader);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Error in PINS deserialization of AllBibilicalTerms.xml: " + e.Message);
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                Logger.LogError("Paratext Not Installed in PINS viewmodel");
            }


            // fix the greek renderings which are inconsistent
            for (int i = _termRenderingsList.TermRendering.Count - 1; i >= 0; i--)
            {
                if (_termRenderingsList.TermRendering[i].Renderings == "")
                {
                    // remove any records without rendering data
                    _termRenderingsList.TermRendering.RemoveAt(i);
                }
                else
                {
                    _termRenderingsList.TermRendering[i].Id =
                        CorrectUnicode(_termRenderingsList.TermRendering[i].Id);
                }
            }

            for (int i = _biblicalTermsList.Term.Count - 1; i >= 0; i--)
            {
                if (_biblicalTermsList.Term[i].Id == "")
                {
                    _biblicalTermsList.Term[i].Id =
                        CorrectUnicode(_biblicalTermsList.Term[i].Id);
                }
            }

            for (int i = _allBiblicalTermsList.Term.Count - 1; i >= 0; i--)
            {
                if (_allBiblicalTermsList.Term[i].Id == "")
                {
                    _allBiblicalTermsList.Term[i].Id =
                        CorrectUnicode(_allBiblicalTermsList.Term[i].Id);
                }
            }


            // load in the spellingstatus.xml
            await Task.Run(() =>
            {
                string xmlPath = Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath,
                    "SpellingStatus.xml");

                if (File.Exists(xmlPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlPath);
                    XmlNodeReader reader = new XmlNodeReader(doc);

                    using (reader)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(SpellingStatus));
                        try
                        {
                            _spellingStatus = (SpellingStatus)serializer.Deserialize(reader);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Error in PINS deserialization of AllBibilicalTerms.xml: " + e.Message);
                        }
                    }
                }
                else
                {
                    Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                }
            }).ConfigureAwait(false);

            Console.WriteLine();

            foreach (var terms in _termRenderingsList.TermRendering)
            {
                string target = terms.Renderings;
                target = target.Replace("||", "; ");

                if (target == "Ijip")
                {
                    Console.WriteLine();
                }


                string source = terms.Id;

                string pbtsense;
                string pbtspell;
                if (source.Contains("."))
                {                                                                                                               // Sense number uses "." in gateway language; this Sense number will not match anything in abt or bbt
                    pbtsense = source[..source.IndexOf(".")] + " {" + source[(source.IndexOf(".") + 1)..] + "}";    // place Sense in braces  
                    pbtspell = source = source[..source.IndexOf(".")];                                                 // remove the Sense number from word/phrase for correct matching with AllBiblicalTerms
                }
                else if (source.Contains("-"))                                                                               // Sense number uses "-" in Gk & Heb, this will match bbt
                {
                    pbtsense = source.Trim().Replace("-", " {") + "}";                                                       // place Sense in braces
                    pbtspell = source[..source.IndexOf("-")];                                                             // remove the Sense number from word/phrase for correct matching with Spelling
                }
                else
                    pbtspell = pbtsense = source;


                // CHECK AGAINST SPELLING
                var spellingRecords = _spellingStatus.Status.FindAll(s => s.Word.ToLower() == pbtspell.ToLower());
                if (spellingRecords.Count == 0)
                {
                    pbtspell = "";
                }
                else
                {
                    if (spellingRecords[0].State == "W")
                    {
                        // misspelled
                        pbtspell = " [misspelled]";
                    } 
                    else if (spellingRecords[0].SpecificCase != pbtspell)
                    {
                        //   has wrong case
                        pbtspell = " [" + spellingRecords[0].SpecificCase + "] wrong case";
                    }
                    else
                    {
                        pbtspell = "";
                    }

                }

                var notes = terms.Notes;
                var denials = terms.Denials;
                var gloss = "";
                List<string> verselist = new List<string>();

                // check against the BiblicalTermsList
                var bt = _biblicalTermsList.Term.FindAll(t => t.Id == source);
                if (bt.Count > 0)
                {
                    gloss = bt[0].Gloss;

                    foreach (var verse in bt[0].References.Verse)
                    {
                        verselist.Add(verse);
                    }

                    _thedata.Add(new PinsDataTable
                    {
                        Code = "KeyTerm",
                        Gloss = gloss,
                        Lang = "",
                        Lform = "",
                        Match = "KeyTerm" + source,
                        Notes = "",
                        Phrase = "",
                        Prefix = "",
                        Refs = "",
                        SimpRefs = "",
                        Source = target + pbtspell,
                        Stem = "",
                        Suffix = "",
                        Word = "",
                    });
                }
                else
                {
                    // now check AllBiblicalTerms
                    var abt = _allBiblicalTermsList.Term.FindAll(t => t.Id == source);
                    if (abt.Count > 0)
                    {
                        Console.WriteLine();
                    }
                    else
                    {
                        _thedata.Add(new PinsDataTable
                        {
                            Code = "KeyTerm",
                            Gloss = gloss,
                            Lang = "",
                            Lform = "",
                            Match = "KeyTerm" + source,
                            Notes = "",
                            Phrase = "",
                            Prefix = "",
                            Refs = "",
                            SimpRefs = "",
                            Source = target + pbtspell,
                            Stem = "",
                            Suffix = "",
                            Word = "",
                        });
                    }
                }

                Console.WriteLine();
            }


            base.OnViewReady(view);
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        #endregion //Constructor

        #region Methods

        private string CorrectUnicode(string instr)
        {                                       
            // There is a problem in Gk Unicode Vowels exhibiting in Paratext. See https://wiki.digitalclassicist.org/Greek_Unicode_duplicated_vowels
            // The basic code is preferred by the Unicode Consortium
            // AllBiblicalTerms.xml and Biblical Terms.xml tend to use the Extended codes while TermRenderings.xml tends to use the Basic codes
            //  Unicode Basic   Extend
            instr = instr.Replace('ά', 'ά')     //  ά 	    03AC 	1F71
                .Replace('έ', 'έ')     //  έ 	    03AD 	1F73
                .Replace('ή', 'ή')     //  ή 	    03AE 	1F75
                .Replace('ί', 'ί')     //  ί 	    03AF 	1F77
                .Replace('ό', 'ό')     //  ό 	    03CC 	1F79
                .Replace('ύ', 'ύ')     //  ύ 	    03CD 	1F7B
                .Replace('ώ', 'ώ')     //  ώ 	    03CE 	1F7D
                .Replace('Ά', 'Ά')     //  Ά 	    0386 	1FBB
                .Replace('Έ', 'Έ')     //  Έ 	    0388 	1FC9
                .Replace('Ή', 'Ή')     //  Ή 	    0389 	1FCB
                .Replace('Ί', 'Ί')     //  Ί 	    038A 	1FDB
                .Replace('Ό', 'Ό')     //  Ό 	    038C 	1FF9
                .Replace('Ύ', 'Ύ')     //  Ύ 	    038E 	1FEB
                .Replace('Ώ', 'Ώ')     //  Ώ 	    038F 	1FFB
                .Replace('ΐ', 'ΐ')     //  ΐ 	    0390 	1FD3
                .Replace('ΰ', 'ΰ');    //  ΰ 	    03B0 	1FE3
            return instr;
        }

        #endregion // Methods
    }





    [XmlRoot(ElementName = "TermRendering")]
    public class TermRendering
    {

        [XmlElement(ElementName = "Changes")]
        public Changes Changes { get; set; }

        [XmlElement(ElementName = "Notes")]
        public object Notes { get; set; }

        [XmlElement(ElementName = "Denials")]
        public Denials Denials { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "Guess")]
        public bool Guess { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Renderings")]
        public string Renderings { get; set; }

        [XmlElement(ElementName = "Glossary")]
        public object Glossary { get; set; }
    }

    [XmlRoot(ElementName = "Change")]
    public class Change
    {

        [XmlElement(ElementName = "Before")]
        public object Before { get; set; }

        [XmlElement(ElementName = "After")]
        public string After { get; set; }

        [XmlAttribute(AttributeName = "UserName")]
        public string UserName { get; set; }

        [XmlAttribute(AttributeName = "TermId")]
        public string TermId { get; set; }

        [XmlAttribute(AttributeName = "Date")]
        public DateTime Date { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Changes")]
    public class Changes
    {

        [XmlElement(ElementName = "Change")]
        public List<Change> Change { get; set; }
    }

    [XmlRoot(ElementName = "Denials")]
    public class Denials
    {

        [XmlElement(ElementName = "Denial")]
        public List<int> Denial { get; set; }
    }

    [XmlRoot(ElementName = "TermRenderingsList")]
    public class TermRenderingsList
    {

        [XmlElement(ElementName = "TermRendering")]
        public List<TermRendering> TermRendering { get; set; }
    }








    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(BiblicalTermsList));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (BiblicalTermsList)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "References")]
    public class References
    {

        [XmlElement(ElementName = "Verse")]
        public List<string> Verse { get; set; }
    }

    [XmlRoot(ElementName = "Term")]
    public class Term
    {
        [XmlElement(ElementName = "Strong")]
        public string Strong { get; set; }

        [XmlElement(ElementName = "Transliteration")]
        public string Transliteration { get; set; }

        [XmlElement(ElementName = "Category")]
        public string Category { get; set; }

        [XmlElement(ElementName = "Domain")]
        public string Domain { get; set; }

        [XmlElement(ElementName = "Language")]
        public string Language { get; set; }

        [XmlElement(ElementName = "Definition")]
        public string Definition { get; set; }

        [XmlElement(ElementName = "Gloss")]
        public string Gloss { get; set; }

        [XmlElement(ElementName = "References")]
        public References References { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Link")]
        public string Link { get; set; }
    }

    [XmlRoot(ElementName = "BiblicalTermsList")]
    public class BiblicalTermsList
    {

        [XmlElement(ElementName = "Term")]
        public List<Term> Term { get; set; }

        [XmlAttribute(AttributeName = "xsi")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd")]
        public string Xsd { get; set; }

        [XmlText]
        public string Text { get; set; }
    }




    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(SpellingStatus));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (SpellingStatus)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "Status")]
    public class Status
    {

        [XmlAttribute(AttributeName = "Word")]
        public string Word { get; set; }

        [XmlAttribute(AttributeName = "State")]
        public string State { get; set; }

        [XmlElement(ElementName = "SpecificCase")]
        public string SpecificCase { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Correction")]
        public string Correction { get; set; }
    }

    [XmlRoot(ElementName = "SpellingStatus")]
    public class SpellingStatus
    {

        [XmlElement(ElementName = "Status")]
        public List<Status> Status { get; set; }
    }



}
