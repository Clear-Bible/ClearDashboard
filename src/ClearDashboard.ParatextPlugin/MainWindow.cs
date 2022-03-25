using Serilog;
using ClearDashboard.ParatextPlugin;
using ClearDashboard.ParatextPlugin.Actions;
using H.Formatters;
using H.Pipes;
using Newtonsoft.Json;
using Paratext.PluginInterfaces;
using Pipes_Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClearDashboard.Pipes_Shared.Models;
using Pipes_Shared.Models;
using Serilog.Core;

namespace ClearDashboardPlugin
{
    public partial class MainWindow : EmbeddedPluginControl
    {
        #region props

        private IProject m_project;
        private int m_booknum;
        private IVerseRef m_verseRef;
        private IReadOnlyList<IProjectNote> m_noteList;

        private IWindowPluginHost m_host;
        private IBiblicalTermList m_list;
        IPluginChildWindow m_parent;

        private ListType ProjectList = new ListType("Project", true, BiblicalTermListType.All);
        private ListType AllList = new ListType("All", false, BiblicalTermListType.All);
        private ListType MajorList = new ListType("Major", false, BiblicalTermListType.Major);

        private IBiblicalTermList m_listProject;
        private IBiblicalTermList m_listAll;

        ////private ServerPipe _serverPipe;
        private string _clearSuitePath = "";

        private delegate void AppendTextDelegate(string text, StringBuilder sb);
        private delegate void AppendMsgTextDelegate(MsgColor color, string text);

        //private StringBuilder _sb = new StringBuilder();

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

            // configure serilog
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File("Plugin.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // set instance to global logger
            Log.Logger = log;


            // get the version information
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format($"Plugin Version: {version}");

            // hook up an event when the window is closed so we can kill off the pipe
            this.Disposed += MainWindow_Disposed;

            bool showErrorMessage = true;

            //// check to see if ClearSuite is installed
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

            //_PipeServer = new PipeServer<PipeMessage>(PipeName);
            _ = new PipeMessage();
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
            _PipeServer.ClientDisconnected += (o, args) =>
            {
                Clients.Remove(args.Connection.PipeName);
                UpdateClientList();

                AppendText(MsgColor.Green, $"{args.Connection.PipeName} disconnected!");
            };
            _PipeServer.MessageReceived += async (sender, args) =>
            {
                if (args.Message != null)
                {
                    await OnMessageReceivedAsync(args.Message).ConfigureAwait(false);
                }
            };

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
            
            //PipeMessage msg = null;
            //try
            //{
            //    msg = JsonConvert.DeserializeObject<PipeMessage>(e.String);
            //}
            //catch (Exception exception)
            //{
            //    msg = null;
            //    Log.Logger.Error(exception.Message);
            //    return;
            //}


            // Do the command's action
            switch (message.Action)
            {
                case ActionType.GetCurrentVerse:
                    // send the current BCV location of Paratext
                    await WriteMessageToPipeAsync(new PipeMessage
                    {
                        Action = ActionType.CurrentVerse,
                        Text = m_verseRef.BBBCCCVVV.ToString(),
                    }).ConfigureAwait(false);

                    break;
                case ActionType.CurrentVerse:

                    break;
                case ActionType.SendText:
                    AppendText(MsgColor.Orange, "INBOUND <- " + message.Action.ToString() + ": " + message.Text);
                    break;
                case ActionType.GetBibilicalTermsAll:

                    List<BiblicalTermsData> termsList;
                    if (m_list == null)
                    {
                        m_list = m_host.GetBiblicalTermList(BiblicalTermListType.All);

                        BibilicalTerms btAll = new BibilicalTerms(AllList, m_project, m_host);
                        termsList = btAll.ProcessBiblicalTerms(m_project);
                    }

                    var payloadBTAll = JsonConvert.SerializeObject(m_list, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    await WriteMessageToPipeAsync(new PipeMessage
                    {
                        Action = ActionType.SetBiblicalTerms,
                        Text = "Project Object",
                        Payload = payloadBTAll
                    }).ConfigureAwait(false);

                    break;
                case ActionType.GetBibilicalTermsProject:
                    BibilicalTerms bt = new BibilicalTerms(ProjectList, m_project, m_host);
                    var btList = bt.ProcessBiblicalTerms(m_project);

                    var payloadBT = JsonConvert.SerializeObject(btList, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    await WriteMessageToPipeAsync(new PipeMessage
                    {
                        Action = ActionType.SetBiblicalTerms,
                        Text = "Project Object",
                        Payload = payloadBT
                    }).ConfigureAwait(false);

                    break;
                case ActionType.GetTargetVerses:
                    await GetUSXScripture().ConfigureAwait(false);
                    break;
                case ActionType.GetNotes:
                    //await GetNoteList(msg.actionCommand, msg.jsonPayload).ConfigureAwait(false);
                    break;
                case ActionType.GetProject:
                    AppendText(MsgColor.Green, "Sending Project Information");
                    await WriteMessageToPipeAsync(message).ConfigureAwait(false);
                    AppendText(MsgColor.Green, String.Format($"Project Sent: {m_project.LongName}"));
                    break;
                case ActionType.OnConnected:
                    AppendText(MsgColor.Green, "ClearDashboard Connected");

                    // send the current BCV location of Paratext
                    var msgOut = new PipeMessage
                    {
                        Action = ActionType.CurrentVerse,
                        Text = m_verseRef.BBBCCCVVV.ToString(),
                    };

                    await WriteMessageToPipeAsync(msgOut).ConfigureAwait(false);
                    AppendText(MsgColor.Green, String.Format($"Sent Current Verse: {m_verseRef.ToString()}"));

                    // get the paratext project info and send that over
                    Project proj = BuildProjectObject();

                    var payload = JsonConvert.SerializeObject(proj, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    await OnMessageReceivedAsync(new PipeMessage
                    {
                        Action = ActionType.GetProject,
                        Text = "Project Object",
                        Payload = payload
                    });
                    break;
                case ActionType.OnDisconnected:
                    AppendText(MsgColor.Orange, "ClearDashboard DisConnected");

                    await Task.Delay(2000).ConfigureAwait(true);

                    btnRestart_Click(null, null);
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
        private async void VerseRefChanged(IPluginChildWindow sender, IVerseRef oldReference, IVerseRef newReference)
        {

            if (newReference != m_verseRef)
            {
                m_verseRef = newReference;
                await OnMessageReceivedAsync(new PipeMessage { Action = ActionType.CurrentVerse }).ConfigureAwait(false);
            }
        }

        #endregion Paratext overrides - standard functions


        /// <summary>
        /// The client has sent a message through the pipe to us
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private async void ServerPipeOnDataReceived(object sender, PipeEventArgs e)
        //{
        //    PipeMessage msg = null;
        //    try
        //    {
        //        msg = JsonConvert.DeserializeObject<PipeMessage>(e.String);
        //    }
        //    catch (Exception exception)
        //    {
        //        msg = null;
        //        _logger.Log(LogLevel.Error, exception);
        //        return;
        //    }

        //    // Do the command's action
        //    switch (msg.actionType)
        //    {
        //        case "BibilicalTerms":
        //            await BibilicalTerms(msg);
        //            break;
        //        case "GetTargetVerses":
        //            await GetUSXScripture().ConfigureAwait(false);
        //            break;
        //        case "GetNotes":
        //            await GetNoteList(msg.actionCommand, msg.jsonPayload).ConfigureAwait(false);
        //            break;
        //        case "OnConnected":
        //            AppendText(MsgColor.Green, "ClearDashboard Connected");

        //            // send the current BCV location of Paratext
        //            var msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.CurrentVerse, m_verseRef.BBBCCCVVV.ToString(), "");
        //            var msgSend = msgOut.CreateMessage();

        //            await _serverPipe.WriteString(msgSend).ConfigureAwait(false);
        //            AppendText(MsgColor.Green, String.Format($"Sent Current Verse: {0} {1}:{2}", m_verseRef.BookCode, m_verseRef.ChapterNum, m_verseRef.VerseNum));

        //            await GetUSXScripture().ConfigureAwait(false);

        //            await BibilicalTerms(msg);
        //            break;
        //        case "OnDisconnected":
        //            AppendText(MsgColor.Orange, "ClearDashboard DisConnected");

        //            await Task.Delay(2000).ConfigureAwait(true);

        //            btnRestart_Click(null, null);
        //            break;
        //    }
        //}

        //private async Task BibilicalTerms(NamedPipeMessage msg)
        //{
        //    _logger.Log(LogLevel.Info, "ServerPipeOnDataReceived: " + msg.actionType.ToString());
        //    AppendText(MsgColor.Blue, "ServerPipeOnDataReceived: " + msg.actionType.ToString());
        //    string dataPayload = "";

        //    await Task.Run(() => { 
        //        BibilicalTerms bt = new BibilicalTerms(ProjectList, m_project, m_host);
        //        List<BiblicalTermsData> biblicalTermList = bt.ProcessBiblicalTerms(m_project);

        //        dataPayload = JsonConvert.SerializeObject(biblicalTermList, Formatting.None,
        //            new JsonSerializerSettings()
        //            {
        //                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //            });
        //    }).ConfigureAwait(true);

        //    NamedPipeMessage msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.SetBiblicalTerms, "", dataPayload);
        //    var msgSend = msgOut.CreateMessage();

        //    _logger.Log(LogLevel.Info, "ServerPipeOnDataReceived: " + NamedPipeMessage.ActionType.SetBiblicalTerms.ToString());

        //    //await _serverPipe.WriteString(msgSend).ConfigureAwait(false);

        //    try
        //    {
        //        await _PipeServer.WriteAsync(new PipeMessage
        //        {
        //            Action = NamedPipeMessage.ActionType.SendText,
        //            Text = "Biblical Terms Sent"
        //        }).ConfigureAwait(false);
        //    }
        //    catch (Exception exception)
        //    {
        //        OnExceptionOccurred(exception);
        //    }


        //    _logger.Log(LogLevel.Info, "ServerPipeOnDataReceived: msgSend sent");
        //    AppendText(MsgColor.Blue, "ServerPipeOnDataReceived: msgSend sent");
        //}

        #region Methods

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

        private async Task GetNoteList(string actionCommand, string jsonPayload)
        {
            var data = JsonConvert.DeserializeObject<GetNotesData>(jsonPayload);

            if (data.BookID >= 0 && data.BookID <= 66 && data.ChapterID > 0)
            {
                m_booknum = m_project.AvailableBooks[data.BookID].Number;
                int chapter = data.ChapterID;
                // include resolved notes
                bool onlyUnresolved = !data.IncludeResolved;
                m_noteList = m_project.GetNotes(m_booknum, chapter, onlyUnresolved);
                AppendText(MsgColor.Green, $"Book Num: {m_booknum} / {chapter}: {m_noteList.Count.ToString()}");

                var dataPayload = JsonConvert.SerializeObject(m_noteList, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto,
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.None,
                    });

                try
                {
                    var deserializedObj = JsonConvert.DeserializeObject<IReadOnlyList<IProjectNote>>(dataPayload, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto,
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.None,
                    });

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
        /// Retrieves the USX & USFM for the project passing in the current book number
        /// </summary>
        /// <returns></returns>
        private async Task GetUSXScripture()
        {
            var usx = m_project.GetUSX(m_verseRef.BookNum);
            if (usx != null)
            {
                var dataPayload = JsonConvert.SerializeObject(usx, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                await WriteMessageToPipeAsync(new PipeMessage
                {
                    Action= ActionType.SetUSX,
                    Text = "Set USX",
                    Payload= dataPayload,
                }).ConfigureAwait(false);
            }

            var usfm = m_project.GetUSFM(m_verseRef.BookNum);
            if (usfm != null)
            {
                var dataPayload = JsonConvert.SerializeObject(usfm, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                await WriteMessageToPipeAsync(new PipeMessage
                {
                    Action = ActionType.SetUSFM,
                    Text = "Set USFM",
                    Payload = dataPayload,
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Append colored text to the rich text box
        /// </summary>
        /// <param name="sError"></param>
        /// <param name="color"></param>
        private void AppendText(string sError, StringBuilder sb)
        {
            //check for threading issues
            if (this.InvokeRequired)
            {
                this.Invoke(new AppendTextDelegate(AppendText), new object[] { sError, sb });
            }
            else
            {
                cRTB.AppendText(sError + "\n\n", Color.Red, this.rtb);
                cRTB.AppendText(sb.ToString(), Color.Blue, this.rtb);
                cRTB.AppendText("\n\n", Color.Red, this.rtb);
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
        private async void btnRestart_Click(object sender, EventArgs e)
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


        private async Task ShowUSFMScripture()
        {
            IEnumerable<IUSFMToken> tokens = m_project.GetUSFMTokens(m_verseRef.BookNum, m_verseRef.ChapterNum);
            List<string> lines = new List<string>();
            foreach (var token in tokens)
            {
                if (token is IUSFMMarkerToken marker)
                {
                    switch (marker.Type)
                    {
                        case MarkerType.Verse:
                            lines.Add($"v {marker.Data}");
                            break;
                        case MarkerType.Book:
                            lines.Add($"{marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.Chapter:
                            lines.Add($"{marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.Character:
                            lines.Add($"Marker Character: {marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.Milestone:
                            lines.Add($"Marker Milestone: {marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.MilestoneEnd:
                            lines.Add($"Marker MilestoneEnd: {marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.End:
                            lines.Add($"Marker End: {marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.Note:
                            lines.Add($"Marker Note: {marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.Unknown:
                            lines.Add($"Marker Unknown: {marker.Marker} {marker.Data}");
                            break;
                        case MarkerType.Paragraph:
                            lines.Add($" {marker.Marker} {marker.Data}");
                            break;
                    }
                }
                else if (token is IUSFMTextToken textToken)
                {
                    if (lines.Count > 0)
                    {
                        lines[lines.Count - 1] += (textToken.Text);
                    }
                    else
                    {
                        lines.Add(textToken.Text);
                    }
                }
                else if (token is IUSFMAttributeToken)
                {
                    lines.Add("Attribute Token: " + token.ToString());
                }
                else
                {
                    lines.Add("Unexpected token type: " + token.ToString());
                }
            }

            var dataPayload = JsonConvert.SerializeObject(lines, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            //PipeMessage msgOut = new PipeMessage(ActionType.SetUSX, "", dataPayload);
            //var msgSend = msgOut.CreateMessage();
            //AppendText(MsgColor.Blue, "VerseTextSent: msgSend created");
            //await _serverPipe.WriteString(msgSend).ConfigureAwait(false);
            //AppendText(MsgColor.Blue, "VerseTextSent: msgSend sent");
        }

        #endregion
    }
}
