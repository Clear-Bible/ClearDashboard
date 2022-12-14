using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record PutFormCommand(
        LexicalItemId LexicalItemId,
        Form Form) : ProjectRequestCommand<FormId>;
}
