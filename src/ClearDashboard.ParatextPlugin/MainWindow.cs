using Serilog;
using ClearDashboard.ParatextPlugin;
using ClearDashboard.ParatextPlugin.Actions;
using H.Formatters;
using H.Pipes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Paratext.PluginInterfaces;
using Pipes_Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ClearDashboard.ParatextPlugin.Helpers;
using ClearDashboard.ParatextPlugin.Models;
using ClearDashboard.Pipes_Shared.Models;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using Pipes_Shared.Models;
using Serilog.Core;

namespace ClearDashboardPlugin
{
    public partial class MainWindow : EmbeddedPluginControl
    {
        #region props

        // ReSharper disable once InconsistentNaming
        private IProject m_project;
        // ReSharper disable once IdentifierTypo
        private int m_booknum;
        // ReSharper disable once InconsistentNaming
        private IVerseRef m_verseRef;
        // ReSharper disable once InconsistentNaming
        private IReadOnlyList<IProjectNote> m_noteList;

        // ReSharper disable once InconsistentNaming
        private IWindowPluginHost m_host;
        private List<BiblicalTermsData> _termsList;
        // ReSharper disable once NotAccessedField.Local
        IPluginChildWindow m_parent;

        private readonly ListType _projectList = new ListType("Project", true, BiblicalTermListType.All);
        private readonly ListType _allList = new ListType("All", false, BiblicalTermListType.All);
        private readonly ListType _majorList = new ListType("Major", false, BiblicalTermListType.Major);

        // ReSharper disable once InconsistentNaming
        private readonly IBiblicalTermList m_listProject;
        // ReSharper disable once InconsistentNaming
        private readonly IBiblicalTermList m_listAll;

        private JsonSerializerOptions _jsonOptions;
        private delegate void AppendMsgTextDelegate(MsgColor color, string text);
        private Form _parentForm;


        // Named Pipe Props
        private const string PipeName = "ClearDashboard";
        private PipeServer<PipeMessage> _PipeServer { get; }
        private ISet<string> Clients { get; } = new HashSet<string>();


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
                WriteIndented = true
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

                AppendText(MsgColor.Green, $"{args.Connection.PipeName} connected!");

                try
                {
                    // send notice to client that we are connected
                    AppendText(MsgColor.Green, "Sending OnConnected Message to Client");
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

                AppendText(MsgColor.Green, $"{args.Connection.PipeName} disconnected!");
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
                assemblyName = "Pipes_Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            }
        }

        /// <summary>
        /// this window is closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Disposed(object sender, EventArgs e)
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
            AppendText(MsgColor.Purple, "INBOUND <- " + message.Action.ToString());

            // Do the command's action
            switch (message.Action)
            {
                case ActionType.SendText:
                    AppendText(MsgColor.Purple, "INBOUND <- " + message.Action.ToString() + ": " + message.Text);
                    break;
                case ActionType.GetCurrentVerse:
                    // send the current BCV location of Paratext
                    GetCurrentVerse().Forget();

                    break;
                case ActionType.GetBibilicalTermsAll:
                    // fire off into background
                    GetBiblicalTermsAllBackground().Forget();
                    break;
                case ActionType.GetBibilicalTermsProject:
                    // fire off into background
                    GetBiblicalTermsProjectBackground().Forget();
                    break;
                case ActionType.GetTargetVerses:
                    // fire off into background
                    GetUSXScripture().Forget();
                    break;
                case ActionType.GetNotes:
                    await GetNoteList(message).ConfigureAwait(false);
                    break;
                case ActionType.GetUSFM:
                    //await ShowUSFMScripture().ConfigureAwait(false);
                    break;
                case ActionType.GetUSX:
                    AppendText(MsgColor.Purple, "INBOUND <- " + message.Action.ToString());
                    await GetUSXScripture().ConfigureAwait(false);
                    break;
                case ActionType.OnConnected:
                    AppendText(MsgColor.Green, "ClearDashboard Connected");

                    // send the current BCV location of Paratext
                    GetCurrentVerse().Forget();

                    // get the paratext project info and send that over
                    Project proj = BuildProjectObject();
                    var payload = JsonSerializer.Serialize(proj, _jsonOptions);

                    AppendText(MsgColor.Orange, "OUTBOUND -> Sending Project Information");
                    await WriteMessageToPipeAsync(new PipeMessage
                    {
                        Action = ActionType.SetProject,
                        Text = "Project Object",
                        Payload = payload
                    });
                    AppendText(MsgColor.Orange, $"OUTBOUND -> Project Sent: {m_project.LongName}");
                    break;
                case ActionType.OnDisconnected:
                    AppendText(MsgColor.Orange, "ClearDashboard DisConnected");

                    await Task.Delay(1000).ConfigureAwait(true);

                    //btnRestart_Click(null, null);
                    break;
                default:
                    AppendText(MsgColor.Red, $"Method {message.Action} not implemented");
                    break;
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

        private async void OnLoad(object sender, EventArgs eventArgs)
        {
            try
            {
                AppendText(MsgColor.Green, "PipeServer starting...");
                _ = new PipeMessage();
                await _PipeServer.StartAsync().ConfigureAwait(false);
                AppendText(MsgColor.Green, "PipeServer is started!");
            }
            catch (Exception exception)
            {
                AppendText(MsgColor.Red, $"OnLoad {exception.Message}");
                OnExceptionOccurred(exception);
            }
        }

        private void OnExceptionOccurred(Exception exception)
        {
            // write to Serilog
            Log.Error($"OnLoad {exception.Message}");
            AppendText(MsgColor.Red, $"OnLoad {exception.Message}");
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
            parent.ProjectChanged += ProjectChanged;
            parent.VerseRefChanged += VerseRefChanged;

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
                GetCurrentVerse().Forget();
            }
        }

        #endregion Paratext overrides - standard functions


        #region Methods


        /// <summary>
        /// Send out the current verse through the pipe
        /// </summary>
        /// <returns></returns>
        private async Task GetCurrentVerse()
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
            AppendText(MsgColor.Orange, $"OUTBOUND -> Sent Current Verse: {m_verseRef}");
        }

        /// <summary>
        /// Send out the Biblical Terms for ALL BTs
        /// </summary>
        /// <returns></returns>
        private async Task GetBiblicalTermsAllBackground()
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
            AppendText(MsgColor.Orange, "OUTBOUND -> SetBiblicalTermsAll");
        }

        /// <summary>
        /// Send out the Biblical Terms for only the Project BTs
        /// </summary>
        /// <returns></returns>
        private async Task GetBiblicalTermsProjectBackground()
        {
            // ReSharper disable once InconsistentNaming
            string payloadBT = "";

            BibilicalTerms bt = new BibilicalTerms(_projectList, m_project, m_host);

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
            AppendText(MsgColor.Orange, "OUTBOUND -> SetBiblicalTermsProject");
        }

        /// <summary>
        /// Retrieves the USX & USFM for the project passing in the current book number
        /// </summary>
        /// <returns></returns>
        private async Task GetUSXScripture()
        {
            await Task.Run(async () =>
            {
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
            AppendText(MsgColor.Orange, "OUTBOUND -> SetUSX");

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
            project.ID = m_project.ID;
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

            return project;
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

        private async Task GetNoteList(PipeMessage message)
        {
            var bookNum = 0;
            var chapNum = 0;

            var data = JsonSerializer.Deserialize<GetNotesData>((string)message.Payload);
            //var data = JsonConvert.DeserializeObject<GetNotesData>(jsonPayload);

            if (data.BookID >= 0 && data.BookID <= 66 && data.ChapterID > 0)
            {
                m_booknum = m_project.AvailableBooks[data.BookID].Number;
                int chapter = data.ChapterID;
                // include resolved notes
                bool onlyUnresolved = !data.IncludeResolved;
                m_noteList = m_project.GetNotes(m_booknum, chapter, onlyUnresolved);
                AppendText(MsgColor.Green, $"Book Num: {m_booknum} / {chapter}: {m_noteList.Count.ToString()}");


                var dataPayload = JsonSerializer.Serialize(m_noteList, _jsonOptions);


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
                    AppendText(MsgColor.Red, e.Message);
                }
            }
        }


        /// <summary>
        /// Append colored text to the rich text box
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="color"></param>
        private void AppendText(MsgColor color, string sMsg)
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
                        cRTB.AppendText(sMsg, Color.Blue, this.rtb);
                        break;
                    case MsgColor.Red:
                        cRTB.AppendText(sMsg, Color.Red, this.rtb);
                        break;
                    case MsgColor.Green:
                        cRTB.AppendText(sMsg, Color.Green, this.rtb);
                        break;
                    case MsgColor.Orange:
                        cRTB.AppendText(sMsg, Color.Orange, this.rtb);
                        break;
                    case MsgColor.Purple:
                        cRTB.AppendText(sMsg, Color.Purple, this.rtb);
                        break;
                }

                // write to Serilog
                Log.Information(sMsg);
            }
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

        private void btnExportUSFM_Click(object sender, EventArgs e)
        {
            ExportUSFMScripture();
        }

        private void ExportUSFMScripture()
        {
            string exportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            exportPath = Path.Combine(exportPath, "ClearDashboard_Projects", "DataFiles", m_project.ID);

            if (!Directory.Exists(exportPath))
            {
                try
                {
                    Directory.CreateDirectory(exportPath);
                }
                catch (Exception e)
                {
                    AppendText(MsgColor.Red, e.Message);
                    return;
                }
            }

            // copy over the project's settings file
            string settingsFile = Path.Combine(GetParatextProjectsPath(), m_project.ShortName, "settings.xml");
            if (File.Exists(settingsFile))
            {
                try
                {
                    File.Copy(settingsFile, Path.Combine(exportPath, "settings.xml"), true);
                }
                catch (Exception e)
                {
                    AppendText(MsgColor.Red, e.Message);
                }

                FixParatextSettingsFile(Path.Combine(exportPath, "settings.xml"));

            }

            // copy over the project's custom versification file
            string versificationFile = Path.Combine(GetParatextProjectsPath(), m_project.ShortName, "custom.vrs");
            if (File.Exists(versificationFile))
            {
                try
                {
                    File.Copy(versificationFile, Path.Combine(exportPath, "custom.vrs"), true);
                }
                catch (Exception e)
                {
                    AppendText(MsgColor.Red, e.Message);
                }
            }

            // copy over project's usfm.sty
            string stylePath = GetAttributeFromSettingsXML.GetValue(Path.Combine(GetParatextProjectsPath(), m_project.ShortName, "settings.xml"), "StyleSheet");
            bool bFound = false;
            if (stylePath != "")
            {
                if (stylePath != "usfm.sty") // standard stylesheet
                {
                    if (File.Exists(Path.Combine(GetParatextProjectsPath(), m_project.ShortName, stylePath)))
                    {
                        try
                        {
                            File.Copy(Path.Combine(GetParatextProjectsPath(), m_project.ShortName, stylePath),
                                Path.Combine(exportPath, "usfm.sty"), true);
                        }
                        catch (Exception e)
                        {
                            AppendText(MsgColor.Red, e.Message);
                        }
                    }
                }
            }

            if (!bFound)
            {
                // standard stylesheet
                if (File.Exists(Path.Combine(GetParatextProjectsPath(), "usfm.sty")))
                {
                    try
                    {
                        File.Copy(Path.Combine(GetParatextProjectsPath(), "usfm.sty"),
                            Path.Combine(exportPath, "usfm.sty"), true);
                    }
                    catch (Exception e)
                    {
                        AppendText(MsgColor.Red, e.Message);
                    }
                }
            }


            for (int bookNum = 0; bookNum < this.m_project.AvailableBooks.Count; bookNum++)
            {
                if (BibleBookScope.IsBibleBook(m_project.AvailableBooks[bookNum].Code))
                {
                    AppendText(MsgColor.Blue,$"Processing {m_project.AvailableBooks[bookNum].Code}");

                    StringBuilder sb = new StringBuilder();
                    // do the header
                    sb.AppendLine($@"\id {m_project.AvailableBooks[bookNum].Code}");

                    int bookFileNum = 0;
                    if (m_project.AvailableBooks[bookNum].Number >= 40)
                    {
                        // do that crazy USFM file naming where Matthew starts at 41
                        bookFileNum = m_project.AvailableBooks[bookNum].Number + 1;
                    }
                    else
                    {
                        // normal OT book
                        bookFileNum = m_project.AvailableBooks[bookNum].Number;
                    }
                    var fileName = bookFileNum.ToString().PadLeft(2, '0') 
                                   + m_project.AvailableBooks[bookNum].Code + ".sfm";

                    IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
                    try
                    {
                        // get tokens by book number (from object) and chapter
                        tokens = m_project.GetUSFMTokens(m_project.AvailableBooks[bookNum].Number);
                    }
                    catch (Exception)
                    {
                        AppendText(MsgColor.Orange, $"No Scripture for {bookNum}");
                    }

                    foreach (var token in tokens)
                    {
                        if (token is IUSFMMarkerToken marker)
                        {
                            // a verse token
                            if (marker.Type == MarkerType.Verse)
                            {
                                // ReSharper disable once NotAccessedVariable
                                int p = 0;
                                bool result = int.TryParse(marker.Data, out p);

                                if (result)
                                {
                                    sb.Append($@"\v {marker.Data} ");
                                }
                                else
                                {
                                    // verse span so bust up the verse span
                                    string[] nums = marker.Data.Split('-');
                                    if (nums.Length > 1)
                                    {
                                        if (int.TryParse(nums[0], out p))
                                        {
                                            if (int.TryParse(nums[1], out p))
                                            {
                                                int start = Convert.ToInt16(nums[0]);
                                                int end = Convert.ToInt16(nums[1]);
                                                for (int j = start; j < end + 1; j++)
                                                {
                                                    sb.Append($@"\v {j} ");
                                                }
                                            }
                                        }
                                    }
                                }
                            } 
                            else if (marker.Type == MarkerType.Chapter)
                            {
                                // new chapter
                                sb.AppendLine();
                                sb.AppendLine(@"\c " + marker.Data);
                            }
                        }
                        else if (token is IUSFMTextToken textToken)
                        {
                            if (token.IsScripture)
                            {
                                // verse text
                                sb.AppendLine(textToken.Text);
                            }
                        }
                    }

                    // write out to \Documents\ClearDashboard\DataFiles\(project guid)\usfm files
                    File.WriteAllText(Path.Combine(exportPath, fileName), sb.ToString());
                }
            }
        }

        // update the settings file to use "normal" file extensions
        private void FixParatextSettingsFile(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            XmlElement root = doc.DocumentElement;
            var node = root.SelectSingleNode("//Naming");

            if (node != null)
            {
                node.Attributes["PrePart"].Value = "";
                node.Attributes["PostPart"].Value = ".sfm";
                node.Attributes["BookNameForm"].Value = "41MAT";
            }


            node = root.SelectSingleNode("//FileNameForm");
            if (node != null)
            {
                node.InnerText = "41MAT";
            }

            node = root.SelectSingleNode("//FileNameBookNameForm");
            if (node != null)
            {
                node.InnerText = "41MAT";
            }

            node = root.SelectSingleNode("//FileNamePostPart");
            if (node != null)
            {
                node.InnerText = ".sfm";
            }

            node = root.SelectSingleNode("//FileNamePrePart");
            if (node != null)
            {
                node.InnerText = "";
            }

            doc.Save(path);
        }


        /// <summary>
        /// Returns the Paratext Project's path.  Usually:
        /// {drive}:\My Paratext 9 Projects\
        /// </summary>
        /// <returns></returns>
        private string GetParatextProjectsPath()
        {
            string paratextProjectPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
            // check if directory exists
            if (!Directory.Exists(paratextProjectPath))
            {
                // directory doesn't exist so null this out
                paratextProjectPath = "";
            }

            return paratextProjectPath;
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
