namespace ClearDashboard.DataAccessLayer.Models;

public static class TokenTypes
{
    public static string Prefix => "Prefix";
    public static string Infix => "Infix";
    public static string Stem => "Stem";
    public static string Suffix => "Suffix";
    public static string Circumfix => "Circumfix";

    public static string[] All => new[] { Prefix, Infix, Stem, Suffix, Circumfix };
}