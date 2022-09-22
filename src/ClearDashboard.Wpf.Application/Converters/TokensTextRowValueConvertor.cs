using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters;
[Obsolete]
public class TokensTextRowValueConvertor : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {

        var list = new List<TokenDisplayViewModel>();
        if (value == null)
        {
            return list;
        }


        var tokensTextRow = (TokensTextRow)value;
        var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
        if (tokensTextRow.Tokens != null && tokensTextRow.Tokens.Any())
        {
            var tokensWithPadding = detokenizer.Detokenize(tokensTextRow.Tokens);
            return tokensWithPadding.Select(t => new TokenDisplayViewModel { Token = t.token, PaddingAfter = t.paddingAfter, PaddingBefore = t.paddingBefore });
        }
        return list;
    
       

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}