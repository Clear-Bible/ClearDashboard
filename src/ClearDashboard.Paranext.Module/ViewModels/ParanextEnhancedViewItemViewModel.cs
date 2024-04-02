using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using ClearDashboard.Paranext.Module.Models;
using System;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using SIL.Scripture;
using CefSharp;
using ClearDashboard.Paranext.Module.Views;
using System.Linq;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Paranext.Module.ViewModels
{
    public class ParanextEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        ParanextEnhancedViewItemView? paranextEnhancedViewItemView;
        public ParanextEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager,  
            IEnhancedViewManager enhancedViewManager, 
            INavigationService? navigationService,
            ILogger<VerseAwareEnhancedViewItemViewModel>? logger, 
            IEventAggregator? eventAggregator, 
            IMediator? mediator, 
            ILifetimeScope? lifetimeScope, 
            IWindowManager windowManager, 
            [KeyFilter("ParanextExtension")] ILocalizationService localizationService)
            : base(
                  projectManager, 
                  enhancedViewManager, 
                  navigationService, 
                  logger, 
                  eventAggregator, 
                  mediator, 
                  lifetimeScope, 
                  windowManager, 
                  localizationService)
        {
        }


        private double height_ = 400;
        public double Height
        {
            get => height_;
            set => Set(ref height_, value);
        }

        private Uri? uri_;
        public Uri? Uri
        {
            get => uri_;
            set => Set(ref uri_, value);
        }
        public override async Task GetData(CancellationToken cancellationToken)
        {
            DisplayName = (EnhancedViewItemMetadatum as ParanextEnhancedViewItemMetadatum)?.DisplayName ?? "<display name not set>";
            Random r = new Random();
            int randomInt = r.Next(0, 5000);
            await Task.Delay(randomInt);
            Uri = new Uri((EnhancedViewItemMetadatum as ParanextEnhancedViewItemMetadatum)?.UrlString ?? throw new Exception("Url string not set"));
            return;// Task.CompletedTask; //return base.GetData(metadatum, cancellationToken);
        }
        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }

        public void SetVerse()
        {
            if (ParentViewModel == null) return;

            var currentVerseRef = new VerseRef(ParentViewModel!.CurrentBcv?.GetBBBCCCVVV() ?? throw new Exception("GetBBBCCVVV is null"));

            //var iFrameId = paranextEnhancedViewItemView?.Cef.GetBrowser().GetFrameIdentifiers()
            //    .FirstOrDefault(fi => fi != paranextEnhancedViewItemView?.Cef.GetBrowser().MainFrame.Identifier)
            //    ?? throw new Exception("no paranextEnhancedViewItemView or couldn't find a non-main frame identifier. ");
            //var frame = paranextEnhancedViewItemView?.Cef.GetBrowser().GetFrame(iFrameId) ?? throw new Exception($"No frame id ${iFrameId} found");

            var frame = paranextEnhancedViewItemView?.Cef.GetBrowser().MainFrame;
            if (frame == null) throw new Exception("main frame is not available");

            frame!.ExecuteJavaScriptAsync(
               $"papi.commands.sendCommand('platform.dashboardVerseChange', '{currentVerseRef}', 0);");
                
        }
        public override Task RefreshData(ReloadType reloadType = ReloadType.Refresh, CancellationToken cancellationToken = default)
        {
            if (ParentViewModel == null)
                return Task.CompletedTask;

            SetVerse();
            return base.RefreshData(reloadType, cancellationToken);
        }
        protected override void OnViewAttached(object view, object context)
        {
            paranextEnhancedViewItemView = (ParanextEnhancedViewItemView)view;
            base.OnViewAttached(view, context);
        }
    }
}
