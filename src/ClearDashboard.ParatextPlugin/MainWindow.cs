using ClearDashboard.NamedPipes.Models;
using ClearDashboard.ParatextPlugin.Actions;
using Microsoft.Win32;
using NamedPipes;
using Newtonsoft.Json;
using NLog;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClearDashboard.ParatextPlugin
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

        private ServerPipe _serverPipe;
        private string _clearSuitePath = "";
        private NLog.ILogger _logger;

        private delegate void AppendTextDelegate(string text, StringBuilder sb);
        private delegate void AppendMsgTextDelegate(MsgColor color, string text);

        private StringBuilder _sb = new StringBuilder();

        private Form _parentForm;

        public enum MsgColor
        {
            Red,
            Green,
            Blue,
            Orange,
        }

        #endregion


        #region startup

        public MainWindow()
        {
            InitializeComponent();

            // set the version information
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format("Plugin Version: {0}", version);

            // hook up an event when the window is closed so we can kill off the pipe
            this.Disposed += MainWindow_Disposed;

            // hook up a logger
            _logger = NLog.LogManager.GetCurrentClassLogger();

            bool showErrorMessage = true;

            // check to see if ClearSuite is installed
            if (CheckIfClearSuiteInstalledAsync())
            {
                if (_clearSuitePath != string.Empty)
                {
                    _serverPipe = new ServerPipe("ClearSuitePlugin", p => p.StartStringReaderAsync());
                    _serverPipe.DataReceived += ServerPipeOnDataReceived;
                    AppendText(MsgColor.Green, "Server Pipe Created");

                    // launch ClearEngineController
                    try
                    {
                        //using (Process proc = new Process())
                        //{
                        //    proc.StartInfo.FileName = _clearSuitePath;
                        //    proc.StartInfo.UseShellExecute = true;
                        //    proc.Start();
                        //}

                        showErrorMessage = false;
                    }
                    catch (Exception)
                    {
                        showErrorMessage = true;
                        AppendText(MsgColor.Red, "ClearSuite Not Detected ERROR");
                    }
                }
            }


            if (showErrorMessage)
            {
                // TODO Do some alert now that ClearSuite is NOT installed
            }

        }

        /// <summary>
        /// this window is closing event
        /// </summary>
        /// <param abbr="sender"></param>
        /// <param abbr="e"></param>
        private async void MainWindow_Disposed(object sender, EventArgs e)
        {
            // this user control is closing - clean up pipe
            NamedPipeMessage msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.ServerClosed, "", "");
            var msgSend = msgOut.CreateMessage();
            await _serverPipe.WriteString(msgSend).ConfigureAwait(false);

            // we need a delay before closing to ensure that the message is received on the client
            await Task.Delay(2000).ConfigureAwait(true);

            UnhookPipe();
        }

        /// <summary>
        /// Disconnect the named pipe
        /// </summary>
        private void UnhookPipe()
        {
            // unhook the pipe
            _serverPipe.Close();
            _serverPipe.DataReceived -= ServerPipeOnDataReceived;
            _serverPipe = null;
        }

        #endregion

        #region overrides - standard functions

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
            parent.SetTitle(ClearSuitePlugin.pluginName);
            parent.ProjectChanged += ProjectChanged;
            parent.VerseRefChanged += VerseRefChanged;

            SetProject(parent.CurrentState.Project);
            m_verseRef = parent.CurrentState.VerseRef;

            m_host = host;
            m_project = parent.CurrentState.Project;
            m_parent = parent;

            parent.VerseRefChanged += VerseRefChanged;
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
        /// <param abbr="sender"></param>
        /// <param abbr="oldReference"></param>
        /// <param abbr="newReference"></param>
        private async void VerseRefChanged(IPluginChildWindow sender, IVerseRef oldReference, IVerseRef newReference)
        {

            if (newReference != m_verseRef)
            {
                m_verseRef = newReference;
                await ShowUSXScripture().ConfigureAwait(true);
            }
        }

        #endregion overrides - standard functions


        /// <summary>
        /// The client has sent a message through the pipe to us
        /// </summary>
        /// <param abbr="sender"></param>
        /// <param abbr="e"></param>
        private async void ServerPipeOnDataReceived(object sender, PipeEventArgs e)
        {
            NamedPipeMessage msg = null;
            try
            {
                msg = JsonConvert.DeserializeObject<NamedPipeMessage>(e.String);
            }
            catch (Exception exception)
            {
                msg = null;
                _logger.Log(LogLevel.Error, exception);
                return;
            }

            // Do the command's action
            switch (msg.actionType)
            {
                case "GetBibilicalTerms":
                    _logger.Log(LogLevel.Info, "ServerPipeOnDataReceived: " + msg.actionType.ToString());
                    AppendText(MsgColor.Blue, "ServerPipeOnDataReceived: " + msg.actionType.ToString());

                    GetBibilicalTerms bt = new GetBibilicalTerms(ProjectList, m_project, m_host);
                    List<BiblicalTermsData> biblicalTermList = bt.ProcessBiblicalTerms(m_project);

                    var dataPayload = JsonConvert.SerializeObject(biblicalTermList, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    NamedPipeMessage msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.BiblicalTerms, "", dataPayload);
                    var msgSend = msgOut.CreateMessage();

                    _logger.Log(LogLevel.Info, "ServerPipeOnDataReceived: " + NamedPipeMessage.ActionType.BiblicalTerms.ToString());

                    await _serverPipe.WriteString(msgSend).ConfigureAwait(false);

                    _logger.Log(LogLevel.Info, "ServerPipeOnDataReceived: msgSend sent");
                    AppendText(MsgColor.Blue, "ServerPipeOnDataReceived: msgSend sent");
                    break;
                case "GetTargetVerses":
                    await ShowUSXScripture().ConfigureAwait(false);
                    break;
                case "GetNotes":
                    await GetNoteList(msg.actionCommand, msg.jsonPayload).ConfigureAwait(false);
                    break;
                case "OnConnected":
                    AppendText(MsgColor.Green, "ClearDashboard Connected");

                    // send the current BCV location of Paratext
                    msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.CurrentVerse, m_verseRef.BBBCCCVVV.ToString(), "");
                    msgSend = msgOut.CreateMessage();

                    await _serverPipe.WriteString(msgSend).ConfigureAwait(false);
                    AppendText(MsgColor.Green, String.Format($"Sent Current Verse: {0} {1}:{2}",m_verseRef.BookCode, m_verseRef.ChapterNum, m_verseRef.VerseNum));
                    break;
                case "OnDisconnected":
                    AppendText(MsgColor.Orange, "ClearDashboard DisConnected");
                    break;
            }
        }

        /// <summary>
        /// Does a registry check to ensure that ClearSuite is reachable
        /// </summary>
        /// <returns></returns>
        public bool CheckIfClearSuiteInstalledAsync()
        {
            //Here we peek into the registry to see if they even have clear engine controller installed
            var clearEngineControllerPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Clear\ClearSuite", "Path", null);

            if (Directory.Exists(clearEngineControllerPath))
            {
                // file doesn't exist so null this out
                if (File.Exists(Path.Combine(clearEngineControllerPath, "ClearSuite.Wpf.exe")))
                {
                    _clearSuitePath = Path.Combine(clearEngineControllerPath, "ClearSuite.Wpf.exe");
                    return true;
                }
            }

            return false;
        }

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
                }
                catch (Exception e)
                {
                    AppendText(MsgColor.Red, e.Message);
                }

                NamedPipeMessage msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.NotesObject, "", dataPayload);
                var msgSend = msgOut.CreateMessage();

                await _serverPipe.WriteString(msgSend).ConfigureAwait(false);

            }
        }

        private async Task ShowUSXScripture()
        {
            var temp = m_project.GetUSX(m_verseRef.BookNum);
            if (temp != null)
            {
                var dataPayload = JsonConvert.SerializeObject(temp, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                var msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.TargetVerseText, m_verseRef.BBBCCCVVV.ToString(), dataPayload);
                var msgSend = msgOut.CreateMessage();

                _logger.Log(LogLevel.Info, "VerseTextSent: msgSend sent");
                AppendText(MsgColor.Blue, "VerseTextSent: msgSend created");

                _logger.Log(LogLevel.Info, "VerseTextSent: " + NamedPipeMessage.ActionType.BiblicalTerms.ToString());

                await _serverPipe.WriteString(msgSend).ConfigureAwait(false);

                _logger.Log(LogLevel.Info, "VerseTextSent: msgSend sent");
                AppendText(MsgColor.Blue, "VerseTextSent: msgSend sent");
            }
        }

        /// <summary>
        /// Append colored text to the rich text box
        /// </summary>
        /// <param abbr="sError"></param>
        /// <param abbr="color"></param>
        internal void AppendText(string sError, StringBuilder sb)
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
        /// <param abbr="sMsg"></param>
        /// <param abbr="color"></param>
        internal void AppendText(MsgColor color, string sMsg)
        {
            //check for threading issues
            if (this.InvokeRequired)
            {
                this.Invoke(new AppendMsgTextDelegate(AppendText), new object[] { color, sMsg });
            }
            else
            {
                switch (color)
                {
                    case MsgColor.Blue:
                        cRTB.AppendText(sMsg + "\n", Color.Blue, this.rtb);
                        break;
                    case MsgColor.Red:
                        cRTB.AppendText(sMsg + "\n", Color.Red, this.rtb);
                        break;
                    case MsgColor.Green:
                        cRTB.AppendText(sMsg + "\n", Color.Green, this.rtb);
                        break;
                    case MsgColor.Orange:
                        cRTB.AppendText(sMsg + "\n", Color.Orange, this.rtb);
                        break;
                }

            }
        }

        /// <summary>
        /// Force a restart of the named pipes
        /// </summary>
        /// <param abbr="sender"></param>
        /// <param abbr="e"></param>
        private async void btnRestart_Click(object sender, EventArgs e)
        {
            // disconnect the pipe
            UnhookPipe();

            await Task.Run(() => Task.Delay(500)).ConfigureAwait(false);

            // reconnect the pipe
            _serverPipe = new ServerPipe("ClearSuitePlugin", p => p.StartStringReaderAsync());
            _serverPipe.DataReceived += ServerPipeOnDataReceived;

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
            AppendText(MsgColor.Green, "Server Pipe Restarted");

        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            GetNotesData data = new GetNotesData();
            data.BookID = 5;
            data.ChapterID = 1;
            data.IncludeResolved = true;
            var dataPayload = JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            _ = GetNoteList("", dataPayload);
        }


        //private async Task ShowUSFMScripture()
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

        //    var dataPayload = JsonConvert.SerializeObject(lines, Formatting.None,
        //        new JsonSerializerSettings()
        //        {
        //            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //        });

        //    NamedPipeMessage msgOut = new NamedPipeMessage(NamedPipeMessage.ActionType.TargetVerseText, "", dataPayload);
        //    var msgSend = msgOut.CreateMessage();

        //    _logger.Log(LogLevel.Info, "VerseTextSent: msgSend sent");
        //    AppendText(MsgColor.Blue, "VerseTextSent: msgSend created");

        //    _logger.Log(LogLevel.Info, "VerseTextSent: " + NamedPipeMessage.ActionType.BiblicalTerms.ToString());

        //    await _serverPipe.WriteString(msgSend).ConfigureAwait(false);

        //    _logger.Log(LogLevel.Info, "VerseTextSent: msgSend sent");
        //    AppendText(MsgColor.Blue, "VerseTextSent: msgSend sent");
        //}
    }
}
