namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization
{
    public class EnhancedViewItemMetadatumOld
    {
        public MessageType MessageType { get; set; }
        public object? Data { get; set; }
    }

    public enum MessageType
    {
        ShowTokenizationWindowMessage,
        ShowParallelTranslationWindowMessage
    }
}


// Alignments/Interlinear

//"TranslationSetId": "3a16c1e2-1bed-403a-97d8-f35346c8ca39",
//"AlignmentSetId": null,
//"DisplayName": "zz_SUR - Macula Greek Interlinear",
//"ParallelCorpusId": "e7e65c43-83ac-4b5b-8da5-b9c109e6d8c6",
//"ParallelCorpusDisplayName": "zz_SUR - Macula Greek",
//"IsRTL": false,
//"IsTargetRTL": null,
//"IsNewWindow": false,
//"SourceParatextId": "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
//"TargetParatextId": "5db21342-5b71-4efc-9dd2-3794525058a5"

// CorpusNode

//"ParatextProjectId": "3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff",
//"ProjectName": "HEB/GRK",
//"TokenizationType": "LatinWordTokenizer",
//"CorpusId": "2a3700d7-db49-4d03-aaae-fee239679ab5",
//"TokenizedTextCorpusId": "37236cb7-bf6d-4a2a-9687-aff29abba1f6",
//"CorpusType": 10,
//"IsRTL": true,
//"IsNewWindow": false
