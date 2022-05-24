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
using ClearDashboard.DataAccessLayer.Paratext;

namespace ClearDashboard.Wpf.ViewModels
{
    public class PinsViewModel : ToolViewModel
    {

        #region Member Variables

        private TermRenderingsList _termRenderingsList = new ();
        private BiblicalTermsList _biblicalTermsList = new ();
        private BiblicalTermsList _allBiblicalTermsList = new ();

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

                if (!File.Exists(xmlPath))
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



            base.OnViewReady(view);
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        #endregion //Constructor

        #region Methods


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


}
