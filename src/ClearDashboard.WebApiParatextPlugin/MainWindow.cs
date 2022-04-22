using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Threading;
using Paratext.PluginInterfaces;
using Serilog;
using ClearDashboard.WebApiParatextPlugin;
using ClearDashboard.WebApiParatextPlugin.Helpers;
using Microsoft.Owin.Hosting;
using System.Net.Http;


namespace ClearDashboard.WebApiParatextPlugin
{
    public partial class MainWindow : EmbeddedPluginControl
    {
        #region props

        private IProject _project;
        private int _bookNumber;
        private IVerseRef _verseRef;
        private IReadOnlyList<IProjectNote> _noteList;
        private IWindowPluginHost _host;
        private IPluginChildWindow _parent;


        //private List<BiblicalTermsData> _termsList;
        //private readonly ListType _projectList = new ListType("Project", true, BiblicalTermListType.All);
        //private readonly ListType _allList = new ListType("All", false, BiblicalTermListType.All);
        //private readonly ListType _majorList = new ListType("Major", false, BiblicalTermListType.Major);

        private readonly IBiblicalTermList _listProject;
        // ReSharper disable once InconsistentNaming
        private readonly IBiblicalTermList _listAll;

        private readonly JsonSerializerOptions _jsonOptions;
        private delegate void AppendMsgTextDelegate(MsgColor color, string text);
        private Form _parentForm;


        // Named Pipe Props
        //private const string PipeName = "ClearDashboard";
        //private PipeServer<PipeMessage> _PipeServer { get; }
        //private ISet<string> Clients { get; } = new HashSet<string>();


        public enum MsgColor
        {
            Red,
            Green,
            Blue,
            Orange,
            Purple,
        }

        #endregion

        #region startup

        public MainWindow()
        {
            InitializeComponent();

            // configure Serilog
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File("Plugin.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // set instance to global logger
            Log.Logger = log;


            // get the version information
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format($@"Plugin Version: {version}");

            // hook up an event when the window is closed so we can kill off the pipe
            this.Disposed += MainWindow_Disposed;

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
            };

           

            AppendText(MsgColor.Green, "Owin Web Api host started");



        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        #endregion

        #region pipes section

       
        /// <summary>
        /// this window is closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void MainWindow_Disposed(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
       
        }

    


        private void OnExceptionOccurred(Exception exception)
        {
            // write to Serilog
            Log.Error($"OnLoad {exception.Message}");
            AppendText(MsgColor.Red, $"OnLoad {exception.Message}");
        }

    

        #endregion

        #region Paratext overrides - standard functions

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (this.ParentForm != null)
            {
                // get a reference to the Paratext calling form
                _parentForm = this.ParentForm;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.ParentForm != null)
            {
                // get a reference to the Paratext calling form
                _parentForm = this.ParentForm;
            }

        }

        public override void OnAddedToParent(IPluginChildWindow parent, IWindowPluginHost host, string state)
        {
            parent.SetTitle(ClearDashboardWebApiPlugin.PluginName);
            parent.ProjectChanged += ProjectChanged;
            parent.VerseRefChanged += VerseRefChanged;

            SetProject(parent.CurrentState.Project);
            _verseRef = parent.CurrentState.VerseRef;

            _host = host;
            _project = parent.CurrentState.Project;
            _parent = parent;

            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            var webApp = WebApp.Start(baseAddress,
                (appBuilder) =>
                {
                    var startup = new Startup(_project,_verseRef);
                    startup.Configuration(appBuilder);
                });
        }



        /// <summary>
        /// Gets the current state of the control, from which the control must know how to
        /// restore itself EmbeddedPluginControl.OnAddedToParent(Paratext.PluginInterfaces.IPluginChildWindow,Paratext.PluginInterfaces.IWindowPluginHost,System.String) is called).
        /// </summary>
		public override string GetState()
        {
            return null;
        }

        /// <summary>
        /// Will always be called after
        /// EmbeddedPluginControl.OnAddedToParent(Paratext.PluginInterfaces.IPluginChildWindow,Paratext.PluginInterfaces.IWindowPluginHost,System.String) on a separate thread from the main UI thread.
        /// Long-running tasks needed for loading should be done here.
        /// The control's content will not be visible until this method returns.
        /// </summary>
		public override void DoLoad(IProgressInfo progressInfo)
        {

        }


        private void ProjectChanged(IPluginChildWindow sender, IProject newProject)
        {
            SetProject(newProject);
        }

        private void SetProject(IProject newProject)
        {
            _project = newProject;
        }

        /// <summary>
        /// Paratext has a verse change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="oldReference"></param>
        /// <param name="newReference"></param>
        private void VerseRefChanged(IPluginChildWindow sender, IVerseRef oldReference, IVerseRef newReference)
        {
            if (newReference != _verseRef)
            {
                _verseRef = newReference;
                GetCurrentVerseAsync().Forget();
            }
        }

        #endregion Paratext overrides - standard functions


        #region Methods


        /// <summary>
        /// Send out the current verse through the pipe
        /// </summary>
        /// <returns></returns>
        private async Task GetCurrentVerseAsync()
        {
            //string verseId = _verseRef.BBBCCCVVV.ToString();
            //if (verseId.Length < 8)
            //{
            //    verseId = verseId.PadLeft(8, '0');
            //}

            //await WriteMessageToPipeAsync(new PipeMessage
            //{
            //    Action = ActionType.CurrentVerse,
            //    Text = verseId,
            //}).ConfigureAwait(false);
            //AppendText(MsgColor.Orange, $"OUTBOUND -> Sent Current Verse: {_verseRef}");
        }

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

            //await WriteMessageToPipeAsync(new PipeMessage
            //{
            //    Action = ActionType.SetBiblicalTerms,
            //    Text = "Project Object",
            //    Payload = payloadBTAll
            //}).ConfigureAwait(false);
            //AppendText(MsgColor.Orange, "OUTBOUND -> SetBiblicalTermsAll");
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

            //await WriteMessageToPipeAsync(new PipeMessage
            //{
            //    Action = ActionType.SetBiblicalTerms,
            //    Text = "Project Object",
            //    Payload = payloadBT
            //}).ConfigureAwait(false);
            //AppendText(MsgColor.Orange, "OUTBOUND -> SetBiblicalTermsProject");
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

            ////    await WriteMessageToPipeAsync(new PipeMessage
            ////    {
            ////        Action = ActionType.SetUSFM,
            ////        Text = "Set USFM",
            ////        Payload = dataPayload,
            ////    }).ConfigureAwait(false);
            ////}
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

        /// <summary>
        /// Does a registry check to ensure that ClearSuite is reachable
        /// </summary>
        /// <returns></returns>
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
        /// <param name="sMsg"></param>
        /// <param name="color"></param>
        public void AppendText(MsgColor color, string sMsg)
        {
            //check for threading issues
            if (this.InvokeRequired)
            {
                this.Invoke(new AppendMsgTextDelegate(AppendText), new object[] { color, sMsg });
            }
            else
            {
                sMsg += $"{Environment.NewLine}";
                switch (color)
                {
                    case MsgColor.Blue:
                        ColoredRichTextBox.AppendText(sMsg, Color.Blue, this.rtb);
                        break;
                    case MsgColor.Red:
                        ColoredRichTextBox.AppendText(sMsg, Color.Red, this.rtb);
                        break;
                    case MsgColor.Green:
                        ColoredRichTextBox.AppendText(sMsg, Color.Green, this.rtb);
                        break;
                    case MsgColor.Orange:
                        ColoredRichTextBox.AppendText(sMsg, Color.Orange, this.rtb);
                        break;
                    case MsgColor.Purple:
                        ColoredRichTextBox.AppendText(sMsg, Color.Purple, this.rtb);
                        break;
                }

                // write to Serilog
                Log.Information(sMsg);
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

            AppendText(MsgColor.Green, DateTime.Now.ToShortTimeString());
            AppendText(MsgColor.Green, "_PipeServer Pipe Restarted");

        }

        private void btnTest_Click(object sender, EventArgs e)
        {
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
