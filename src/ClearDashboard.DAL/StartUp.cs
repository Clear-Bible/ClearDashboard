using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using ClearDashboard.DAL.Events;
using ClearDashboard.DAL.NamedPipes;

namespace ClearDashboard.DAL
{
    public class StartUp
    {
        #region Events


        // event handler to be raised when the Paratext Username changes
        public static event EventHandler ParatextUserNameEventHandler;


        public event NamedPipesClient.PipesEventHandler NamedPipeChanged;

        private void RaisePipesChangedEvent(string s)
        {
            NamedPipesClient.PipeEventArgs args = new NamedPipesClient.PipeEventArgs(s);
            NamedPipeChanged?.Invoke(this, args);
        }

        #endregion

        #region Startup

        public StartUp()
        {
            // Wire up named pipes
            NamedPipesClient.Instance.InitializeAsync().ContinueWith(t =>
                    Debug.WriteLine($"Error while connecting to pipe server: {t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);

            NamedPipesClient.Instance.NamedPipeChanged += HandleEvent;



            GetParatextUserName();
        }

        #endregion

        #region Shutdown

        public void OnClosing()
        {
            NamedPipesClient.Instance.Dispose();
        }

        #endregion


        #region Methods

        private void HandleEvent(object sender, NamedPipesClient.PipeEventArgs args)
        {
            RaisePipesChangedEvent(args.Text);
        }

        public void GetParatextUserName()
        {
            // TODO this is a hack that reads the first user in the Paratext project's directory
            // from the localUsers.txt file.  This needs to be changed to the user we get from 
            // the Paratext API
            Paratext.ParatextUtils paratextUtils = new Paratext.ParatextUtils();
            var user = paratextUtils.GetCurrentParatextUser();

            // raise the paratext username event
            ParatextUserNameEventHandler?.Invoke(this, new CustomEvents.ParatextUsernameEventArgs(user));
        }

        public async Task SendMessage(string message)
        {
            message = message.Trim() + " through the DAL";

            await NamedPipesClient.Instance.WriteAsync(message);
        }

        #endregion



    }
}
