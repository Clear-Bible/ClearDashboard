using System.Text.Json.Serialization;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration;

public class LexiconSnapshot
{
    public IEnumerable<Models.Lexicon_Lexeme>? Lexemes { get; set; }
    public IEnumerable<Models.Lexicon_Meaning>? Meanings { get; set; }
    public IEnumerable<Models.Lexicon_Form>? Forms { get; set; }
    public IEnumerable<Models.Lexicon_Translation>? Translations { get; set; }
    public IEnumerable<Models.Lexicon_SemanticDomain>? SemanticDomains { get; set; }
    public IEnumerable<Models.Lexicon_SemanticDomainMeaningAssociation>? SemanticDomainMeaningAssociations { get; set; }
}