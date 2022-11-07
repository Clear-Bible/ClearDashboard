using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.WebApiParatextPlugin.Extensions;
using ClearDashboard.WebApiParatextPlugin.Helpers;
using ClearDashboard.WebApiParatextPlugin.Hubs;
using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Paratext.PluginInterfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using Microsoft.Win32;
using SIL.Linq;
using SIL.Scripture;
using ProjectType = Paratext.PluginInterfaces.ProjectType;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.WebApiParatextPlugin.Models;

namespace ClearDashboard.WebApiParatextPlugin
{
    public partial class MainWindow : EmbeddedPluginControl, IPluginLogger
    {

        #region Properties

        private IProject _project;

        public IProject Project
        {
            get { return _project; }
            set { _project = value; }
        }

        private List<IProject> _projectList = new();
        private IVerseRef _verseRef;
        private IWindowPluginHost _host;
        private IPluginChildWindow _parent;
        private IMediator _mediator;
        private IHubContext HubContext => GlobalHost.ConnectionManager.GetHubContext<PluginHub>();
        private WebHostStartup WebHostStartup { get; set; }
        private IDisposable WebAppProxy { get; set; }
        private delegate void AppendMsgTextDelegate(Color color, string text);

        private List<TextCollection> _textCollections = new();
        #endregion

        #region startup

        public MainWindow()
        {
            InitializeComponent();
            ConfigureLogging();
            DisplayPluginVersion();
            Disposed += HandleWindowDisposed;

            // NB:  Use the following for debugging plug-in start up crashes.
            Application.ThreadException += ThreadExceptionEventHandler;
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionEventHandler;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;

        }


        #region Please leave these for debugging plug-in start up crashes
        private static void ThreadExceptionEventHandler(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Log.Error($"ThreadExceptionEventHandler - Exception={e.Exception}");
        }
        private static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error($"UnhandledExceptionEventHandler - Exception={e.ExceptionObject}");
        }

        private static void FirstChanceExceptionEventHandler(object sender, FirstChanceExceptionEventArgs e)
        {
            Log.Error($"FirstChanceExceptionEventHandler - Exception={e.Exception.ToString()}");
        }
        #endregion Please leave these for debugging plug-in start up crashes

        private void HandleWindowDisposed(object sender, EventArgs e)
        {
            WebAppProxy?.Dispose();
            WebAppProxy = null;
        }

        #endregion


        #region Paratext overrides - standard functions
        public override void OnAddedToParent(IPluginChildWindow parent, IWindowPluginHost host, string state)
        {
            parent.SetTitle(ClearDashboardWebApiPlugin.PluginName);
            parent.ProjectChanged += ProjectChanged;
            parent.VerseRefChanged += VerseRefChanged;

            SetProject(parent.CurrentState.Project);
            SetVerseRef(parent.CurrentState.VerseRef);

            _host = host;
            _project = parent.CurrentState.Project;
            _parent = parent;
            AppendText(Color.Green, $"OnAddedToParent called");

            UpdateProjectList();
            //ShowScripture(_project);
        }

        public override string GetState()
        {
            // override required by base class, return null string.
            return null;
        }


        public override void DoLoad(IProgressInfo progressInfo)
        {
            StartWebHost();

            // Since DoLoad is done on a different thread than what was used
            // to create the control, we need to use the Invoke method.
            //Invoke((Action)(() => GetAllProjects(true)));

            //Invoke((Action)(() => GetUsfmForBook("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff", 40)));

            //zzSur
            //Invoke((Action)(() => GetUsfmForBook("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", 40)));
        }


        private List<string> ExpectedFailedToLoadAssemblies = new List<string> { "Microsoft.Owin", "Microsoft.Extensions.DependencyInjection.Abstractions" };
        private Assembly FailedAssemblyResolutionHandler(object sender, ResolveEventArgs args)
        {
            // Get just the name of assembly without version and other metadata
            var truncatedName = new Regex(",.*").Replace(args.Name, string.Empty);

            if (!ExpectedFailedToLoadAssemblies.Contains(truncatedName))
            {
                AppendText(Color.Orange, $"Cannot load {args.RequestingAssembly?.FullName} which is not part of the expected assemblies that will not properly be loaded by the plug-in, returning null.");
                    return null;
            }
            // Load the most up to date version
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(truncatedName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            AppendText(Color.Orange, $"Cannot load {args.Name}, loading {assembly.FullName} instead.");

            return assembly;
        }


        private void StartWebHost()
        {
            AppendText(Color.Green, $"StartWebApplication called");

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += FailedAssemblyResolutionHandler;

            try
            {
                var baseAddress = "http://localhost:9000/";

                WebAppProxy?.Dispose();

                // Start OWIN host 
                WebAppProxy = WebApp.Start(baseAddress,
                    (appBuilder) =>
                    {
                        WebHostStartup = new WebHostStartup(_project, _verseRef, this, _host, _parent, this);
                        WebHostStartup.Configuration(appBuilder);
                    });



                AppendText(Color.Green, "Owin Web Api host started");
            }
            finally
            {
                currentDomain.AssemblyResolve -= FailedAssemblyResolutionHandler;
            }
        }

        /// <summary>
        /// Called when the user selects a different project in Paratext from the drop down list
        /// We are unable to switch the project via the plugin API.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newProject"></param>
        private void ProjectChanged(IPluginChildWindow sender, IProject newProject)
        {
            SetProject(newProject, reloadWebHost: true);
        }


        /// <summary>
        /// Standard Paratext verse reference has been changed
        /// </summary>
        /// <param name="verseRef"></param>
        /// <param name="reloadWebHost"></param>
        private void SetVerseRef(IVerseRef verseRef, bool reloadWebHost = false)
        {
            _verseRef = verseRef;
            if (reloadWebHost)
            {
                StartWebHost();
            }
        }

        /// <summary>
        /// Standard the project has changed
        /// </summary>
        /// <param name="newProject"></param>
        /// <param name="reloadWebHost"></param>
        private void SetProject(IProject newProject, bool reloadWebHost = false)
        {
            _project = newProject;
            if (reloadWebHost)
            {
                StartWebHost();
            }
        }

        /// <summary>
        /// Paratext has a verse change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="oldReference"></param>
        /// <param name="newReference"></param>
        private async void VerseRefChanged(IPluginChildWindow sender, IVerseRef oldReference, IVerseRef newReference)
        {
            // send the new verse & text collections
            try
            {
                if (newReference != _verseRef)
                {
                    //SetVerseRef(newReference, reloadWebHost: true);

                    _verseRef = newReference;

                    WebHostStartup.ChangeVerse(newReference);

                    try
                    {
                        AppendText(Color.DarkOrange, $"Sending verse - {_verseRef.BBBCCCVVV.ToString()}");
                        await HubContext.Clients.All.SendVerse(_verseRef.BBBCCCVVV.ToString());

                    }
                    catch (Exception ex)
                    {
                        AppendText(Color.Red,
                            $"Unexpected error occurred calling PluginHub.SendVerse() : {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "An unexpected error occurred when the Verse reference changed.");
            }

        }

        #endregion Paratext overrides - standard functions


        #region Methods

        private void OnExceptionOccurred(Exception exception)
        {
            Log.Error($"OnLoad {exception.Message}");
            AppendText(Color.Red, $"OnLoad {exception.Message}");
        }

        private void DisplayPluginVersion()
        {
            // get the version information
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format($@"Plugin Version: {version}");
        }

        private static void ConfigureLogging()
        {

            var fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects\\Logs\\Plugin.log");
            //create if does not exist
            DirectoryInfo di = new DirectoryInfo(fullPath);
            if (di.Exists == false)
            {
                di.Create();
            }

            // configure Serilog
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File(fullPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // set instance to global logger
            Log.Logger = log;
        }

        /// <summary>
        /// Gets the list of all the available Paratext projects
        /// </summary>
        private void UpdateProjectList()
        {
            _projectList.Clear();
            var windows = _host.AllOpenWindows;
            //ProjectsListBox.Items.Clear();
            foreach (var window in windows)
            {
                if (window is ITextCollectionChildState tc)
                {
                    var projects = tc.AllProjects;
                    foreach (var proj in projects)
                    {
                        _projectList.Add(proj);
                        //ProjectsListBox.Items.Add(proj.ShortName);
                    }
                    _verseRef = tc.VerseRef;
                    break;
                }
            }
            //if (ProjectsListBox.Items.Count > 0)
            //{
            //    ProjectsListBox.SelectedIndex = 0;
            //    ProjectListBox_SelectedIndexChanged(null, null);
            //}
        }


        /// <summary>
        /// Used by a slice to get the USFM data from the currently connected project's
        /// textcollection panel in Paratext
        /// </summary>
        /// <returns></returns>
        public List<TextCollection> GetTextCollectionsData()
        {
            // get the text collections
            List<TextCollection> textCollections = new();

            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { GetTextCollectionsData(); };
                this.Invoke(del);
                return _textCollections;
            }
            else
            {
                _textCollections.Clear();

                var windows = _host.AllOpenWindows;
                foreach (var window in windows)
                {
                    // check if window is text collection
                    if (window is ITextCollectionChildState tc)
                    {
                        // get the projects for this window
                        var projects = tc.AllProjects;
                        foreach (var project in projects)
                        {
                            TextCollection textCollection = new();
                            if (project != null)
                            {
                                IEnumerable<IUSFMToken> tokens = null;
                                try
                                {
                                    tokens = project.GetUSFMTokens(_verseRef.BookNum, _verseRef.ChapterNum,
                                        _verseRef.VerseNum);
                                }
                                catch (Exception e)
                                {
                                    Log.Logger.Error(e, $"Cannot get USFM Tokens for {project.ShortName} : {e.Message}");
                                }

                                if (tokens != null)
                                {
                                    textCollection.ReferenceShort = project.ShortName;

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
                                                //textCollection.Data += $"{marker.Type} Marker: {marker.Data}";
                                            }
                                        }
                                        else if (token is IUSFMTextToken textToken)
                                        {
                                            if (token.IsScripture)
                                            {
                                                textCollection.Data += textToken.Text + " ";
                                            }
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
                                            textCollection.Data =
                                                textCollection.Data.Substring(0, textCollection.Data.Length - 2);
                                        }
                                    }

                                    textCollections.Add(textCollection);
                                }
                            }
                        }
                    }
                }
            }

            _textCollections = textCollections;

            return _textCollections;
        }


        /// <summary>
        /// Append colored text to the rich text box
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public void AppendText(Color color, string message)
        {
            //check for threading issues
            if (this.InvokeRequired)
            {
                this.Invoke(new AppendMsgTextDelegate(AppendText), new object[] { color, message });
            }
            else
            {
                message += $"{Environment.NewLine}";
                this.rtb.AppendText(message, color);
                Log.Information(message);
            }
        }

        /// <summary>
        /// Used to generate clean USFM files in the \My Documents\ClearDashboard_Projects\Data directory
        /// Organized by the project's guid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportUSFM_Click(object sender, EventArgs e)
        {
            var paratextExtractUSFM = new ParatextExtractUSFM();
            paratextExtractUSFM.ExportUSFMScripture(_project, this);
        }

        public void SwitchVerseReference(int book, int chapter, int verse)
        {
            if (this.InvokeRequired)
            {
                Action safeWrite = delegate { SwitchVerseReference(book, chapter, verse); };
                this.Invoke(safeWrite);
            }
            else
            {
                try
                {
                    // set up a new Versification reference for this verse
                    var newVerse = _project.Versification.CreateReference(book, chapter, verse);
                    _host.SetReferenceForSyncGroup(newVerse, _host.ActiveWindowState.SyncReferenceGroup);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    AppendText(Color.Red, ex.Message);
                }
            }
        }

        /// <summary>
        /// Clear the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearWindow_Click(object sender, EventArgs e)
        {
            // clear out the existing data
            if (rtb.InvokeRequired)
            {
                rtb.Invoke((MethodInvoker)(() => rtb.Clear()));
            }
            else
            {
                rtb.Clear();
            }

            AppendText(Color.Green, DateTime.Now.ToShortTimeString());
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var hubProxy = GlobalHost.ConnectionManager.GetHubContext<PluginHub>();
            if (hubProxy == null)
            {
                AppendText(Color.Red, "HubContext is null");
                return;
            }

            hubProxy.Clients.All.Send(Guid.NewGuid(), @"Can you hear me?");
        }

        /// <summary>
        /// Used to generate data for unit tests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVersificationTest_Click(object sender, EventArgs e)
        {
            var v = _project.Versification;

            var newVerse = v.CreateReference(19, 20, 1);

            newVerse = v.ChangeVersification(newVerse);
        }


        public List<ParatextProjectMetadata> GetProjectMetadata()
        {
            var projects = _host.GetAllProjects(true);


            var metadata=  projects.Select(project => new ParatextProjectMetadata
                {
                    Id = project.ID,
                    LanguageName = project.LanguageName,
                    Name = project.ShortName,
                    LongName = project.LongName,
                    CorpusType = DetermineCorpusType(project.Type),
                    IsRtl = project.Language.IsRtoL,
                    AvailableBooks = project.GetAvailableBooks(),
            })
                .ToList();

            var projectNames = metadata.Select(project => project.Name).ToList();

            var directoryInfo = new DirectoryInfo(GetParatextProjectsPath());
            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories.Where(directory=> projectNames.Contains(directory.Name)))
            {
                var projectMetadatum = metadata.FirstOrDefault(metadatum => metadatum.Name == directory.Name);
                if (projectMetadatum != null)
                {
                    projectMetadatum.ProjectPath = directory.FullName;
                }
            }

            foreach (var directory in directories.Where(directory => !projectNames.Contains(directory.Name)))
            {
                var projectMetadatum = metadata.FirstOrDefault(metadatum => metadatum.Name == directory.Name);
                if (projectMetadatum != null)
                {
                    projectMetadatum.CorpusType = CorpusType.Resource;
                }
            }

            return metadata;
        }

        /// <summary>
        /// Returns the directory path to where the Paratext project resides
        /// </summary>
        /// <returns></returns>
        private string GetParatextProjectsPath()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
        }

        /// <summary>
        /// Returns the Paratext project type
        /// </summary>
        /// <param name="projectType"></param>
        /// <returns></returns>
        private CorpusType DetermineCorpusType(ProjectType projectType)
        {
            try
            {
                return (CorpusType)(projectType);
            }
            catch
            {
                return CorpusType.Unknown;
            }
        }



        /// <summary>
        /// Called by a slice
        /// ClearDashboard.WebApiParatextPlugin.Features.AllProjects
        /// </summary>
        /// <param name="showInConsole"></param>
        /// <returns></returns>
        public List<ParatextProject> GetAllProjects(bool showInConsole = false)
        {
            List<ParatextProject> allProjects = new();
            // get all the projects & resources
            var projects = _host.GetAllProjects(true);

            projects.ForEach(p => allProjects.Add(BuildParatextProject(p)));

            allProjects = allProjects.OrderBy(x => x.Type)
                .ThenBy(n => n.ShortName)
                .ToList();

            if (showInConsole)
            {
                foreach (var p in allProjects)
                {
                    var text = $"{p.ShortName} is a {p.CorpusType} Project: {p.Guid}";

                    switch (p.Type)
                    {
                        case ParatextProject.ProjectType.Auxiliary:
                            AppendText(Color.Brown, text);
                            break;
                        case ParatextProject.ProjectType.BackTranslation:
                            AppendText(Color.DarkOliveGreen, text);
                            break;
                        case ParatextProject.ProjectType.ConsultantNotes:
                            AppendText(Color.Aqua, text);
                            break;
                        case ParatextProject.ProjectType.Daughter:
                            AppendText(Color.DeepPink, text);
                            break;
                        case ParatextProject.ProjectType.MarbleResource:
                            AppendText(Color.BlueViolet, text);
                            break;
                        case ParatextProject.ProjectType.Resource:
                            AppendText(Color.Orange, text);
                            break;
                        case ParatextProject.ProjectType.Standard:
                            AppendText(Color.CadetBlue, text);
                            break;
                        case ParatextProject.ProjectType.StudyBible:
                        case ParatextProject.ProjectType.StudyBibleAdditions:
                            AppendText(Color.DarkSalmon, text);
                            break;
                        case ParatextProject.ProjectType.TransliterationManual:
                        case ParatextProject.ProjectType.TransliterationWithEncoder:
                            AppendText(Color.DarkSlateBlue, text);
                            break;
                        case ParatextProject.ProjectType.XmlDictionary:
                            AppendText(Color.Brown, text);
                            break;
                        case ParatextProject.ProjectType.SourceLanguage:
                            AppendText(Color.DarkGreen, text);
                            break;
                    }
                }
            }

            // test 
            //var proj = allProjects.FirstOrDefault(x => x.ShortName == "HEB/GRK");
            //if (proj != null)
            //{
            //    var referenceUsfm = GetReferenceUSFM(proj.Guid);
            //}
            

            return allProjects;
        }


        /// <summary>
        /// takes in a project and builds a model from it
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private ParatextProject BuildParatextProject(IProject project)
        {
            var paratextProject = new ParatextProject();

            paratextProject.ShortName = project.ShortName;
            paratextProject.Name = project.ShortName;
            paratextProject.Guid = project.ID;
            paratextProject.LongName = project.LongName;

            foreach (var book in project.AvailableBooks)
            {
                paratextProject.AvailableBooks.Add(new BookInfo
                {
                    Code = book.Code,
                    InProjectScope = book.InProjectScope,
                    Number = book.Number
                });
            }

            foreach (var user in project.NonObserverUsers)
            {
                paratextProject.NonObservers.Add(user.ToString());
            }

            paratextProject.NormalizationForm = project.NormalizationType.ToString();

            if (project.LanguageName != null)
            {
                paratextProject.LanguageName = project.LanguageName;
            }

            if (project.Language.Font != null)
            {
                paratextProject.DefaultFont = project.Language.Font.FontFamily;
            }

            if (project.Language.Id != null)
            {
                paratextProject.LanguageIsoCode = project.Language.Id;
            }

            if (project.Language.IsRtoL)
            {
                paratextProject.IsRTL = true;
            }

            if (project.BaseProject != null)
            {
                var corpusType = CorpusType.Unknown;

                switch (project.BaseProject.Type)
                {
                    case ProjectType.Standard:
                        corpusType = CorpusType.Standard;
                        break;
                    case ProjectType.BackTranslation:
                        corpusType = CorpusType.BackTranslation;
                        break;
                    case ProjectType.EnhancedResource:
                        corpusType = CorpusType.MarbleResource;
                        break;
                    case ProjectType.Auxiliary:
                        corpusType = CorpusType.Auxiliary;
                        break;
                    case ProjectType.Daughter:
                        corpusType = CorpusType.Daughter;
                        break;
                    case ProjectType.TransliterationManual:
                        corpusType = CorpusType.TransliterationManual;
                        break;
                    case ProjectType.TransliterationWithEncoder:
                        corpusType = CorpusType.TransliterationWithEncoder;
                        break;
                    case ProjectType.ConsultantNotes:
                        corpusType = CorpusType.ConsultantNotes;
                        break;
                    case ProjectType.StudyBible:
                        corpusType = CorpusType.StudyBible;
                        break;
                    case ProjectType.StudyBibleAdditions:
                        corpusType = CorpusType.StudyBibleAdditions;
                        break;
                    case ProjectType.Xml:
                        corpusType = CorpusType.Xml;
                        break;
                    case ProjectType.SourceLanguage:
                        corpusType = CorpusType.SourceLanguage;
                        break;
                }

                paratextProject.BaseTranslation = new TranslationInfo
                {
                    CorpusType = corpusType,
                    ProjectName = project.BaseProject.ShortName,
                    ProjectGuid = project.ID,
                };

            }

            
            if (project.Type != null)
            {
                paratextProject.CorpusType = CorpusType.Unknown;

                switch (project.Type)
                {
                    case ProjectType.Standard:
                        paratextProject.CorpusType = CorpusType.Standard;
                        paratextProject.Type = ParatextProject.ProjectType.Standard;
                        break;
                    case ProjectType.BackTranslation:
                        paratextProject.CorpusType = CorpusType.BackTranslation;
                        paratextProject.Type = ParatextProject.ProjectType.BackTranslation;
                        break;
                    case ProjectType.EnhancedResource:
                        paratextProject.CorpusType = CorpusType.MarbleResource;
                        paratextProject.Type = ParatextProject.ProjectType.MarbleResource;
                        break;
                    case ProjectType.Auxiliary:
                        paratextProject.CorpusType = CorpusType.Auxiliary;
                        paratextProject.Type = ParatextProject.ProjectType.Auxiliary;
                        break;
                    case ProjectType.Daughter:
                        paratextProject.CorpusType = CorpusType.Daughter;
                        paratextProject.Type = ParatextProject.ProjectType.Daughter;
                        break;
                    case ProjectType.TransliterationManual:
                        paratextProject.CorpusType = CorpusType.TransliterationManual;
                        paratextProject.Type = ParatextProject.ProjectType.TransliterationManual;
                        break;
                    case ProjectType.TransliterationWithEncoder:
                        paratextProject.CorpusType = CorpusType.TransliterationWithEncoder;
                        paratextProject.Type = ParatextProject.ProjectType.TransliterationWithEncoder;
                        break;
                    case ProjectType.ConsultantNotes:
                        paratextProject.CorpusType = CorpusType.ConsultantNotes;
                        paratextProject.Type = ParatextProject.ProjectType.ConsultantNotes;
                        break;
                    case ProjectType.StudyBible:
                        paratextProject.CorpusType = CorpusType.StudyBible;
                        paratextProject.Type = ParatextProject.ProjectType.StudyBible;
                        break;
                    case ProjectType.StudyBibleAdditions:
                        paratextProject.CorpusType = CorpusType.StudyBibleAdditions;
                        paratextProject.Type = ParatextProject.ProjectType.StudyBibleAdditions;
                        break;
                    case ProjectType.Xml:
                        paratextProject.CorpusType = CorpusType.Xml;
                        paratextProject.Type = ParatextProject.ProjectType.XmlDictionary;
                        break;
                    case ProjectType.SourceLanguage:
                        paratextProject.CorpusType = CorpusType.SourceLanguage;
                        paratextProject.Type = ParatextProject.ProjectType.SourceLanguage;
                        break;
                }
            }
            Debug.WriteLine($"{project.ShortName} {project.Type} {paratextProject.CorpusType}");

            paratextProject.Versification = (int)project.Versification.Type;

            return paratextProject;
        }

        /// <summary>
        /// Gets the USFM for a project with the following project guid
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public ReferenceUsfm GetReferenceUSFM(string projectId)
        {
            ReferenceUsfm referenceUsfm = new();
            referenceUsfm.Id = projectId;

            // get all the projects & resources
            var projects = _host.GetAllProjects(true);
            var project = projects.FirstOrDefault(p => p.ID == projectId);

            if (project == null)
            {
                return referenceUsfm;
            }

            // creating usfm directory
            try
            {
                var paratextExtractUSFM = new ParatextExtractUSFM();
                var usfmHelper = paratextExtractUSFM.ExportUSFMScripture(project, this);

                referenceUsfm.UsfmDirectoryPath = usfmHelper.Path;
                referenceUsfm.Name = project.ShortName;
                referenceUsfm.LongName = project.LongName;
                referenceUsfm.Language = project.LanguageName;
                referenceUsfm.IsRTL = project.Language.IsRtoL;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                AppendText(Color.Red, e.Message);
            }


            return referenceUsfm;
        }

        /// <summary>
        /// Gets the USFM for a project with the following project guid
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public UsfmHelper GetCheckForUsfmErrors(string projectId)
        {
            // get all the projects & resources
            var projects = _host.GetAllProjects(true);
            var project = projects.FirstOrDefault(p => p.ID == projectId);

            if (project == null)
            {
                return new UsfmHelper
                {
                    Path = null,
                    NumberOfErrors = 0,
                    UsfmErrors = new List<UsfmError>()
                };
            }

            // creating usfm directory
            UsfmHelper usfmHelper = new();
            try
            {
                var paratextExtractUSFM = new ParatextExtractUSFM();
                usfmHelper = paratextExtractUSFM.ExportUSFMScripture(project, this);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                AppendText(Color.Red, e.Message);
            }

            return usfmHelper;
        }
        /// <summary>
        /// Called by a slice
        /// returns both the versification and the list of books available
        /// </summary>
        /// <param name="ParatextProjectId"></param>
        /// <returns></returns>
        public VersificationBookIds GetVersificationAndBooksForProject(string ParatextProjectId)
        {
            // get the right project
            // get all the projects & resources
            var projects = _host.GetAllProjects(true);
            var project = projects.FirstOrDefault(p => p.ID == ParatextProjectId);

            var versificationBookIds = new VersificationBookIds();

            if (project != null)
            {
                switch (project.Versification.Type)
                {
    
                    case StandardScrVersType.English:
                        versificationBookIds.Versification = ScrVers.English;
                        break;
                    case StandardScrVersType.RussianProtestant:
                        versificationBookIds.Versification = ScrVers.RussianProtestant;
                        break;
                    case StandardScrVersType.RussianOrthodox:
                        versificationBookIds.Versification = ScrVers.RussianOrthodox;
                        break;
                    case StandardScrVersType.Original:
                        versificationBookIds.Versification = ScrVers.Original;
                        break;
                    case StandardScrVersType.Vulgate:
                        versificationBookIds.Versification = ScrVers.Vulgate;
                        break;
                    case StandardScrVersType.Unknown:
                        // there is no "Unknown" ScrVers so set to english
                        versificationBookIds.Versification = ScrVers.English;
                        break;

                    default:
                        // default to english
                        versificationBookIds.Versification = ScrVers.English;
                        break;
                }

                var books = project.AvailableBooks.Where(b => b.Code != "");
                versificationBookIds.BookAbbreviations = books.Select(item => item.Code).ToList();
                return versificationBookIds;
            }
            return new VersificationBookIds();
        }


        /// <summary>
        /// Given a projectId and bookId, return the parsed verse text for the book
        ///
        /// called from: ClearDashboard.WebApiParatextPlugin.Features.BookUsfm
        /// </summary>
        /// <param name="paratextProjectId"></param>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public List<UsfmVerse> GetUsfmForBook(string paratextProjectId, string bookId)
        {
            // get all the projects & resources
            var projects = _host.GetAllProjects(true);
            // get the right project
            var project = projects.FirstOrDefault(p => p.ID == paratextProjectId);

            var verses = new List<UsfmVerse>();
            if (project == null)
            {
                AppendText(Color.Orange, $"Could not find a project with Id = '{paratextProjectId}'. Returning an empty list.");
                return verses;
            }

            // filter down to the book desired
            var book = project.AvailableBooks.FirstOrDefault(b => b.Code == bookId);
            if (book == null)
            {
                AppendText(Color.Orange, $"Could not find a book with Id = '{bookId}'. Returning an empty list.");
                return verses;
            }
            
            // only return information for "bible books" and not the extra material
            // TODO - is this true??
            if (BibleBookScope.IsBibleBook(book.Code) == false)
            {
                AppendText(Color.Orange, $"'{book.Code}' is not a bible book. Returning an empty list.");
                return verses;
            }

            AppendText(Color.Blue, $"Processing {book.Code}");

            var sb = new StringBuilder();
            IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
            try
            {
                // get tokens by book number (from object) and chapter
                tokens = project.GetUSFMTokens(book.Number);
            }
            catch (Exception)
            {
                AppendText(Color.Orange, $"No Scripture for {bookId}");
                return null;
            }

            var chapter = "";
            var verse = "";
            var verseText = "";

            var lastTokenChapter = false;
            var lastTokenText = false;
            var lastVerseZero = false;
            foreach (var token in tokens)
            {
                if (token is IUSFMMarkerToken marker)
                {
                    // a verse token
                    if (marker.Type == MarkerType.Verse)
                    {
                        lastTokenText = false;
                        if (!lastTokenChapter || lastVerseZero)
                        {
                            sb.AppendLine();
                        }

                        // this includes single verses (\v 1) and multiline (\v 1-3)
                        sb.Append($@"\v {marker.Data.Trim()} ");

                        if (verse != "" && chapter != "" && verseText != "")
                        {
                            var usfm = new UsfmVerse
                            {
                                Chapter = chapter,
                                Verse = verse,
                                Text = verseText
                            };
                            verses.Add(usfm);
                            verseText = "";
                        }

                        verse = marker.Data.Trim();

                        lastTokenChapter = false;
                        lastVerseZero = false;
                    }
                    else if (marker.Type == MarkerType.Chapter)
                    {
                        lastVerseZero = false;
                        lastTokenText = false;
                        // new chapter
                        sb.AppendLine();
                        sb.AppendLine();
                        sb.AppendLine(@"\c " + marker.Data);

                        if (verse != "" && chapter != "" && verseText != "")
                        {
                            var usfm = new UsfmVerse
                            {
                                Chapter = chapter,
                                Verse = verse,
                                Text = verseText
                            };
                            verses.Add(usfm);
                            verseText = "";
                        }
                        chapter = marker.Data.Trim();
                        verse = string.Empty;

                        lastTokenChapter = true;
                    }
                }
                else if (token is IUSFMTextToken textToken)
                {
                    if (token.IsScripture)
                    {
                        // verse text

                        // check to see if this is a verse zero
                        if (textToken.VerseRef.VerseNum == 0)
                        {
                            if (lastVerseZero == false)
                            {
                                sb.Append(@"\v 0 " + textToken.Text);
                                verseText = textToken.Text;
                            }
                            else
                            {
                                sb.Append(textToken.Text);
                                verseText = textToken.Text;
                            }

                            lastVerseZero = true;
                            lastTokenText = true;
                        }
                        else
                        {
                            // check to see if the last character is a space
                            if (sb[sb.Length - 1] == ' ' && lastTokenText)
                            {
                                sb.Append(textToken.Text.TrimStart());
                                verseText += textToken.Text.TrimStart();
                            }
                            else
                            {
                                if (sb[sb.Length - 1] == ' ' && textToken.Text.StartsWith(" "))
                                {
                                    sb.Append(textToken.Text.TrimStart());
                                    verseText = textToken.Text.TrimStart();
                                }
                                else
                                {
                                    sb.Append(textToken.Text);
                                    verseText += textToken.Text;
                                }

                            }

                            lastTokenText = true;
                        }
                    }
                }
            }

            // do the last verse
            if (verse != "" && chapter != "" && verseText != "")
            {
                var usfm = new UsfmVerse
                {
                    Chapter = chapter,
                    Verse = verse,
                    Text = verseText
                };
                verses.Add(usfm);
            }

            //foreach (var v in verses)
            //{
            //    Console.WriteLine($"{v.Chapter}:{v.Verse} {v.Text}");
            //}

            return verses;

        }

        #endregion

        private void ProjectListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var found = false;
            //if (ProjectsListBox.SelectedItem != null)
            //{
            //    var name = ProjectsListBox.SelectedItem.ToString();
            //    foreach (var proj in _projectList.Where(proj => name == proj.ShortName))
            //    {
            //        ShowScripture(proj);
            //        found = true;
            //        break;
            //    }
            //}
            //if (!found)
            //{
            //    textBox.Text = "Cannot find project.";
            //}

        }

        //private void ShowScripture(IProject project)
        //{
        //    var lines = new List<string>();
        //    if (_project == null)
        //    {
        //        lines.Add("No project to display");
        //    }
        //    else
        //    {
        //        lines.Add("USFM Tokens:");
        //        IEnumerable<IUSFMToken> tokens = null;
        //        var sawException = false;
        //        try
        //        {
        //            tokens = project.GetUSFMTokens(_verseRef.BookNum, _verseRef.ChapterNum, _verseRef.VerseNum);
        //        }
        //        catch (Exception e)
        //        {
        //            lines.Add($" Cannot get the USFM Tokens for this project because {e.Message}");
        //            sawException = true;
        //        }

        //        if ((tokens == null) && (sawException == false))
        //        {
        //            lines.Add("Cannot get the USFM Tokens for this project");
        //        }

        //        if (tokens != null)
        //        {
        //            lines.Add(_project.ShortName);

        //            foreach (var token in tokens)
        //            {
        //                if (token is IUSFMMarkerToken marker)
        //                {
        //                    if (marker.Type == MarkerType.Verse)
        //                    {
        //                        //skip
        //                    }
        //                    else if (marker.Type == MarkerType.Paragraph)
        //                    {
        //                        lines.Add("/");
        //                    }
        //                    else
        //                    {
        //                        //lines.Add($"{marker.Type} Marker: {marker.Data}");
        //                    }
        //                }
        //                else if (token is IUSFMTextToken textToken)
        //                {
        //                    if (textToken.IsScripture)
        //                    {
        //                        lines.Add(textToken.Text);
        //                    }
        //                }
        //                else if (token is IUSFMAttributeToken)
        //                {
        //                    //lines.Add("Attribute Token: " + token.ToString());
        //                }
        //                else
        //                {
        //                    //lines.Add("Unexpected token type: " + token.ToString());
        //                }
        //            }

        //            // remove the last paragraph tag if at the end
        //            if (lines.Count > 0)
        //            {
        //                if (lines[lines.Count - 1] == "/")
        //                {
        //                    lines.RemoveAt(lines.Count - 1);
        //                }
        //            }
        //        }

        //        textBox.Lines = lines.ToArray();
        //    }
        //}
    }
}
