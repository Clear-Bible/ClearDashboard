using System;
using ParaTextPlugin.Data;

namespace ClearDashboard.DataAccessLayer.NamedPipes;

public class PipeEventArgs : EventArgs
{
    public PipeEventArgs(PipeMessage pipeMessage)
    {
        this.PipeMessage = pipeMessage;
    }

    public PipeMessage PipeMessage { get; }
}