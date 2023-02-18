using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Extensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaTokenizedTextCorpusMetadata
{
    public string? VersionId;


    private static readonly string TokenizedCorpusMetadataKey = "ClearDashboardAquaModule";
    public static AquaTokenizedTextCorpusMetadata Get(TokenizedTextCorpus tokenizedTextCorpus)
    {
        try
        {
            return tokenizedTextCorpus.TokenizedTextCorpusId.Metadata?.DeserializeValue<AquaTokenizedTextCorpusMetadata>(TokenizedCorpusMetadataKey)
                ?? throw new InvalidParameterEngineException(
                    name: "tokenizedTextCorpusId",
                    value: "null",
                    message: "either tokenizedTextCorpusId or its Metadata property are null");
        }
        catch (KeyNotFoundException)
        { 
            return new AquaTokenizedTextCorpusMetadata(); 
        }
    }
    public async Task Save(TokenizedTextCorpusId tokenizedTextCorpusId, IMediator mediator)
    {
        var tokenizedTextCorpus = await TokenizedTextCorpus.Get(
            mediator,
            tokenizedTextCorpusId,
            false);

        if (tokenizedTextCorpus.TokenizedTextCorpusId.Metadata.ContainsKey(TokenizedCorpusMetadataKey))
            tokenizedTextCorpus.TokenizedTextCorpusId.Metadata[TokenizedCorpusMetadataKey] = this;
        else
            tokenizedTextCorpus.TokenizedTextCorpusId.Metadata.Add(TokenizedCorpusMetadataKey, this);

        await tokenizedTextCorpus.Update(mediator);
    }
}
