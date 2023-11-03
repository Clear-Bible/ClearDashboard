using System;

namespace ClearDashboard.DataAccessLayer;

public static class ManuscriptIds
{
    public static readonly Guid HebrewManuscriptGuid = Guid.Parse("5db213425b714efc9dd23794525058a4");
    public static readonly string HebrewManuscriptId = HebrewManuscriptGuid.ToString();
    public static readonly string HebrewManuscriptLanguageId = "he";

    public static readonly Guid GreekManuscriptGuid = Guid.Parse("5db213425b714efc9dd23794525058a5");
    public static readonly string GreekManuscriptId = GreekManuscriptGuid.ToString();
    public static readonly string GreekManuscriptLanguageId = "el";
}