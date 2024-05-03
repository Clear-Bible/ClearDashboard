namespace ClearDashboard.DAL.Alignment.Features.Corpora;

public record SplitInstruction(
    int Index,
    int Length,
    string TokenText,
    string? TrainingText
);