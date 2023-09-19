using System;

namespace ClearDashboard.DataAccessLayer;

public static class ManuscriptIds
{
    public static Guid HebrewManuscriptGuid = Guid.Parse("5db213425b714efc9dd23794525058a4");
    public static string HebrewManuscriptId = HebrewManuscriptGuid.ToString();

    public static Guid GreekManuscriptGuid = Guid.Parse("5db213425b714efc9dd23794525058a5");
    public static string GreekManuscriptId = GreekManuscriptGuid.ToString();
}