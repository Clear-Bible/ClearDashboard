using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    public class TokensTextRowMultiValueConvertor : IMultiValueConverter
    {
        private EngineStringDetokenizer CreateDetokenizer(string tokenizerName)
        {
            var tokenizer = (Tokenizer)Enum.Parse(typeof(Tokenizer), tokenizerName);
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
                try
                {
                    var tokensWithPadding = detokenizer.Detokenize(tokensTextRow.Tokens);
                    return tokensWithPadding.Select(t => new TokenDisplay
                    { Token = t.token, PaddingAfter = t.paddingAfter, PaddingBefore = t.paddingBefore });
                }
                catch (Exception ex)
                {
                    var logger = IoC.Get<ILogger<TokensTextRowMultiValueConvertor>>();
                    logger.LogError(ex, $"An unexpected error occurred while detokenizing '{tokensTextRow.Text} tokens");
                }
            }
            return list;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
