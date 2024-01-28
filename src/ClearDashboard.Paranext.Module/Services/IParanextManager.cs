using SIL.Scripture;
using System.Threading.Tasks;

namespace ClearDashboard.Paranext.Module.Services
{
    public interface IParanextManager
    {
        public const string SampleDialogMenuId = "SampleDialogMenuId";

        public Task LoadRenderer();

        public Task EvaluateScriptInRendererAsync(string script);

        public Task SendVerseChangeCommandAsync(VerseRef verseRef);
    }
}
