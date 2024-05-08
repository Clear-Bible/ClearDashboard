using Autofac;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Extensions;
using MediatR;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaTokenizedTextCorpusMetadata
{
    //used to determine if version has been added to aqua
    public int? id { get; set; }

    //properties set on AddVersion but not returned in ListVersion.

    public string? abbreviation { get; set; }
    public string? isoLanguage { get; set; }
    public string? isoScript { get; set; }
    public int? forwardTranslationToVersionId { get; set; } = null;
    public int? backTranslationToVersionId { get; set; } = null;
    public bool machineTranslation { get; set; } = false;


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
    public async Task SaveAsync(TokenizedTextCorpusId tokenizedTextCorpusId, IComponentContext context, CancellationToken cancellationToken)
    {
        var tokenizedTextCorpus = await TokenizedTextCorpus.GetAsync(
            context,
            tokenizedTextCorpusId,
            false,
            cancellationToken);

        if (tokenizedTextCorpus.TokenizedTextCorpusId.Metadata.ContainsKey(TokenizedCorpusMetadataKey))
            tokenizedTextCorpus.TokenizedTextCorpusId.Metadata[TokenizedCorpusMetadataKey] = this;
        else
            tokenizedTextCorpus.TokenizedTextCorpusId.Metadata.Add(TokenizedCorpusMetadataKey, this);

        await tokenizedTextCorpus.UpdateAsync(context, cancellationToken);
    }
}
