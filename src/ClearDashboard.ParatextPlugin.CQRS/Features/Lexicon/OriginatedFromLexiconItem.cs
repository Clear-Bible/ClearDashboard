using System;
using System.Collections.Generic;
using System.Text;
using LexemeFileModel = ClearDashboard.DataAccessLayer.Models.Lexeme;
using SenseFileModel = ClearDashboard.DataAccessLayer.Models.Sense;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public class OriginatedFromLexiconItem
    {
        public const string APP_PARATEXT = "Paratext";
        public const string MODULE_LEXICON = "Lexicon";
        public const string MODULE_BIBLICALTERMS = "BiblicalTerms";
        public const string MODULE_TERMRENDERINGS = "TermRenderings";
        public const string REFERENCE_LOCATION_TYPE_VERSE = "Verse";

        public string App { get; set; } = OriginatedFromLexiconItem.APP_PARATEXT;
        public string Module { get; set; } = OriginatedFromLexiconItem.MODULE_LEXICON;
        public string LexemeType { get; set; } = string.Empty;
        public string Form { get; set; } = string.Empty;
        public int Homograph { get; set; }
        public string SenseId { get; set; } = string.Empty;

        public static OriginatedFromLexiconItem FromLexiconItemFileModel(LexemeFileModel lexemeFileModel, SenseFileModel senseFileModel)
        {
            return new OriginatedFromLexiconItem
            {
                LexemeType = lexemeFileModel.Type,
                Form = lexemeFileModel.Form,
                Homograph = lexemeFileModel.Homograph,
                SenseId = senseFileModel.Id
            };
        }
    }
}
