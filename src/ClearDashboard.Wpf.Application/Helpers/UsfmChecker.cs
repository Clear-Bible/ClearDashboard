using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class UsfmChecker
    {
        public static async Task<List<UsfmErrorsWrapper>> CheckUsfm(ParatextProjectMetadata selectedProject, ProjectManager projectManager, ILocalizationService localizationService)
        {
            if (selectedProject is null)
            {
                return new List<UsfmErrorsWrapper>();
            }

            var result = await projectManager!.ExecuteRequest(new GetCheckUsfmQuery(selectedProject!.Id), CancellationToken.None);
            if (result.Success)
            {
                var errors = result.Data;

                ObservableCollection<UsfmError> usfmErrors;
                string errorTitle;

                if (errors!= null && errors.NumberOfErrors == 0)
                {
                    usfmErrors = new();

                    errorTitle = localizationService!.Get("AddParatextCorpusDialog_NoErrors");
                }
                else
                {
                    usfmErrors = new ObservableCollection<UsfmError>(errors.UsfmErrors);
                    errorTitle = localizationService!.Get("AddParatextCorpusDialog_ErrorCount");
                }

                return new List<UsfmErrorsWrapper>()
                {
                    new()
                    {
                        ProjectName = selectedProject.LongName,
                        ProjectId = selectedProject.Id,
                        UsfmErrors = usfmErrors,
                        ErrorTitle = errorTitle
                    }
                };

            }

            return new List<UsfmErrorsWrapper>();
        }
    }
}
