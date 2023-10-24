using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.WebApiParatextPlugin.Extensions;
using ClearDashboard.WebApiParatextPlugin.Helpers;
using ClearDashboard.WebApiParatextPlugin.Hubs;
using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Hosting;
using Microsoft.Win32;
using Paratext.PluginInterfaces;
using Serilog;
using SIL.Linq;
using SIL.Scripture;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ProjectType = Paratext.PluginInterfaces.ProjectType;

namespace ClearDashboard.WebApiParatextPlugin
{
    public partial class MainWindow : EmbeddedPluginControl, IPluginLogger
    {

        #region Properties

        private IProject _project;
        private const int PortNumber = 9000;

        public IProject Project
        {
            get => _project;
            set => _project = value;
        }

        private List<IProject> _projectList = new();
        private IVerseRef _verseRef;
        private IWindowPluginHost _host;
        private IPluginChildWindow _parent;
        private IMediator _mediator;
        private IPluginLogger _logger;
        private IHubContext HubContext => GlobalHost.ConnectionManager.GetHubContext<PluginHub>();
        private WebHostStartup WebHostStartup { get; set; }
        private IDisposable WebAppProxy { get; set; }
        private delegate void AppendMsgTextDelegate(Color color, string text);

        private List<TextCollection> _textCollections = new();
        private List<ParatextProjectMetadata> _projectMetadata;
        private bool _hasFontErrorBeenDisplayed = false;
        private bool _hasSignalRfailed = false;

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

        private async void HandleWindowDisposed(object sender, EventArgs e)
        {
            WebAppProxy?.Dispose();
            WebAppProxy = null;
        }

        public override void OnAddedToParent(IPluginChildWindow parent, IWindowPluginHost host, string state)
        {
            FunctionsAssemblyResolver.RedirectAssembly();

            parent.SetTitle(ClearDashboardWebApiPlugin.PluginName);
            parent.ProjectChanged += ProjectChanged;
            parent.VerseRefChanged += VerseRefChanged;
            parent.WindowClosing += WindowClosing;

            SetProject(parent.CurrentState.Project);
            SetVerseRef(parent.CurrentState.VerseRef);

            _host = host;
            _project = parent.CurrentState.Project;
            _parent = parent;

            // get the version of SIL Convertors if present
            GetSilConvertorsVersion();


            UpdateProjectList();

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
            _logger = WebHostStartup.ServiceProvider.GetService<IPluginLogger>();
        }

        private async void WindowClosing(IPluginChildWindow sender, CancelEventArgs args)
        {
            try
            {
                HubContext.Clients.All.SendPluginClosing(new PluginClosing
                    { PluginConnectionChangeType = PluginConnectionChangeType.Closing });
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                AppendText(Color.Red,
                    $"Unexpected error occurred calling PluginHub.SendConnectionChange() : {ex.Message}");
            }
            finally
            {
                WebAppProxy?.Dispose();
            }
        }


        #endregion


        #region Paratext overrides - standard functions

        public override string GetState()
        {
            // override required by base class, return null string.
            return null;
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

        /// <summary>
        /// Called when the user selects a different project in Paratext from the drop down list
        /// We are unable to switch the project via the plugin API.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newProject"></param>
        private async void ProjectChanged(IPluginChildWindow sender, IProject newProject)
        {
            SetProject(newProject, reloadWebHost: false);

            WebHostStartup.ChangeProject(newProject);

            var hubProxy = GlobalHost.ConnectionManager.GetHubContext<PluginHub>();
            if (hubProxy == null)
            {
                AppendText(Color.Red, "HubContext is null");
                return;
            }

            try
            {
                var paratextProject = ConvertIProjectToParatextProject.BuildParatextProjectFromIProject(_project);
                AppendText(Color.DarkOrange, $"Sending project - {newProject.ShortName}");
                await HubContext.Clients.All.SendProject(paratextProject);
            }
            catch (Exception e)
            {
                AppendText(Color.Red, e.Message);
            }

        }

        #endregion Paratext overrides - standard functions


        #region WebServer Related

        private List<string> ExpectedFailedToLoadAssemblies = new List<string> { "Microsoft.Owin", "Microsoft.Extensions.DependencyInjection.Abstractions" };
        private Assembly FailedAssemblyResolutionHandler(object sender, ResolveEventArgs args)
        {
            // Get just the name of assembly without version and other metadata
            var truncatedName = new Regex(",.*").Replace(args.Name, string.Empty);

            if (!ExpectedFailedToLoadAssemblies.Contains(truncatedName))
            {
                if (args.Name.StartsWith("WeifenLuo") == false || args.Name.StartsWith("Paratext") == false)
                {
                    return null;
                }

                if (args.RequestingAssembly == null && args.Name.StartsWith("WeifenLuo") == false && args.Name.StartsWith("Paratext") == false)
                {
                    AppendText(Color.Orange, $"Cannot load {args.Name} which is not part of the expected assemblies that will not properly be loaded by the plug-in, returning null.");
                }
                else
                {
                    AppendText(Color.Orange, $"Cannot load {args.RequestingAssembly?.FullName} which is not part of the expected assemblies that will not properly be loaded by the plug-in, returning null.");
                }

                return null;
            }
            // Load the most up to date version
            Assembly assembly = null;
            try
            {
                _hasSignalRfailed = true;
                assembly = Assembly.Load(truncatedName);
            }
            catch (Exception e)
            {

                AppendText(Color.Red, $"Cannot load [{assembly?.FullName}] {e.Message}");
            }
            AppendText(Color.Orange, $"Cannot load:\n [{BustUpAssemblyName(args.Name)}]\nLoading instead:\n [{BustUpAssemblyName(assembly?.FullName)}]\n");

            return assembly;
        }

        private string BustUpAssemblyName(string assemblyName)
        {
            try
            {
                int index = assemblyName.IndexOf(',', assemblyName.IndexOf(',') + 1);
                assemblyName = assemblyName.Substring(0, index);
            }
            catch (Exception e)
            {
                AppendText(Color.Red, $"BustUpAssemblyName Error [{assemblyName}] [{e.Message}]");
                return assemblyName;
            }

            return assemblyName;
        }


        private async Task StartWebHost()
        {
            // check to see if the port is in use which means that we probably have 
            // a window open already
            if (PortInUse(PortNumber))
            {
                PortInUseMethod();
                return;
            }

            AppendText(Color.CornflowerBlue, "CHANGE PROJECTS using the dropdown in the tool bar.");
            AppendText(Color.CornflowerBlue, string.Empty);

            AppendText(Color.Green, "StartWebApplication called");

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += FailedAssemblyResolutionHandler;

            try
            {
                bool error = false;
                var baseAddress = $"http://localhost:{PortNumber}/";
                WebAppProxy?.Dispose();

                // Start OWIN host 
                WebAppProxy = WebApp.Start(baseAddress,
                    (appBuilder) =>
                    {
                        try
                        {
                            GlobalHost.Configuration.ConnectionTimeout = new TimeSpan(0, 0, 10);
                            GlobalHost.Configuration.DisconnectTimeout = new TimeSpan(0, 0, 10);
                            GlobalHost.Configuration.KeepAlive = new TimeSpan(0, 0, 3);
                        }
                        catch (Exception e)
                        {
                            AppendText(Color.Purple, $"\n\nERROR: {e.Message}");
                            if (e.Message.StartsWith("Disconnect"))
                            {
                                ServerInUseMethod();
                            }

                            error = true;
                        }
                        finally
                        {
                            WebHostStartup = new WebHostStartup(_project, _verseRef, this, _host, _parent, this);
                            WebHostStartup.Configuration(appBuilder);
                        }
                    });

                if (error == false)
                {
                    AppendText(Color.Green, "Owin Web Api host started");
                }
            }
            catch (Exception e)
            {
                if (e.InnerException.Message.StartsWith("Failed to listen on prefix 'http://localhost:"))
                {
                    PortInUseMethod();
                }
                else
                {
                    AppendText(Color.Purple,
                        $"\n\nERROR: {e.Message}");
                }
            }
            finally
            {
                currentDomain.AssemblyResolve -= FailedAssemblyResolutionHandler;
            }


            if (_hasSignalRfailed == false)
            {
                // signalR probably didn't load correctly and is listening
                await TestConnection();
            }
        }

        private async Task TestConnection()
        {
            var connection = new HubConnection("http://localhost:9000/signalr");
            var hubProxy = connection.CreateHubProxy("Plugin");

            hubProxy.On<string, string>("send", (name, msg) =>
            {
                var message = $"{name} - {msg}";
                AppendText(Color.Orange, $"\n\nERROR: {message}");
            });

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                await connection.Start();
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                AppendText(Color.Purple, $"\nERROR: {e.Message}");
                AppendText(Color.Purple, $"TIME (ms): {stopwatch.ElapsedMilliseconds}");
                return;
            }

            _ = await hubProxy.Invoke<string>("ping", "Message", 0);
        }

        /// <summary>
        /// Method to check if a port is in use or not
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }

        private void PortInUseMethod()
        {
            ClearTextWindow();

            AppendText(Color.Purple,
                $"\n\nIF YOU WISH TO SWITCH THE DASHBOARD PLUGIN TO BE ASSOCIATED WITH THE {_project.ShortName} PROJECT, PLEASE CLOSE ALL OTHER OPEN DASHBOARD PLUGIN WINDOWS FIRST.");
        }

        private void ServerInUseMethod()
        {
            ClearTextWindow();

            AppendText(Color.Purple,
                $"\n\nYOU NEED TO RESTART PARATEXT AS PARATEXT HAS NOT CLOSED THE WEB SERVER PROPERLY");

            PlaySound.PlaySoundFromResource(SoundType.Error);
        }

        private void OnExceptionOccurred(Exception exception)
        {
            Log.Error($"OnLoad {exception.Message}");
            AppendText(Color.Red, $"OnLoad {exception.Message}");
        }

        #endregion

        #region Methods

        private void GetSilConvertorsVersion()
        {
            // get the path to the SIL Converters uninstaller
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\SIL\SilEncConverters40\Installer");

            //if it does exist, retrieve the stored values  
            if (key != null)
            {
                // remove the trailing file name from the path
                var path = Path.GetDirectoryName(key.GetValue("InstallerPath").ToString());
                var filePath = Path.Combine(path, "Microsoft.Extensions.DependencyInjection.Abstractions.dll");
                if (File.Exists(filePath))
                {
                    // get the file version
                    var versionInfo = FileVersionInfo.GetVersionInfo(filePath);

                    AppendText(Color.SaddleBrown, "Microsoft.Extensions.DependencyInjection.Abstractions.dll");

                    AppendText(Color.SaddleBrown, $"Version {versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}");

                    if (versionInfo.FileMajorPart < 7)
                    {
                        AppendText(Color.Red, $"Incompatible version of SIL Converters Detected - Please update to at least version 5.2 from https://software.sil.org/silconverters/");
                    }
                    else
                    {
                        AppendText(Color.SeaGreen, $"SIL Converters 5.2 or higher Detected");
                    }

                    AppendText(Color.Black, $"");
                }
            }

            if (key is not null)
            {
                key.Close();
            }
            
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

        private void DisplayPluginVersion()
        {
            // get the version information
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format($@"Plugin Version: {version}");
        }

        private static void ConfigureLogging()
        {

            var fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects\\Logs\\Plugin.log");
            try
            {
                //create if does not exist
                DirectoryInfo di = new DirectoryInfo(fullPath);
                if (di.Exists == false)
                {
                    di.Create();
                }
            }
            catch (Exception)
            {

            }

            // configure Serilog
            // set to 150 MB limit on file size
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File(fullPath, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 104857600, rollOnFileSizeLimit: true, retainedFileCountLimit: 15)
                .CreateLogger();
            // set instance to global logger
            Log.Logger = log;
        }

        /// <summary>
        /// Gets the list of all the available Paratext projects
        /// </summary>
        private void UpdateProjectList()
        {
            // snag a list of all the projects
            var allProjectsList = _host.GetAllProjects();

            _projectList.Clear();


            var windows = _host.AllOpenWindows;
            foreach (var window in windows)
            {
                if (window is ITextCollectionChildState tc)
                {

                }
                else if (window is IParatextChildState win)
                {
                    if (win.Project is not null)
                    {
                        try
                        {
                            // add only those projects for which the window is open
                            var shortName = win.Project.ShortName;
                            var project = allProjectsList.FirstOrDefault(x => x.ShortName == shortName);
                            if (project != null)
                            {
                                _projectList.Add(project);
                            }
                        }
                        catch (Exception)
                        {
                            // no-op
                        }
                    }
                }

            }
        }


        /// <summary>
        /// Used by a slice to get the USFM data from the currently connected project's
        /// textcollection panel in Paratext
        /// </summary>
        /// <returns></returns>
        public List<TextCollection> GetTextCollectionsData(bool fetchUsx, bool isVerseByVerse)
        {
            // get the text collections
            List<TextCollection> textCollections = new();

            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { GetTextCollectionsData(fetchUsx, isVerseByVerse); };
                this.Invoke(del);
                return _textCollections;
            }
            else
            {
                _textCollections.Clear();
                var currentProjectGroup = SyncReferenceGroup.None;
                var windows = _host.AllOpenWindows;

                foreach (var window in windows)
                {
                    if (window.Project?.ID == _project.ID)
                    {
                        currentProjectGroup = window.SyncReferenceGroup;
                        break;
                    }
                }

                var currentProjectGroupIsNone = currentProjectGroup == SyncReferenceGroup.None;

                foreach (var window in windows)
                {
                    // check if window is text collection
                    if (window is ITextCollectionChildState tc && (window.SyncReferenceGroup == currentProjectGroup || currentProjectGroupIsNone))
                    {

                        // get the projects for this window
                        var projects = tc.AllProjects;
                        foreach (var project in projects)
                        {
                            TextCollection textCollection = new();
                            if (project != null)
                            {
                                if (fetchUsx)
                                {
                                    var usxString = project.GetUSX(_verseRef.BookNum);

                                    try
                                    {
                                        List<XmlNode> verseNodeList = new();
                                        XmlDocument xDoc = new();
                                        if (usxString != null)
                                        {
                                            xDoc.LoadXml(usxString);

                                            switch (isVerseByVerse)
                                            {
                                                case true:
                                                    verseNodeList = CreateVerseNodeListRecursive(project, xDoc.DocumentElement, xDoc, isVerseByVerse, false);
                                                    break;
                                                case false:
                                                    verseNodeList = CreateVerseNodeList(project, xDoc, isVerseByVerse, false);
                                                    break;
                                            }
                                        }

                                        if (verseNodeList.Count == 0)
                                        {
                                            textCollections = UsfmToTextCollection(project, textCollection, textCollections);
                                        }
                                        else
                                        {
                                            textCollections =AddNodeListToTextCollections(verseNodeList, project, textCollections);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error(ex, "There was an issue while parsing the USX for a text collection.  A text collection might not have been found.");
                                    }
                                }
                                else
                                {
                                    textCollections = UsfmToTextCollection(project, textCollection, textCollections);
                                }
                            }
                        }
                    }
                }
            }

            _textCollections = textCollections;

            return _textCollections;
        }


        private bool _inVerse = false;

        private List<XmlNode> CreateVerseNodeListRecursive(IProject project, XmlNode node, XmlDocument xDoc, bool isVerseByVerse, bool isCommentary)
        {
            List<XmlNode> verseNodeList = new();
            bool nextStartMarkerFound = false;
            var attributeTagName = string.Empty;

            XmlAttribute nodeSidRecursive;
            XmlAttribute nodeEidRecursive;
            string nodeSidValueRecursive = string.Empty;
            string nodeEidValueRecursive = string.Empty;

            if (node.Attributes != null)
            {
                nodeSidRecursive = node.Attributes["sid"];
                nodeEidRecursive = node.Attributes["eid"];

                if (nodeSidRecursive != null)
                {
                    nodeSidValueRecursive = nodeSidRecursive.Value;
                }
                if (nodeEidRecursive != null)
                {
                    nodeEidValueRecursive = nodeEidRecursive.Value;
                }
            }

            if (_inVerse && node.OuterXml.Contains("sid="))
            {
                _inVerse = false;
                nextStartMarkerFound = true;
            }

            if (nodeSidValueRecursive == _verseRef.ToString() || nodeEidValueRecursive == _verseRef.ToString())
            {
                _inVerse = true;
            }

            else if (nodeSidValueRecursive.Contains(_verseRef.BookCode + " " + _verseRef.ChapterNum + ":")  && nodeSidValueRecursive.Contains("-"))
            {
                attributeTagName = "sid";

            }
            else if (nodeEidValueRecursive.Contains(_verseRef.BookCode + " " + _verseRef.ChapterNum + ":")   && nodeEidValueRecursive.Contains("-"))
            {
                attributeTagName = "eid";
            }

            if (attributeTagName != string.Empty)
            {

                var nodeIdValue = node.Attributes[attributeTagName];

                RangedVerseCheck(project, xDoc, node, isVerseByVerse, isCommentary, nodeIdValue, attributeTagName);
            }
            if (_inVerse && !nextStartMarkerFound)
            {
                verseNodeList.Add(node);
            }

            if (!_inVerse)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    verseNodeList.AddRange(CreateVerseNodeListRecursive(project, child, xDoc, isVerseByVerse, isCommentary));
                }
            }

            return verseNodeList;
        }

        private List<XmlNode> CreateVerseNodeList(IProject project, XmlDocument xDoc, bool isVerseByVerse, bool isCommentary)
        {
            List<XmlNode> verseNodeList = new();
            bool nextStartMarkerFound = false;

            IEnumerable parentNode;
            parentNode = xDoc;

            foreach (XmlNode middlenode in parentNode)
            {
                foreach (XmlNode node in middlenode.ChildNodes)
                {
                    var attributeTagName = string.Empty;

                    if (_inVerse && node.OuterXml.Contains("sid="))
                    {
                        _inVerse = false;
                        nextStartMarkerFound = true;
                    }

                    if (node.OuterXml.Contains("sid=\"" + _verseRef + "\""))
                    {
                        _inVerse = true;

                        attributeTagName = "sid";

                        FindAndHighlightNode(project, xDoc, node, isVerseByVerse, isCommentary, attributeTagName);
                    }

                    if (node.OuterXml.Contains("eid=\"" + _verseRef + "\""))
                    {
                        _inVerse = true;

                        attributeTagName = "eid";

                        FindAndHighlightNode(project, xDoc, node, isVerseByVerse, isCommentary, attributeTagName);
                    }
                    
                    var nodeVerseElementList = node.SelectNodes("verse");
                    if (nodeVerseElementList.Count > 0)
                    {
                        foreach (XmlNode child in nodeVerseElementList)
                        {

                            if (child.OuterXml.Contains("sid=\"" + _verseRef.BookCode + " " + _verseRef.ChapterNum + ":") &&
                                child.OuterXml.Contains("-"))
                            {
                                attributeTagName = "sid";

                            }
                            else if (child.OuterXml.Contains("eid=\"" + _verseRef.BookCode + " " + _verseRef.ChapterNum + ":") &&
                                     child.OuterXml.Contains("-"))
                            {
                                attributeTagName = "eid";
                            }

                            var nodeIdValue = child.Attributes[attributeTagName];

                            RangedVerseCheck(project, xDoc, child, isVerseByVerse, isCommentary, nodeIdValue, attributeTagName);
                        }
                    }
                    else if (node.Name == "verse")
                    {
                        var nodeIdValue = node.Attributes[attributeTagName];

                        RangedVerseCheck(project, xDoc, node, isVerseByVerse, isCommentary, nodeIdValue, attributeTagName);
                    }
                    
                    if (_inVerse && !nextStartMarkerFound)
                    {
                        verseNodeList.Add(node);
                    }
                }
            }

            return verseNodeList;
        }

        private void RangedVerseCheck(IProject project, XmlDocument xDoc, XmlNode node, bool isVerseByVerse, bool isCommentary, XmlAttribute nodeIdValue, string attributeTagName)
        {
            if (nodeIdValue != null)
            {
                var nodeIdVerseNumber = nodeIdValue.Value.Split(':')[1];
                var idVerseNumberIsRange = nodeIdVerseNumber.Contains("-");

                if (idVerseNumberIsRange)
                {
                    var nodeIdVerseRange = nodeIdVerseNumber.Split('-');

                    var numberOnlyLowerId = Regex.Replace(nodeIdVerseRange[0], "[^0-9.]", "");
                    var numberOnlyUpperId = Regex.Replace(nodeIdVerseRange[1], "[^0-9.]", "");

                    int.TryParse(numberOnlyLowerId, out var lowerId);
                    int.TryParse(numberOnlyUpperId, out var upperId);

                    if (lowerId <=
                        _verseRef.VerseNum && _verseRef.VerseNum
                        <= upperId)
                    {
                        _inVerse = true;

                        if(!isVerseByVerse)
                            FindAndHighlightNode(project, xDoc, node, isVerseByVerse, isCommentary, attributeTagName);
                    }
                }
            }
        }

        private void FindAndHighlightNode(IProject project, XmlDocument xDoc, XmlNode node, bool isVerseByVerse, bool isCommentary, string attributeTagName)
        {
            if (!ProjectIsKnownCommentary(project))
            {
                if (node.ChildNodes.Count > 0)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        AddHighlightAttributeToNode(xDoc, child, isVerseByVerse, isCommentary, attributeTagName);
                    }
                }
                else
                {
                    AddHighlightAttributeToNode(xDoc, node, isVerseByVerse, isCommentary, attributeTagName);
                }
            }
        }

        private void AddHighlightAttributeToNode(XmlDocument xDoc, XmlNode node, bool isVerseByVerse, bool isCommentary, string attributeTagName)
        {
            if (node.LocalName == "verse" && node.Attributes[attributeTagName] != null &&
                (
                    node.Attributes[attributeTagName].Value == _verseRef.ToString() ||
                    (node.Attributes[attributeTagName].Value.Contains('-')
                     && node.Attributes[attributeTagName].Value.Contains(_verseRef.BookCode+" "+_verseRef.ChapterNum + ":"))
                )
               )
            {
                if (node.Attributes["style"] != null)
                {
                    node.Attributes["style"].Value="vh";
                }
                else
                {
                    XmlAttribute attr = xDoc.CreateAttribute("style");
                    attr.Value = "vh";

                    node.Attributes.Append(attr);
                }
            }
        }

        private List<TextCollection> AddNodeListToTextCollections(List<XmlNode> verseNodeList, IProject project, List<TextCollection> textCollections)
        {
            var usxString = "<usx version=\"3.0\">";
            foreach (XmlNode node in verseNodeList)
            {
                usxString += node.OuterXml;
            }

            usxString += "</usx>";

            textCollections.Add(new TextCollection()
            {
                ReferenceShort = project.ShortName,
                Data = usxString,
                Id = project.ID
        });

            return textCollections;
        }

        private bool ProjectIsKnownCommentary(IProject project)
        {
            switch (project.ShortName)
            {
                case "HBKENG":
                    return true;
                case "TND":
                    return true;
                case "TNN":
                    return true;
                case "NTBN":
                    return true;
                default:
                    return false;
            }
        }

        private List<TextCollection> UsfmToTextCollection(IProject project, TextCollection textCollection, List<TextCollection> textCollections)
        {
            IEnumerable<IUSFMToken> tokens = null;
            try
            {
                tokens = project.GetUSFMTokens(_verseRef.BookNum, _verseRef.ChapterNum, _verseRef.VerseNum);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, $"Cannot get USFM Tokens for {project.ShortName} : {e.Message}");
            }

            if (tokens != null)
            {
                textCollection.ReferenceShort = project.ShortName;
                textCollection.Id = project.ID;

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
                if (textCollection.Data != null && textCollection.Data.Length > 2)
                {
                    if (textCollection.Data.EndsWith("/ "))
                    {
                        textCollection.Data =
                            textCollection.Data.Substring(0, textCollection.Data.Length - 2);
                    }
                }

                textCollections.Add(textCollection);
            }
            return textCollections;
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
                // make sure that we actually have an active window in Paratext
                if (_host.ActiveWindowState is not null)
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
        }

        /// <summary>
        /// Clear the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearWindow_Click(object sender, EventArgs e)
        {
            ClearTextWindow();
            AppendText(Color.Green, DateTime.Now.ToShortTimeString());

            var ret = PortInUse(PortNumber);
            AppendText(Color.Green, $"Port {PortNumber} in use: {ret}");
        }

        private void ClearTextWindow()
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
            if (_projectMetadata == null)
            {
                bool fontError = false;
                var projects = _host.GetAllProjects(true);


                var metadata = projects.Select(project =>
                {
                    ScrVersType scrVersType = new ScrVersType();
                    switch (project.Versification.Type)
                    {
                        case StandardScrVersType.Unknown:
                            scrVersType = ScrVersType.Unknown;
                            break;
                        case StandardScrVersType.Original:
                            scrVersType = ScrVersType.Original;
                            break;
                        case StandardScrVersType.Septuagint:
                            scrVersType = ScrVersType.Septuagint;
                            break;
                        case StandardScrVersType.Vulgate:
                            scrVersType = ScrVersType.Vulgate;
                            break;
                        case StandardScrVersType.English:
                            scrVersType = ScrVersType.English;
                            break;
                        case StandardScrVersType.RussianProtestant:
                            scrVersType = ScrVersType.RussianProtestant;
                            break;
                        case StandardScrVersType.RussianOrthodox:
                            scrVersType = ScrVersType.RussianOrthodox;
                            break;
                        default:
                            scrVersType = ScrVersType.Unknown;
                            break;
                    }

                    var metaData = new ParatextProjectMetadata
                    {
                        Id = project.ID,
                        LanguageName = project.LanguageName,
                        LanguageId = project.Language.Id,
                        Name = project.ShortName,
                        LongName = project.LongName,
                        CorpusType = DetermineCorpusType(project.Type, project.IsResource),
                        IsRtl = project.Language.IsRtoL,
                        AvailableBooks = project.GetAvailableBooks(),
                        ScrVers = new ScrVers(scrVersType),
                    };

                    metaData.FontFamily = project.Language.Font.FontFamily;

                    try
                    {
                        // check to see if this font is installed locally
                        FontFamily family = new FontFamily(project.Language.Font.FontFamily);
                    }
                    catch (Exception e)
                    {
                        if (_hasFontErrorBeenDisplayed == false)
                        {
                            fontError = true;
                            AppendText(Color.PaleVioletRed, $"Project: {project.ShortName} FontFamily Warning: {e.Message} on this computer");
                        }

                        // use the default font
                        metaData.FontFamily = "Segoe UI";
                    }

                    return metaData;
                }).ToList();

                var projectNames = metadata.Select(project => project.Name).ToList();

                //foreach (var project in metadata)
                //{
                //    AppendText(Color.CadetBlue, $"Project: {project.Name} : Font Family: {project.FontFamily}");
                //}

                var directoryInfo = new DirectoryInfo(GetParatextProjectsPath());
                var directories = directoryInfo.GetDirectories();
                foreach (var directory in directories.Where(directory => projectNames.Contains(directory.Name)))
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
                _projectMetadata = metadata;

                if (fontError)
                {
                    _hasFontErrorBeenDisplayed = true;
                }
            }

            return _projectMetadata;
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
        /// <param name="isResource"></param>
        /// <returns></returns>
        private CorpusType DetermineCorpusType(ProjectType projectType, bool isResource)
        {
            try
            {
                if (isResource)
                {
                    return CorpusType.Resource;
                }
                else
                {
                    return (CorpusType)(projectType);
                }
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

            var paratextPath = GetParatextProjectsPath();

            projects.ForEach(p => allProjects.Add(BuildParatextProject(p, paratextPath)));

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
        /// <param name="paratextPath"></param>
        /// <returns></returns>
        private ParatextProject BuildParatextProject(IProject project, string paratextPath)
        {
            var paratextProject = new ParatextProject();

            paratextProject.ShortName = project.ShortName;
            paratextProject.Name = project.ShortName;
            paratextProject.Guid = project.ID;
            paratextProject.LongName = project.LongName;
            paratextProject.IsResource = project.IsResource;

            paratextProject.DirectoryPath = Path.Combine(paratextPath, project.ShortName);

            if (Directory.Exists(paratextProject.DirectoryPath) == false)
            {
                paratextProject.DirectoryPath = "";
            }

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
                versificationBookIds.BookAbbreviations = books
                    .Where(item => CheckUsfmBookForVerseData(project.ID, item.Code))
                    .Select(item => item.Code)
                    .ToList();

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

            var previousTokenWasCp = false;
            foreach (var token in tokens)
            {
                if (previousTokenWasCp)
                {
                    previousTokenWasCp= false;
                    continue;
                }

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

                        if (lastVerseZero)
                        {
                            var usfm = new UsfmVerse
                            {
                                Chapter = chapter,
                                Verse = "0",
                                Text = verseText
                            };
                            verses.Add(usfm);
                            verseText = "";
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

                    if (marker.Marker == "cp")
                    {
                        previousTokenWasCp = true;
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
        public bool CheckUsfmBookForVerseData(string paratextProjectId, string bookCode)
        {
            // get all the projects & resources
            var projects = _host.GetAllProjects(true);
            // get the right project
            var project = projects.FirstOrDefault(p => p.ID == paratextProjectId);

            var verses = new List<UsfmVerse>();
            if (project == null)
            {
                AppendText(Color.Orange, $"Could not find a project with Id = '{paratextProjectId}'. Returning an empty list.");
                return false;
            }

            // filter down to the book desired
            var book = project.AvailableBooks.FirstOrDefault(b => b.Code == bookCode);
            if (book == null)
            {
                AppendText(Color.Orange, $"Could not find a book with Id = '{bookCode}'. Returning an empty list.");
                return false;
            }

            // only return information for "bible books" and not the extra material
            // TODO - is this true??
            if (BibleBookScope.IsBibleBook(book.Code) == false)
            {
                AppendText(Color.Orange, $"'{book.Code}' is not a bible book. Returning an empty list.");
                return false;
            }



            IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
            try
            {
                // get tokens by book number (from object) and chapter
                tokens = project.GetUSFMTokens(book.Number);
            }
            catch (Exception)
            {
                AppendText(Color.Orange, $"No Scripture for {bookCode}");
                return false;
            }

            foreach (var token in tokens)
            {
                if (token is IUSFMTextToken textToken)
                {
                    if (token.IsScripture)
                    {
                        // verse text
                        if (textToken.Text.Trim() != "")
                        {
                            //AppendText(Color.Green, $"Processing {book.Code} TRUE");
                            return true;
                        }
                    }
                }

            }

            //AppendText(Color.PaleVioletRed, $"Processing {book.Code} FALSE");
            return false;
        }

        #endregion
        private void btnSwitchProject_Click(object sender, EventArgs e)
        {
            SwitchProjectWindow switchProjectWindow = new SwitchProjectWindow();
            switchProjectWindow.Show();
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
