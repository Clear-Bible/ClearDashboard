using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParatextPluginTextCorpus : ScriptureTextCorpus
    {
        public string ParatextPluginId { get; set; }
        private string projectName_;

        internal ParatextPluginTextCorpus(string paratextPluginId, IMediator mediator, ScrVers versification, IEnumerable<string> bookAbbreviations, string projectName)
        {
            ParatextPluginId = paratextPluginId;

            Versification = versification;
            projectName_ = projectName;

            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new ParatextPluginText(ParatextPluginId, mediator, Versification, bookAbbreviation, projectName));
            }
        }
        public override ScrVers Versification { get; }


        public static async Task<ParatextPluginTextCorpus> Get(string projectName, 
            IMediator mediator,
            string paratextPluginId)
        {
            var command = new GetVersificationAndBookIdByParatextPluginIdQuery(paratextPluginId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return new ParatextPluginTextCorpus(
                    command.ParatextPluginId, 
                    mediator, result.Data.versification ?? throw new InvalidParameterEngineException(name: "versification", value: "null"), 
                    result.Data.bookAbbreviations, projectName);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
