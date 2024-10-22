﻿namespace ClearDashboard.DataAccessLayer.Models;

public static class MetadatumKeys
{
    public static string IsParallelCorpusToken => "IsParallelCorpusToken";
    public static string ParallelCorpusId => "ParallelCorpusId";
	public static string WasSplit => "WasSplit";
	public static string SplitTokenSourceId => "SplitTokenSourceId";
	public static string SplitTokenSourceSurfaceText => "SplitTokenSourceSurfaceText";
	public static string SplitTokenInitialChildren => "SplitTokenInitialChildren";
	public static string ModelTokenMetadata => "ModelTokenMetadata";

    public static string Type => "Type";
    public static string GrammarId => "GrammarId";
    public static string CircumfixGroup => "CircumfixGroup";
    public static string IsParallelCompositeToken => "IsParallelCompositeToken";
}