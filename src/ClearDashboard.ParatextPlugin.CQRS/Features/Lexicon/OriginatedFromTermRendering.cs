using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TermRenderingFileModel = ClearDashboard.DataAccessLayer.Models.TermRendering;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public class OriginatedFromTermRendering
    {
        public string App { get; set; } = OriginatedFromLexiconItem.APP_PARATEXT;
        public string Module { get; set; } = OriginatedFromLexiconItem.MODULE_TERMRENDERINGS;
        public string TermRenderingId { get; set; } = string.Empty;

        public static OriginatedFromTermRendering FromTermRenderingFileModel(TermRenderingFileModel termRenderingFileModel)
        {
            return new OriginatedFromTermRendering
            {
                TermRenderingId = termRenderingFileModel.Id
            };
        }
    }
}
