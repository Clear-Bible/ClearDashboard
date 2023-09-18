using System;
using System.Linq;
using System.Text;
using ClearBible.Engine.Corpora;

namespace ClearDashboard.Wpf.Application.Extensions;

public static class TokenExtensions
{
    public static string GetDisplayText(this Token token)
    {
        switch (token.GetType().Name)
        {
            case "Token":
                return token.SurfaceText;

            case "CompositeToken":
                var compositeToken = (CompositeToken)token;
                var orderedTokens = compositeToken.Tokens.OrderBy(t => t.Position).ToArray();
                var builder = new StringBuilder(orderedTokens.First().SurfaceText);
                foreach (var tokenMember in orderedTokens.Skip(1))
                {
                    builder.Append($" {tokenMember.SurfaceText}");
                }
                return builder.ToString();
            default:
                throw new NotImplementedException("Invalid switch case!");

        }
    }
}