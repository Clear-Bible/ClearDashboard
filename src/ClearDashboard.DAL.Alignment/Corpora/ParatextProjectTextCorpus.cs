using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;
using GetVersificationAndBookIdByParatextProjectIdQuery = ClearDashboard.DAL.Alignment.Features.Corpora.GetVersificationAndBookIdByParatextProjectIdQuery;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParatextProjectTextCorpus : ScriptureTextCorpus
    {
        public string ParatextProjectId { get; set; }
        internal ParatextProjectTextCorpus(string paratextProjectId, IMediator mediator, ScrVers versification, IEnumerable<string> bookAbbreviations)
        {
            ParatextProjectId = paratextProjectId;

            Versification = versification;
  
            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new ParatextProjectText(ParatextProjectId, mediator, Versification, bookAbbreviation));
            }
        }
        public override ScrVers Versification { get; }


        public static async Task<ParatextProjectTextCorpus> Get(
            IMediator mediator,
            string paratextProjectId, CancellationToken token)
        {
            var command = new GetVersificationAndBookIdByParatextProjectIdQuery(paratextProjectId);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                return new ParatextProjectTextCorpus(
                    command.ParatextProjectId, 
                    mediator, result.Data.versification ?? throw new InvalidParameterEngineException(name: "versification", value: "null"), 
                    result.Data.bookAbbreviations);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
