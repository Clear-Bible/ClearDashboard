using System;
using System.Text.Json.Serialization;
using AvalonDock.Layout;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;

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

    public Type ConvertToEnhancedViewItemViewModelType()
    {
        string? metadataAssemblyQualifiedName;
        if (this is VerseAwareEnhancedViewItemMetadatum)
        {
            metadataAssemblyQualifiedName = typeof(VerseAwareEnhancedViewItemMetadatum).AssemblyQualifiedName;
        }
        else
        {
            metadataAssemblyQualifiedName =
                (GetType().BaseType != null ?
                    GetType().BaseType!.AssemblyQualifiedName :
                    GetType().AssemblyQualifiedName)
                ?? throw new Exception($"AssemblyQualifiedName is null for type name {GetType().Name}");
        }

        var viewModelAssemblyQualifiedName = metadataAssemblyQualifiedName!
            .Replace("EnhancedViewItemMetadatum", "EnhancedViewItemViewModel")
            .Replace("Models", "ViewModels");
        return Type.GetType(viewModelAssemblyQualifiedName)
               ?? throw new Exception($"AssemblyQualifiedName {viewModelAssemblyQualifiedName} type not found");

    }

    public abstract LayoutDocument CreateLayoutDocument(IEnhancedViewModel viewModel);
   

}