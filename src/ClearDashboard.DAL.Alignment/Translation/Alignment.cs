using ClearBible.Engine.Corpora;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Translation;

public class Alignment
{
    public AlignedTokenPairs AlignedTokenPair { get; }
    public string Verification { get; set; }
    public string OriginatedFrom { get; }

    /// <summary>
    /// For domain model use only!
    /// </summary>
    /// <param name="alignedTokenPair"></param>
    /// <param name="verification"></param>
    /// <param name="originatedFrom"></param>
    public Alignment(AlignedTokenPairs alignedTokenPair, string verification, string originatedFrom)
    {
        AlignedTokenPair = alignedTokenPair;
        Verification = verification;
        OriginatedFrom = originatedFrom;
    }
    
    /// <summary>
    /// Use this one to create manual alignments
    /// </summary>
    /// <param name="alignedTokenPair"></param>
    /// <param name="alignmentVerification"></param>
    public Alignment(AlignedTokenPairs alignedTokenPair, string verification)
    {
        AlignedTokenPair = alignedTokenPair;
        Verification = verification;
        OriginatedFrom = AlignmentOriginatedFrom.Assigned.ToString();
    }
}