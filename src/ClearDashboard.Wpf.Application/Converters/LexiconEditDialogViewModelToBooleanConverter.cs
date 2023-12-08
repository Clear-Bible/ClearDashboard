using System;
using System.Globalization;
using System.Windows.Data;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;

namespace ClearDashboard.Wpf.Application.Converters;

public class LexiconEditDialogViewModelToBooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values.Length != 2)
        {
            throw new ArgumentException(
                $"Two parameters must be passed. The first should be of type '{nameof(LexiconEditDialogViewModel)}' and the second of type '{nameof(EditableLexemeViewModel)}'.");
        }

        if (values[0] is LexiconEditDialogViewModel viewModel)
        {
            if (values[1] is EditableLexemeViewModel lexeme)
            {
                return viewModel.ActionButtonIsEnabled(lexeme);
            }
        }
        return Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}