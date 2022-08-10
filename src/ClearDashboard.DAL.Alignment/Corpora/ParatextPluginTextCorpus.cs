using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParatextPluginTextCorpus : ScriptureTextCorpus
    {
        public string ParatextPluginId { get; set; }
        internal ParatextPluginTextCorpus(string paratextPluginId, IMediator mediator, ScrVers versification, IEnumerable<string> bookAbbreviations)
        {
            ParatextPluginId = paratextPluginId;

            Versification = versification;
  
            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new ParatextPluginText(ParatextPluginId, mediator, Versification, bookAbbreviation));
            }
        }
        public override ScrVers Versification { get; }


        public static async Task<ParatextPluginTextCorpus> Get(
            IMediator mediator,
            string paratextPluginId)
        {
            var command = new GetVersificationAndBookIdByParatextPluginIdQuery(paratextPluginId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return new ParatextPluginTextCorpus(
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
