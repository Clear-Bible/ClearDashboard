

using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Messages;
using SIL.Scripture;
using System.Threading.Tasks;

namespace ClearDashboard.Paranext.Module.Services
{
    public class DashboardJavascriptApi
    {
        private readonly IEventAggregator _eventAggregator;

        public DashboardJavascriptApi(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public DashboardJavascriptApi()
        {
        }

        public async Task<string> VerseChange(string verseString)
        {
            //await _eventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(verseString));
            await Task.Run(() => { });
            return $"hi!:  {verseString}";
        }
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
