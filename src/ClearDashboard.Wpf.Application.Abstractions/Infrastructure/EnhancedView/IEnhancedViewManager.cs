using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Infrastructure.EnhancedView
{
    public interface IEnhancedViewManager
    {

        //Task AddTokenizedCorpusToNewEnhancedView(TokenizedCorpusEnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken);

        //Task AddAlignmentSetToEnhancedView(AlignmentEnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken);

        //Task AddInterlinearToEnhancedView(InterlinearEnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken);

        Task AddMetadatumEnhancedView(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken);

        Task<TViewModel> ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : Screen;
    }
}
