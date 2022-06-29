using ClearDashboard.WebApiParatextPlugin.Extensions;
using ClearDashboard.WebApiParatextPlugin.Helpers;
using ClearDashboard.WebApiParatextPlugin.Hubs;
using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.Threading;
using Paratext.PluginInterfaces;
using Serilog;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;

namespace ClearDashboard.WebApiParatextPlugin
{
    public interface IPluginLogger
    {
        void AppendText(Color color, string message);
    }
    public partial class MainWindow : EmbeddedPluginControl, IPluginLogger
    {
        #region props

        private IProject _project;
        
        private IVerseRef _verseRef;
        private IWindowPluginHost _host;
        private IPluginChildWindow _parent;

        private IMediator _mediator;
        private IHubContext HubContext => GlobalHost.ConnectionManager.GetHubContext<PluginHub>();


        private WebHostStartup WebHostStartup { get; set; }

        private IDisposable WebAppProxy { get; set; }

        private delegate void AppendMsgTextDelegate(Color color, string text);

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
            // write to Serilog
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
        }

     

        public override string GetState()
        {
            // override required by base class, return null string.
            return null;
        }

       
        public override void DoLoad(IProgressInfo progressInfo)
        {
            StartWebHost();
           
        }

        private Assembly FailedAssemblyResolutionHandler(object sender, ResolveEventArgs args)
        {
            // Get just the name of assembly without version and other metadata
            var truncatedName = new Regex(",.*").Replace(args.Name, string.Empty);

            if (truncatedName == "System.XmlSerializers")
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
            if (newReference != _verseRef)
            {
                _verseRef = newReference;
             
                try
                {
                    await HubContext.Clients.All.SendVerse(_verseRef.BBBCCCVVV.ToString());

                }
                catch (Exception ex)
                {
                    AppendText(Color.Red, $"Unexpected error occurred calling PluginHub.SendVerse() : {ex.Message}");
                }
              
            }
        }

        #endregion Paratext overrides - standard functions


        #region Methods


      

        /// <summary>
        /// Send out the Biblical Terms for ALL BTs
        /// </summary>
        /// <returns></returns>
        private async Task GetBiblicalTermsAllBackgroundAsync()
        {
            //// ReSharper disable once InconsistentNaming
            //string payloadBTAll = "";

            //await Task.Run(() =>
            //{
            //    if (_termsList == null)
            //    {
            //        BibilicalTerms btAll = new BibilicalTerms(_allList, _project, _host);
            //        _termsList = btAll.ProcessBiblicalTerms(_project);
            //    }

            //    payloadBTAll = JsonSerializer.Serialize(_termsList, _jsonOptions);

            //});

          
        }

        /// <summary>
        /// Send out the Biblical Terms for only the Project BTs
        /// </summary>
        /// <returns></returns>
        private async Task GetBiblicalTermsProjectBackgroundAsync()
        {
            //// ReSharper disable once InconsistentNaming
            //string payloadBT = "";

            //BibilicalTerms bt = new BibilicalTerms(_projectList, _project, _host);

            //await Task.Run(() =>
            //{
            //    var btList = bt.ProcessBiblicalTerms(_project);
            //    payloadBT = JsonSerializer.Serialize(btList, _jsonOptions);
            //});

          
        }

        /// <summary>
        /// Retrieves the USX & USFM for the project passing in the current book number
        /// </summary>
        /// <returns></returns>
        private async Task GetUSXScriptureAsync()
        {
            //await Task.Run(async () =>
            //{
            //    var usx = _project.GetUSX(_verseRef.BookNum);
            //    if (usx != null)
            //    {
            //        var dataPayload = JsonSerializer.Serialize(usx);

            //        await WriteMessageToPipeAsync(new PipeMessage
            //        {
            //            Action = ActionType.SetUSX,
            //            Text = $"BOOK: {_verseRef.BookNum}",
            //            Payload = dataPayload,
            //        }).ConfigureAwait(false);
            //    }
            //});
            //AppendText(MsgColor.Orange, "OUTBOUND -> SetUSX");

            ////var usfm = m_project.GetUSFM(m_verseRef.BookNum);
            ////if (usfm != null)
            ////{
            ////    var dataPayload = JsonSerializer.Serialize(usfm);


        }

        ///// <summary>
        ///// Build up a project object to send over.  This is only a small portion
        ///// of what is available in the m_project object
        ///// </summary>
        ///// <returns></returns>
        //private Project BuildProjectObject()
        //{
        //    Project project = new Project();
        //    project.ID = _project.ID;
        //    project.LanguageName = _project.LanguageName;
        //    project.ShortName = _project.ShortName;
        //    project.LongName = _project.LongName;
        //    foreach (var users in _project.NonObserverUsers)
        //    {
        //        project.NonObservers.Add(users.Name);
        //    }

        //    foreach (var book in _project.AvailableBooks)
        //    {
        //        project.AvailableBooks.Add(new BookInfo
        //        {
        //            Code = book.Code,
        //            InProjectScope = book.InProjectScope,
        //            Number = book.Number,
        //        });
        //    }

        //    project.Language = new ScrLanguageWrapper
        //    {
        //        FontFamily = _project.Language.Font.FontFamily,
        //        Size = _project.Language.Font.Size,
        //        IsRtol = _project.Language.IsRtoL,
        //    };

        //    switch (_project.Type)
        //    {
        //        case Paratext.PluginInterfaces.ProjectType.Standard:
        //            project.Type = Project.ProjectType.Standard;
        //            break;
        //        default:
        //            project.Type = Project.ProjectType.NotSelected;
        //            break;
        //    }

        //    project.BCVDictionary = GetBCV_Dictionary();

        //    return project;
        //}

        //private Dictionary<string, string> GetBCV_Dictionary()
        //{
        //    Dictionary<string, string> bcvDict = new Dictionary<string, string>();

        //    // loop through all the bible books capturing the BBCCCVVV for every verse
        //    for (int bookNum = 0; bookNum < _project.AvailableBooks.Count; bookNum++)
        //    {
        //        if (BibleBookScope.IsBibleBook(_project.AvailableBooks[bookNum].Code))
        //        {
        //            IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
        //            try
        //            {
        //                // get tokens by book number (from object) and chapter
        //                tokens = _project.GetUSFMTokens(_project.AvailableBooks[bookNum].Number);
        //            }
        //            catch (Exception)
        //            {
        //               AppendText(MainWindow.MsgColor.Orange, $"No Scripture for {bookNum}");
        //            }

        //            foreach (var token in tokens)
        //            {
        //                if (token is IUSFMMarkerToken marker)
        //                {
        //                    // a verse token
        //                    if (marker.Type == MarkerType.Verse)
        //                    {
        //                        string verseID = marker.VerseRef.BBBCCCVVV.ToString().PadLeft(8,'0');
        //                        if (! bcvDict.ContainsKey(verseID))
        //                        {
        //                            bcvDict.Add(verseID, verseID);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return bcvDict;
        //}

        // /// <summary>
        // /// Does a registry check to ensure that ClearSuite is reachable
        // /// </summary>
        // /// <returns></returns>
        //public bool CheckIfClearSuiteInstalledAsync()
        //{
        //    //Here we peek into the registry to see if they even have clear engine controller installed
        //    var clearEngineControllerPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Clear\ClearSuite", "Path", null);

        //    if (Directory.Exists(clearEngineControllerPath))
        //    {
        //        // file doesn't exist so null this out
        //        if (File.Exists(Path.Combine(clearEngineControllerPath, "ClearSuite.Wpf.exe")))
        //        {
        //            _clearSuitePath = Path.Combine(clearEngineControllerPath, "ClearSuite.Wpf.exe");
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        //private async Task GetNoteListAsync(PipeMessage message)
        //{
        //    var bookNum = 0;
        //    var chapNum = 0;

        //    var data = JsonSerializer.Deserialize<GetNotesData>((string)message.Text);
        //    //var data = JsonConvert.DeserializeObject<GetNotesData>(jsonPayload);
        //    AppendText(MsgColor.Blue, $"GetNotesListAsync received: Book: {data.BookID}, Chapter: {data.ChapterID}, IncludeResolved: {data.IncludeResolved}");

        //    if (data.BookID >= 0 && data.BookID <= 66 && data.ChapterID > 0)
        //    {
        //        _bookNumber = _project.AvailableBooks[data.BookID].Number;
        //        int chapter = data.ChapterID;
        //        // include resolved notes
        //        bool onlyUnresolved = !data.IncludeResolved;
        //        _noteList = _project.GetNotes(_bookNumber, chapter, onlyUnresolved);
        //        AppendText(MsgColor.Green, $"Book Num: {_bookNumber} / {chapter}: {_noteList.Count.ToString()}");


        //        var dataPayload = JsonSerializer.Serialize(_noteList, new JsonSerializerOptions
        //                                                                        {
        //                                                                            WriteIndented = true,
        //                                                                            MaxDepth = 1024 * 100000
        //                                                                        });


        //        try
        //        {
        //            var deserializedObj = JsonSerializer.Deserialize<IReadOnlyList<IProjectNote>>(dataPayload);

        //            PipeMessage msgOut = new PipeMessage
        //            {
        //                Action = ActionType.SetNotesObject,
        //                Text = "Notes Object",
        //                Payload = dataPayload
        //            };

        //            await WriteMessageToPipeAsync(msgOut).ConfigureAwait(false);
        //        }
        //        catch (Exception e)
        //        {
        //            AppendText(MsgColor.Red, e.Message);
        //        }
        //    }
        //}


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

        /// <summary>
        /// Force a restart of the named pipes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRestart_Click(object sender, EventArgs e)
        {
            //// disconnect the pipe
            //UnhookPipe();

            //await Task.Run(() => Task.Delay(500)).ConfigureAwait(false);

            //// reconnect the pipe
            //_serverPipe = new ServerPipe("ClearDashboardPlugin", p => p.StartStringReaderAsync());
            //_serverPipe.DataReceived += ServerPipeOnDataReceived;

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
            //var hubProxy = (IHubContext<PluginHub>)GlobalHost.DependencyResolver.GetService(typeof(IHubContext<PluginHub>)); //.GetService<IHubContext<PluginHub>>();
            var hubProxy = GlobalHost.ConnectionManager.GetHubContext<PluginHub>();
            if (hubProxy == null)
            {
                AppendText(Color.Red, "HubContext is null");
                return;
            }

            hubProxy.Clients.All.Send(Guid.NewGuid(), @"Can you hear me?");
            //GetNotesData data = new GetNotesData();
            //data.BookID = 5;
            //data.ChapterID = 1;
            //data.IncludeResolved = true;
            //var dataPayload = JsonConvert.SerializeObject(data, Formatting.None,
            //    new JsonSerializerSettings()
            //    {
            //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //    });

            //_ = GetNoteList("", dataPayload);
        }

        private void btnVersificationTest_Click(object sender, EventArgs e)
        {
            var v = _project.Versification;

            IVersification versification;
            IVerseRef verseRef;


            var newVerse = v.CreateReference(19, 20, 1);

            newVerse = v.ChangeVersification(newVerse);
        }


        //private async Task SetUSFMScripture()
        //{
        //    IEnumerable<IUSFMToken> tokens = m_project.GetUSFMTokens(m_verseRef.BookNum, m_verseRef.ChapterNum);
        //    List<string> lines = new List<string>();
        //    foreach (var token in tokens)
        //    {
        //        if (token is IUSFMMarkerToken marker)
        //        {
        //            switch (marker.Type)
        //            {
        //                case MarkerType.Verse:
        //                    lines.Add($"v {marker.Data}");
        //                    break;
        //                case MarkerType.Book:
        //                    lines.Add($"{marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.Chapter:
        //                    lines.Add($"{marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.Character:
        //                    lines.Add($"Marker Character: {marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.Milestone:
        //                    lines.Add($"Marker Milestone: {marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.MilestoneEnd:
        //                    lines.Add($"Marker MilestoneEnd: {marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.End:
        //                    lines.Add($"Marker End: {marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.Note:
        //                    lines.Add($"Marker Note: {marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.Unknown:
        //                    lines.Add($"Marker Unknown: {marker.Marker} {marker.Data}");
        //                    break;
        //                case MarkerType.Paragraph:
        //                    lines.Add($" {marker.Marker} {marker.Data}");
        //                    break;
        //            }
        //        }
        //        else if (token is IUSFMTextToken textToken)
        //        {
        //            if (lines.Count > 0)
        //            {
        //                lines[lines.Count - 1] += (textToken.Text);
        //            }
        //            else
        //            {
        //                lines.Add(textToken.Text);
        //            }
        //        }
        //        else if (token is IUSFMAttributeToken)
        //        {
        //            lines.Add("Attribute Token: " + token.ToString());
        //        }
        //        else
        //        {
        //            lines.Add("Unexpected token type: " + token.ToString());
        //        }
        //    }

        //    var dataPayload = JsonSerializer.Serialize(lines);

        //    PipeMessage msgOut = new PipeMessage
        //    {
        //        Action = ActionType.SetUSX,
        //        Text = $"{m_verseRef.BookNum}",
        //        Payload = dataPayload
        //    };

        //    await WriteMessageToPipeAsync(msgOut).ConfigureAwait(false);
        //    AppendText(MsgColor.Orange, $"OUTBOUND -> SetUSX: {m_verseRef.BookNum}");
        //}

        #endregion
    }
}
