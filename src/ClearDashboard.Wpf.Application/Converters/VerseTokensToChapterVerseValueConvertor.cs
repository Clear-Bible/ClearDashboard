using System;
using System.Globalization;
using System.Windows.Data;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.Application.Converters;

public class VerseTokensToChapterVerseValueConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is VerseTokens verseTokens ? $"Chapter: {verseTokens.Chapter}, Verse: {verseTokens.Verse}" : string.Empty;

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}