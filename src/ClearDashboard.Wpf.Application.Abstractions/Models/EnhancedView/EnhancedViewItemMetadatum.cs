using AvalonDock.Layout;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using System;
using System.Linq;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;


//[JsonPolymorphic(TypeDiscriminatorPropertyName = "_t")]
//[JsonDerivedType(typeof(AquaCorpusAnalysisEnhancedViewItemMetadatum), typeDiscriminator: nameof(AquaCorpusAnalysisEnhancedViewItemMetadatum))]
//[JsonDerivedType(typeof(InterlinearEnhancedViewItemMetadatum), typeDiscriminator: nameof(InterlinearEnhancedViewItemMetadatum))]
//[JsonDerivedType(typeof(ParallelCorpusEnhancedViewItemMetadatum), typeDiscriminator: nameof(ParallelCorpusEnhancedViewItemMetadatum))]
//[JsonDerivedType(typeof(TokenizedCorpusEnhancedViewItemMetadatum), typeDiscriminator: nameof(TokenizedCorpusEnhancedViewItemMetadatum))]
//[JsonDerivedType(typeof(VerseAwareEnhancedViewItemMetadatum), typeDiscriminator: nameof(VerseAwareEnhancedViewItemMetadatum))]

public abstract class EnhancedViewItemMetadatum
{
    public bool? IsNewWindow { get; set; }

    public string? DisplayName { get; set; }

    public virtual Type ConvertToEnhancedViewItemViewModelType()
    {
        string? metadataAssemblyQualifiedName;
        if (this is VerseAwareEnhancedViewItemMetadatum)
        {
            // we want to display a VerseAwareEnhancedItemView, to do so we need convert to VerseAwareEnhancedViewItemViewModel
            metadataAssemblyQualifiedName = typeof(VerseAwareEnhancedViewItemMetadatum).AssemblyQualifiedName;
        }
        else
        {
            metadataAssemblyQualifiedName = GetType().AssemblyQualifiedName
                                            ?? throw new Exception($"AssemblyQualifiedName is null for type name {GetType().Name}");
        }

        return GetViewModelType(metadataAssemblyQualifiedName);
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

    public abstract LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel);
   

}