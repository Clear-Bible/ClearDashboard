using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using Newtonsoft.Json.Linq;
using SIL.Machine.Tokenization;

namespace ClearDashboard.Wpf.Application.Converters;

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

public class TokensTextRowMultiValueConvertor : IMultiValueConverter 
{

    private EngineStringDetokenizer CreateDetokenizer(string tokenizerName)
    {
        var tokenizer = (Tokenizer)Enum.Parse(typeof(Tokenizer),tokenizerName);
        return tokenizer switch
        {
            Tokenizer.LatinWordTokenizer => new EngineStringDetokenizer(new LatinWordDetokenizer()),
            Tokenizer.WhitespaceTokenizer => new EngineStringDetokenizer(new WhitespaceDetokenizer()),
            Tokenizer.ZwspWordTokenizer => new EngineStringDetokenizer(new ZwspWordDetokenizer()),
            _ => throw new NotSupportedException($"'{tokenizerName}' is not a valid tokenizer name")
        };
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var list = new List<TokenDisplay>();
        if (values.Length < 2)
        {
            throw new NullReferenceException(
                "There must be at least two values passed. The first should be the 'TokensTextRow` and the second the name of the Tokenizer (i.e. 'LatinWordTokenizer').");
        }
        
        if (values[0] is not TokensTextRow)
        {
            throw new NullReferenceException("The first parameter must be of type 'TokensTextRow`");
        }

        if (values[1] is not string)
        {
            throw new NullReferenceException("The second parameter must be of type 'string` which represents the Tokenizer used to tokenize the Corpus");
        }

        var tokensTextRow = (TokensTextRow)values[0];
        var detokenizer = CreateDetokenizer((string)values[1]);
        if (tokensTextRow.Tokens != null && tokensTextRow.Tokens.Any())
        {
            var tokensWithPadding = detokenizer.Detokenize(tokensTextRow.Tokens);
            return tokensWithPadding.Select(t => new TokenDisplay { Token = t.token, PaddingAfter = t.paddingAfter, PaddingBefore = t.paddingBefore });
        }
        return list;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TokensTextRowValueConvertor : IValueConverter
{
    private EngineStringDetokenizer createDetokenizer(Tokenizer tokenizer)
    {

        return null;
    }
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {

        var list = new List<TokenDisplay>();
        if (value == null)
        {
            return list;
        }


        var tokensTextRow = (TokensTextRow)value;
        var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
        if (tokensTextRow.Tokens != null && tokensTextRow.Tokens.Any())
        {
            var tokensWithPadding = detokenizer.Detokenize(tokensTextRow.Tokens);
            return tokensWithPadding.Select(t => new TokenDisplay { Token = t.token, PaddingAfter = t.paddingAfter, PaddingBefore = t.paddingBefore });
        }
        return list;
    
       

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

