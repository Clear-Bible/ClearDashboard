
namespace ClearDashboard.DAL.Alignment.Corpora
{
    public interface ICache
    {
        public bool UseCache { get; set; }
        public void InvalidateCache();
    }
}
