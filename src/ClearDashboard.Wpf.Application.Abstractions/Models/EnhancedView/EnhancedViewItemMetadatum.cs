using System;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

public abstract class EnhancedViewItemMetadatum
{
    public bool? IsNewWindow { get; set; }

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

}