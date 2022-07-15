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
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ClearDashboard.WebApiParatextPlugin
{
    public partial class MainWindow : EmbeddedPluginControl, IPluginLogger
    {

        #region Properties

        private IProject _project;
        private List<IProject> m_ProjectList = new ();
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

        private void DisplayPluginVersion()
        {
            // get the version information
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format($@"Plugin Version: {version}");
        }

        private static void ConfigureLogging()
        {
            // configure Serilog
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File("d:\\temp\\Plugin.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // set instance to global logger
            Log.Logger = log;
        }

        #endregion

        private void HandleWindowDisposed(object sender, EventArgs e)
        {
            WebAppProxy?.Dispose();
            WebAppProxy = null;
        }

        private void OnExceptionOccurred(Exception exception)
        {
            Log.Error($"OnLoad {exception.Message}");
            AppendText(Color.Red, $"OnLoad {exception.Message}");
        }
  

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

            // Since DoLoad is done on a different thread than what was used
            // to create the control, we need to use the Invoke method.
            //Invoke((Action)(() => UpdateProjectList()));
            //Invoke((Action)(() => ShowScripture()));

            UpdateProjectList();
            ShowScripture(_project);
        }

        public override string GetState()
        {
            // override required by base class, return null string.
            return null;
        }

       
        public override void DoLoad(IProgressInfo progressInfo)
        {
            // Since DoLoad is done on a different thread than what was used
            // to create the control, we need to use the Invoke method.
            Invoke((Action)(() => GetAllProjects()));

            StartWebHost();
        }

        private Assembly FailedAssemblyResolutionHandler(object sender, ResolveEventArgs args)
        {
            // Get just the name of assembly without version and other metadata
            var truncatedName = new Regex(",.*").Replace(args.Name, string.Empty);

            if (truncatedName.Contains("XmlSerializers"))
            {
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
            AppendText(Color.Red, $"Cannot load {args.Name}, loading {assembly.FullName} instead.");

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
                        WebHostStartup = new WebHostStartup(_project, _verseRef, this, _host, this);
                        WebHostStartup.Configuration(appBuilder);
                    });

              

                AppendText(Color.Green, "Owin Web Api host started");
            }
            finally
            {
                currentDomain.AssemblyResolve -= FailedAssemblyResolutionHandler;
            }
        }

        private void ProjectChanged(IPluginChildWindow sender, IProject newProject)
        {
            SetProject(newProject, reloadWebHost:true);
        }


        private void SetVerseRef(IVerseRef verseRef, bool reloadWebHost = false)
        {
            _verseRef = verseRef;
            if (reloadWebHost)
            {
                StartWebHost();
            }
        }

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

                    try
                    {
                        await HubContext.Clients.All.SendVerse(_verseRef.BBBCCCVVV.ToString());

                    }
                    catch (Exception ex)
                    {
                        AppendText(Color.Red,
                            $"Unexpected error occurred calling PluginHub.SendVerse() : {ex.Message}");
                    }


                    //var textCollections = GetTextCollectionsData();

                    //if (textCollections.Count > 0)
                    //{
                    //    try
                    //    {
                    //        await HubContext.Clients.All.SendTextCollections(textCollections);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        AppendText(Color.Red,
                    //            $"Unexpected error occurred calling PluginHub.SendTextCollections() : {ex.Message}");
                    //    }
                    //}

                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "An unexpected error occurred when the Verse reference changed.");
            }

        }

        #endregion Paratext overrides - standard functions


        #region Methods

        private void UpdateProjectList()
        {
            m_ProjectList.Clear();
            var windows = _host.AllOpenWindows;
            ProjectsListBox.Items.Clear();
            foreach (var window in windows)
            {
                if (window is ITextCollectionChildState tc)
                {
                    var projects = tc.AllProjects;
                    foreach (var proj in projects)
                    {
                        m_ProjectList.Add(proj);
                        ProjectsListBox.Items.Add(proj.ShortName);
                    }
                    _verseRef = tc.VerseRef;
                    break;
                }
            }
            if (ProjectsListBox.Items.Count > 0)
            {
                ProjectsListBox.SelectedIndex = 0;
                ProjectListBox_SelectedIndexChanged(null, null);
            }
        }

        private void ProjectListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool found = false;
            if (ProjectsListBox.SelectedItem != null)
            {
                string name = ProjectsListBox.SelectedItem.ToString();
                foreach (var proj in m_ProjectList)
                {
                    if (name == proj.ShortName)
                    {
                        ShowScripture(proj);
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                textBox.Text = $"Cannot find project.";
            }

        }

        private void ShowScripture(IProject project)
        {
            List<string> lines = new List<string>();
            if (_project == null)
            {
                lines.Add("No project to display");
            }
            else
            {
                lines.Add("USFM Tokens:");
                IEnumerable<IUSFMToken> tokens = null;
                bool sawException = false;
                try
                {
                    tokens = project.GetUSFMTokens(_verseRef.BookNum, _verseRef.ChapterNum, _verseRef.VerseNum);
                }
                catch (Exception e)
                {
                    lines.Add($" Cannot get the USFM Tokens for this project because {e.Message}");
                    sawException = true;
                }

                if ((tokens == null) && (sawException == false))
                {
                    lines.Add("Cannot get the USFM Tokens for this project");
                }

                if (tokens != null)
                {
                    lines.Add(_project.ShortName);

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
                                lines.Add("/");
                            }
                            else
                            {
                                //lines.Add($"{marker.Type} Marker: {marker.Data}");
                            }
                        }
                        else if (token is IUSFMTextToken textToken)
                        {
                            if (textToken.IsScripture)
                            {
                                lines.Add(textToken.Text);
                            }
                        }
                        else if (token is IUSFMAttributeToken)
                        {
                            //lines.Add("Attribute Token: " + token.ToString());
                        }
                        else
                        {
                            //lines.Add("Unexpected token type: " + token.ToString());
                        }
                    }

                    // remove the last paragraph tag if at the end
                    if (lines.Count > 0)
                    {
                        if (lines[lines.Count - 1] == "/")
                        {
                            lines.RemoveAt(lines.Count - 1);
                        }
                    }
                }

                textBox.Lines = lines.ToArray();
            }
        }

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

        private void btnExportUSFM_Click(object sender, EventArgs e)
        {
            ParatextExtractUSFM paratextExtractUSFM = new ParatextExtractUSFM();
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
        /// Force a restart of the named pipes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRestart_Click(object sender, EventArgs e)
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
            AppendText(Color.Green, "_PipeServer Pipe Restarted");

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

        private void btnVersificationTest_Click(object sender, EventArgs e)
        {
            var v = _project.Versification;

            var newVerse = v.CreateReference(19, 20, 1);

            newVerse = v.ChangeVersification(newVerse);
        }

        private void GetAllProjects()
        {
            var projects = _host.GetAllProjects();

            listProjects.Items.Clear();
            foreach (var p in projects)
            {
                string text = $"{p.ShortName} is a {p.Type} Project: {p.ID}";
                listProjects.Items.Add(text);
            }
        }
        
        #endregion
    }
}
