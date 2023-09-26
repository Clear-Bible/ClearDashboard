using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiblicalTermFileModel = ClearDashboard.DataAccessLayer.Models.Term;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public class OriginatedFromBiblicalTerm
    {
        public string App { get; set; } = OriginatedFromLexiconItem.APP_PARATEXT;
        public string Module { get; set; } = OriginatedFromLexiconItem.MODULE_BIBLICALTERMS;
        public string TermId { get; set; } = string.Empty;
        public string Strong { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public IEnumerable<OriginatedFromLocationReference> References { get; set; } = Enumerable.Empty<OriginatedFromLocationReference>();

        public static OriginatedFromBiblicalTerm FromBiblicalTermFileModel(BiblicalTermFileModel biblicalTermFileModel)
        {
            return new OriginatedFromBiblicalTerm
            {
                TermId = biblicalTermFileModel.Id,
                Strong = biblicalTermFileModel.Strong,
                Language = biblicalTermFileModel.Language,
                Definition = biblicalTermFileModel.Definition,
                References = biblicalTermFileModel.References.Verse.Select(e => new OriginatedFromLocationReference 
                { 
                    LocationType = OriginatedFromLexiconItem.REFERENCE_LOCATION_TYPE_VERSE,
                    Location = e 
                })
            };
        }
    }
}
