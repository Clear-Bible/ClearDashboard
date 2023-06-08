using AvalonDock.Layout;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using System;
using System.Linq;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;


public abstract class EnhancedViewItemMetadatum
{
    public bool? IsNewWindow { get; set; }

    public string? DisplayName { get; set; }

    public virtual Type ConvertToEnhancedViewItemViewModelType()
    {
        string? metadataAssemblyQualifiedName;
        //if (this is VerseAwareEnhancedViewItemMetadatum)
        //{
        //    // we want to display a VerseAwareEnhancedItemView, to do so we need convert to VerseAwareEnhancedViewItemViewModel
        //    metadataAssemblyQualifiedName = typeof(VerseAwareEnhancedViewItemMetadatum).AssemblyQualifiedName;
        //}
        //else
        {
            //metadataAssemblyQualifiedName = GetType().AssemblyQualifiedName
            //                                ?? throw new Exception($"AssemblyQualifiedName is null for type name {GetType().Name}");

            metadataAssemblyQualifiedName = GetType().FullName
                                            ?? throw new Exception($"AssemblyQualifiedName is null for type name {GetType().Name}");
        }

        //return GetViewModelType(metadataAssemblyQualifiedName);

        return GetViewModelTypeByFullName(metadataAssemblyQualifiedName);
    }

    private static Type GetViewModelType(string? metadataAssemblyQualifiedName)
    {
        var viewModelAssemblyQualifiedName = metadataAssemblyQualifiedName!
            .Replace("Metadatum", "ViewModel") // fix class name
            .Replace("Models", "ViewModels"); // fix namespace

        return FindType(viewModelAssemblyQualifiedName) ??
               throw new Exception($"AssemblyQualifiedName {viewModelAssemblyQualifiedName} type not found");
    }

    private static Type? FindType(string assemblyQualifiedName)
    {
        return
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .SingleOrDefault(t => t.AssemblyQualifiedName.Equals(assemblyQualifiedName));
    }

    private static Type GetViewModelTypeByFullName(string? fullName)
    {
        var viewModelFullName = fullName!
            .Replace("Metadatum", "ViewModel") // fix class name
            .Replace("Models", "ViewModels"); // fix namespace

        return FindTypeByFullName(viewModelFullName) ??
               throw new Exception($"AssemblyQualifiedName {viewModelFullName} type not found");
    }

    private static Type? FindTypeByFullName(string fullName)
    {
        return
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .SingleOrDefault(t => t.FullName.Equals(fullName));
    }

    public abstract LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel);
   

}