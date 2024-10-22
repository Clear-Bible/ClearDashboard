﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClearDashboard.ParatextPlugin.Actions;
using ClearDashboard.ParatextPlugin.Data;
using ClearDashboard.ParatextPlugin.Data.Models;
using ClearDashboard.ParatextPlugin.Extensions;
using ClearDashboard.ParatextPlugin.Helpers;
using ClearDashboard.ParatextPlugin.Models;
using H.Formatters;
using H.Pipes;
using Microsoft.VisualStudio.Threading;
using Paratext.PluginInterfaces;
using Serilog;

namespace ClearDashboard.ParatextPlugin
{
    public partial class MainWindow : EmbeddedPluginControl
    {
        #region props

        // ReSharper disable once InconsistentNaming
        private IProject m_project;
        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        private int m_booknum;
        // ReSharper disable once InconsistentNaming
        private IVerseRef m_verseRef;
        // ReSharper disable once InconsistentNaming
        private IReadOnlyList<IProjectNote> m_noteList;

        // ReSharper disable once InconsistentNaming
        private IWindowPluginHost m_host;
        private List<BiblicalTermsData> _termsList;
        // ReSharper disable once NotAccessedField.Local
        // ReSharper disable once InconsistentNaming
        IPluginChildWindow m_parent;

        private readonly ListType _projectList = new ListType("Project", true, BiblicalTermListType.All);
        private readonly ListType _allList = new ListType("All", false, BiblicalTermListType.All);
        private readonly ListType _majorList = new ListType("Major", false, BiblicalTermListType.Major);

        // ReSharper disable once InconsistentNaming
        private readonly IBiblicalTermList m_listProject;
        // ReSharper disable once InconsistentNaming
        private readonly IBiblicalTermList m_listAll;

        private readonly JsonSerializerOptions _jsonOptions;
        private delegate void AppendMsgTextDelegate(Color color, string text);
        private Form _parentForm;


        // Named Pipe Props
        private const string PipeName = "ClearDashboard";
        private PipeServer<PipeMessage> _PipeServer { get; }
        private ISet<string> Clients { get; } = new HashSet<string>();

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


            //// check to see if ClearSuite is installed
            //             bool showErrorMessage = true;
            //if (CheckIfClearSuiteInstalledAsync())
            //{
            //    if (_clearSuitePath != string.Empty)
            //    {
            //        // launch ClearEngineController
            //        try
            //        {
            //            //using (Process proc = new Process())
            //            //{
            //            //    proc.StartInfo.FileName = _clearSuitePath;
            //            //    proc.StartInfo.UseShellExecute = true;
            //            //    proc.Start();
            //            //}

            //            showErrorMessage = false;
            //        }
            //        catch (Exception)
            //        {
            //            showErrorMessage = true;
            //            AppendText(MsgColor.Red, "ClearSuite Not Detected ERROR");
            //        }
            //    }
            //}


            //if (showErrorMessage)
            //{
            //    // TODO Do some alert now that ClearDashboard is NOT installed
            //}


            Load += OnLoad;

            // use a customized formatter for receiving classes so that the pipes knows
            // how to decode the payload
            var formatter = new BinaryFormatter();
            formatter.InternalFormatter.Binder = new CustomizedBinder();
            _PipeServer = new PipeServer<PipeMessage>(PipeName, formatter: formatter);

            _ = new PipeMessage();  // needed to initialize the class otherwise we get weird errors

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            _PipeServer.ClientConnected += async (o, args) =>
            {
                Clients.Add(args.Connection.PipeName);
                UpdateClientList();

                AppendText(Color.Green, $"{args.Connection.PipeName} connected!");

                try
                {
                    // send notice to client that we are connected
                    AppendText(Color.Green, "Sending OnConnected Message to Client");
                    await args.Connection.WriteAsync(new PipeMessage
                    {
                        Action = ActionType.OnConnected,
                        Text = "Connected to Paratext",
                        Payload = null
                    }).ConfigureAwait(false);


                    // send the current verse
                    await OnMessageReceivedAsync(new PipeMessage { Action = ActionType.OnConnected }).ConfigureAwait(false);

                }
                catch (Exception exception)
                {
                    OnExceptionOccurred(exception);
                }
            };

#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            _PipeServer.ClientDisconnected += (o, args) =>
            {
                Clients.Remove(args.Connection.PipeName);
                UpdateClientList();

                AppendText(Color.Green, $"{args.Connection.PipeName} disconnected!");
            };

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            _PipeServer.MessageReceived += async (sender, args) =>
            {
                if (args.Message != null)
                {
                    await OnMessageReceivedAsync(args.Message).ConfigureAwait(false);
                }
            };


#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            _PipeServer.ExceptionOccurred += (o, args) =>
            {
                OnExceptionOccurred(args.Exception);
            };
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        #endregion

        #region pipes section

        /// <summary>
        /// A custom serialization binder to make sure that we are processing the correct
        /// Pipes_Shared library.
        ///
        /// Note: MUST HAVE THE CORRECT VERSION NUMBER IN THE STRING BELOW
        /// </summary>
        sealed class CustomizedBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type returntype = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
                return returntype;
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
                assemblyName = "ClearDashboard.ParatextPlugin.Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            }
        }

        /// <summary>
        /// this window is closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void MainWindow_Disposed(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            // this user control is closing - clean up pipe

            try
            {
                await _PipeServer.WriteAsync(new PipeMessage
                {
                    Action = ActionType.OnDisconnected,
                    Text = "Connection Closed",
                    Payload = null,
                }).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }

            // we need a delay before closing to ensure that the message is received on the client
            await Task.Delay(500).ConfigureAwait(true);

            // unhook the pipe
            await _PipeServer.DisposeAsync().ConfigureAwait(false);
        }

        private async Task OnMessageReceivedAsync(PipeMessage message)
        {
            AppendText(Color.Purple, "INBOUND <- " + message.Action.ToString());

            // Do the command's action
            switch (message.Action)
            {
                case ActionType.SendText:
                {
                    AppendText(Color.Purple, "INBOUND <- " + message.Action.ToString() + ": " + message.Text);
                    break;
                }

                case ActionType.GetCurrentVerse:
                {
                    // send the current BCV location of Paratext
                    GetCurrentVerseAsync().Forget();

                    break;
                }

                case ActionType.GetBibilicalTermsAll:
                {
                    // fire off into background
                    GetBiblicalTermsAllBackgroundAsync().Forget();
                    break;
                }

                case ActionType.GetBibilicalTermsProject:
                {
                    // fire off into background
                    GetBiblicalTermsProjectBackgroundAsync().Forget();
                    break;
                }

                case ActionType.GetTargetVerses:
                {
                    // fire off into background
                    GetUSXScriptureAsync().Forget();
                    break;
                }

                case ActionType.GetNotes:
                {
                    await GetNoteListAsync(message).ConfigureAwait(false);
                    break;
                }

                case ActionType.GetUSFM:
                {
                    //await ShowUSFMScripture().ConfigureAwait(false);
                    break;
                }

                case ActionType.GetUSX:
                {
                    AppendText(Color.Purple, "INBOUND <- " + message.Action.ToString());
                    await GetUSXScriptureAsync().ConfigureAwait(false);
                    break;
                }

                case ActionType.OnConnected:
                {
                    AppendText(Color.Green, "ClearDashboard Connected");

                    // send the current BCV location of Paratext
                    GetCurrentVerseAsync().Forget();

                    // get the paratext project info and send that over
                    Project proj = BuildProjectObject();
                    var payload = JsonSerializer.Serialize(proj, _jsonOptions);

                    AppendText(Color.Orange, "OUTBOUND -> Sending Project Information");
                    await WriteMessageToPipeAsync(new PipeMessage
                    {
                        Action = ActionType.SetProject,
                        Text = "Project Object",
                        Payload = payload
                    });
                    AppendText(Color.Orange, $"OUTBOUND -> Project Sent: {m_project.LongName}");
                    break;
                }

                case ActionType.OnDisconnected:
                {
                    AppendText(Color.Orange, "ClearDashboard DisConnected");

                    await Task.Delay(1000).ConfigureAwait(true);

                    //btnRestart_Click(null, null);
                    break;
                }
                default:
                {
                    AppendText(Color.Red, $"Method {message.Action} not implemented");
                    break;
                }
            }
        }

        private async Task WriteMessageToPipeAsync(PipeMessage msgOut)
        {
            try
            {
                await _PipeServer.WriteAsync(msgOut).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void OnLoad(object sender, EventArgs eventArgs)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            try
            {
                AppendText(Color.Green, "PipeServer starting...");
                _ = new PipeMessage();
                await _PipeServer.StartAsync().ConfigureAwait(false);
                AppendText(Color.Green, "PipeServer is started!");
            }
            catch (Exception exception)
            {
                AppendText(Color.Red, $"OnLoad {exception.Message}");
                OnExceptionOccurred(exception);
            }
        }

        private void OnExceptionOccurred(Exception exception)
        {
            // write to Serilog
            Log.Error($"OnLoad {exception.Message}");
            AppendText(Color.Red, $"OnLoad {exception.Message}");
        }

        private void UpdateClientList()
        {
            listBoxClients.Invoke(new Action(() =>
            {
                listBoxClients.Items.Clear();
                foreach (var client in Clients)
                {
                    listBoxClients.Items.Add(client);
                }
            }));
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
            parent.SetTitle(ClearDashboard.ParatextPlugin.ClearDashboardPlugin.pluginName);
            var startupPath = Application.StartupPath;
            string iconPath = Path.Combine(startupPath, @"plugins\ClearDashboardPlugin\icon.ico");
            if (File.Exists(iconPath))
            {
                Icon icon = new Icon(iconPath);
                try
                {
                    parent.Icon = icon;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            
            parent.ProjectChanged += ProjectChanged;
            parent.VerseRefChanged += VerseRefChanged;
            parent.WindowClosing += Parent_WindowClosing;

            SetProject(parent.CurrentState.Project);
            m_verseRef = parent.CurrentState.VerseRef;

            m_host = host;
            m_project = parent.CurrentState.Project;
            m_parent = parent;
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
            m_project = newProject;
        }

        /// <summary>
        /// Paratext has a verse change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="oldReference"></param>
        /// <param name="newReference"></param>
        private void VerseRefChanged(IPluginChildWindow sender, IVerseRef oldReference, IVerseRef newReference)
        {
            if (newReference != m_verseRef)
            {
                m_verseRef = newReference;
                GetCurrentVerseAsync().Forget();
            }
        }

        private void Parent_WindowClosing(IPluginChildWindow sender, System.ComponentModel.CancelEventArgs args)
        {

        }

        #endregion Paratext overrides - standard functions


        #region Methods


        /// <summary>
        /// Send out the current verse through the pipe
        /// </summary>
        /// <returns></returns>
        private async Task GetCurrentVerseAsync()
        {
            string verseId = m_verseRef.BBBCCCVVV.ToString();
            if (verseId.Length < 8)
            {
                verseId = verseId.PadLeft(8, '0');
            }

            await WriteMessageToPipeAsync(new PipeMessage
            {
                Action = ActionType.CurrentVerse,
                Text = verseId,
            }).ConfigureAwait(false);
            AppendText(Color.Orange, $"OUTBOUND -> Sent Current Verse: {m_verseRef}");
        }

        /// <summary>
        /// Send out the Biblical Terms for ALL BTs
        /// </summary>
        /// <returns></returns>
        private async Task GetBiblicalTermsAllBackgroundAsync()
        {
            // ReSharper disable once InconsistentNaming
            string payloadBTAll = "";

            await Task.Run(() =>
            {
                if (_termsList == null)
                {
                    BibilicalTerms btAll = new BibilicalTerms(_allList, m_project, m_host);
                    _termsList = btAll.ProcessBiblicalTerms(m_project);
                }

                payloadBTAll = JsonSerializer.Serialize(_termsList, _jsonOptions);

            });

            await WriteMessageToPipeAsync(new PipeMessage
            {
                Action = ActionType.SetBiblicalTerms,
                Text = "Project Object",
                Payload = payloadBTAll
            }).ConfigureAwait(false);
            AppendText(Color.Orange, "OUTBOUND -> SetBiblicalTermsAll");
        }

        /// <summary>
        /// Send out the Biblical Terms for only the Project BTs
        /// </summary>
        /// <returns></returns>
        private async Task GetBiblicalTermsProjectBackgroundAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                // ReSharper disable once InconsistentNaming
                string payloadBT = "";

                BibilicalTerms bt = new BibilicalTerms(_projectList, m_project, m_host);

                AppendText(Color.Orange, "EXECUTING -> GetBiblicalTermsProjectBackgroundAsync");
                await Task.Run(() =>
                {
                    var btList = bt.ProcessBiblicalTerms(m_project);
                    payloadBT = JsonSerializer.Serialize(btList, _jsonOptions);
                });

                await WriteMessageToPipeAsync(new PipeMessage
                {
                    Action = ActionType.SetBiblicalTerms,
                    Text = "Project Object",
                    Payload = payloadBT
                }).ConfigureAwait(false);
                AppendText(Color.Orange, "OUTBOUND -> SetBiblicalTermsProject");
            }
            catch (Exception ex)
            {
                AppendText(Color.Red, ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                AppendText(Color.Orange, $"ELAPSED TIME -> SetBiblicalTermsProject : {stopwatch.ElapsedMilliseconds} milliseconds");

            }
          
        }

        /// <summary>
        /// Retrieves the USX & USFM for the project passing in the current book number
        /// </summary>
        /// <returns></returns>
        private async Task GetUSXScriptureAsync()
        {
            await Task.Run(async () =>
            {
                //m_project.GetUSFM()
                var usx = m_project.GetUSX(m_verseRef.BookNum);
                if (usx != null)
                {
                    var dataPayload = JsonSerializer.Serialize(usx);

                    await WriteMessageToPipeAsync(new PipeMessage
                    {
                        Action = ActionType.SetUSX,
                        Text = $"BOOK: {m_verseRef.BookNum}",
                        Payload = dataPayload,
                    }).ConfigureAwait(false);
                }
            });
            AppendText(Color.Orange, "OUTBOUND -> SetUSX");

            //var usfm = m_project.GetUSFM(m_verseRef.BookNum);
            //if (usfm != null)
            //{
            //    var dataPayload = JsonSerializer.Serialize(usfm);

            //    await WriteMessageToPipeAsync(new PipeMessage
            //    {
            //        Action = ActionType.SetUSFM,
            //        Text = "Set USFM",
            //        Payload = dataPayload,
            //    }).ConfigureAwait(false);
            //}
        }

        /// <summary>
        /// Build up a project object to send over.  This is only a small portion
        /// of what is available in the m_project object
        /// </summary>
        /// <returns></returns>
        private Project BuildProjectObject()
        {
            Project project = new Project();
            project.Id = m_project.ID;
            project.LanguageName = m_project.LanguageName;
            project.ShortName = m_project.ShortName;
            project.LongName = m_project.LongName;
            foreach (var users in m_project.NonObserverUsers)
            {
                project.NonObservers.Add(users.Name);
            }

            foreach (var book in m_project.AvailableBooks)
            {
                project.AvailableBooks.Add(new BookInfo
                {
                    Code = book.Code,
                    InProjectScope = book.InProjectScope,
                    Number = book.Number,
                });
            }

            project.Language = new ScrLanguageWrapper
            {
                FontFamily = m_project.Language.Font.FontFamily,
                Size = m_project.Language.Font.Size,
                IsRtol = m_project.Language.IsRtoL,
            };

            switch (m_project.Type)
            {
                case Paratext.PluginInterfaces.ProjectType.Standard:
                    project.Type = Project.ProjectType.Standard;
                    break;
                default:
                    project.Type = Project.ProjectType.NotSelected;
                    break;
            }

            project.BcvDictionary = GetBCV_Dictionary();

            return project;
        }


        /// <summary>
        /// Generate a dictionary of all the unique verse IDs in the project
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetBCV_Dictionary()
        {
            Dictionary<string, string> bcvDict = new Dictionary<string, string>();

            // loop through all the bible books capturing the BBCCCVVV for every verse
            for (int bookNum = 0; bookNum < m_project.AvailableBooks.Count; bookNum++)
            {
                if (BibleBookScope.IsBibleBook(m_project.AvailableBooks[bookNum].Code))
                {
                    IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
                    try
                    {
                        // get tokens by book number (from object) and chapter
                        tokens = m_project.GetUSFMTokens(m_project.AvailableBooks[bookNum].Number);
                    }
                    catch (Exception)
                    {
                       AppendText(Color.Orange, $"No Scripture for {bookNum}");
                    }

                    foreach (var token in tokens)
                    {
                        if (token is IUSFMMarkerToken marker)
                        {
                            // a verse token
                            if (marker.Type == MarkerType.Verse)
                            {
                                string verseID = marker.VerseRef.BBBCCCVVV.ToString().PadLeft(8,'0');
                                if (! bcvDict.ContainsKey(verseID))
                                {
                                    bcvDict.Add(verseID, verseID);
                                }
                            }
                        }
                    }
                }
            }
            return bcvDict;
        }

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

        private async Task GetNoteListAsync(PipeMessage message)
        {
            var bookNum = 0;
            var chapNum = 0;

            var data = JsonSerializer.Deserialize<GetNotesData>((string)message.Text);
            //var data = JsonConvert.DeserializeObject<GetNotesData>(jsonPayload);
            AppendText(Color.Blue, $"GetNotesListAsync received: Book: {data.BookId}, Chapter: {data.ChapterId}, IncludeResolved: {data.IncludeResolved}");

            if (data.BookId >= 0 && data.BookId <= 66 && data.ChapterId > 0)
            {
                m_booknum = m_project.AvailableBooks[data.BookId].Number;
                int chapter = data.ChapterId;
                // include resolved notes
                bool onlyUnresolved = !data.IncludeResolved;
                m_noteList = m_project.GetNotes(m_booknum, chapter, onlyUnresolved);
                AppendText(Color.Green, $"Book Num: {m_booknum} / {chapter}: {m_noteList.Count.ToString()}");


                var dataPayload = JsonSerializer.Serialize(m_noteList, new JsonSerializerOptions
                                                                                {
                                                                                    WriteIndented = true,
                                                                                    MaxDepth = 1024 * 100000
                                                                                });


                try
                {
                    var deserializedObj = JsonSerializer.Deserialize<IReadOnlyList<IProjectNote>>(dataPayload);

                    PipeMessage msgOut = new PipeMessage
                    {
                        Action = ActionType.SetNotesObject,
                        Text = "Notes Object",
                        Payload = dataPayload
                    };

                    await WriteMessageToPipeAsync(msgOut).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    AppendText(Color.Red, e.Message);
                }
            }
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
            var paratextExtractUSFM = new ParatextExtractUSFM();
            paratextExtractUSFM.ExportUSFMScripture(m_project, this);
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
