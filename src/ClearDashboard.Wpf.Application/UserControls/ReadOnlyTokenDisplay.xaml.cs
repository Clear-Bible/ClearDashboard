using System.Windows.Input;
using ClearBible.Engine.Corpora;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a single <see cref="Token"/> with no interactivity.
    /// </summary>
    public partial class ReadOnlyTokenDisplay
    {
        public ReadOnlyTokenDisplay()
        {
            InitializeComponent();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var d = this.DataContext;
            base.OnMouseDown(e);
        }
    }
}
