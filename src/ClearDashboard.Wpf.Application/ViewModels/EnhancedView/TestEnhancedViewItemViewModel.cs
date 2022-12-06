using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class TestEnhancedViewItemViewModel : EnhancedViewItemViewModel
{
    public string? Message { get; set; }

    public TestEnhancedViewItemViewModel()
    {
        Title = "TestEnhancedViewItemViewModel";
        Message = "This message was created in the TestEnhancedViewItemViewModel constructor.";
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return base.OnActivateAsync(cancellationToken);
    }
}