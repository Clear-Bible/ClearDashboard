using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.Converters;

public class VerseTokensValueConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is VerseTokens verseTokens ? verseTokens.Tokens.Select(t => t.SurfaceText).ToList() : new List<string>();

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TokensTextRowValueConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is TokensTextRow verseTokens ? verseTokens.Tokens.Select(t=>t.SurfaceText).ToList() : new List<string>();

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}