using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ClearDashboard.DAL.Alignment.Tests.Collaboration
{
    public class TestOutputMessageSinkAdapter : ITestOutputHelper
    {
        private readonly IMessageSink _diagnosticMessageSink;
        public TestOutputMessageSinkAdapter(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public void WriteLine(string message)
        {
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage(message));
        }

        public void WriteLine(string format, params object[] args)
        {
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage(format, args));
        }
    }
}
