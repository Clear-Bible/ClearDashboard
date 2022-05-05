using System.Text.RegularExpressions;

namespace ClearDashboard.DataAccessLayer.Models
{
    public static class Hebrew
    {
        /// <summary>
        /// This code is based on Reinier De Blois <rdeblois@americanbible.org> VB.Net snippet 
        /// </summary>
        /// <param name="tString"></param>
        /// <returns></returns>
        public static string Transliterate(string tString)
        {
            switch (tString)
            {
                case "יָטְבָתָה":
                {
                    return "yoṭbātāh";
                }

                case "יִשָּׂשׂכָר":
                {
                    return "yiśśākār";
                }

                default:
                {
                    tString = "#" + tString + "#";

                    // hyphen
                    tString = tString.Replace((char)(1470), '@'); // maqqef > hyphen

                    // meteg
                    tString = tString.Replace((char)(1469), '!'); // meteg > exclamation mark

                    // consonants
                    tString = tString.Replace((char)(1488), (char)(702)); // aleph

                    tString = tString.Replace(((char)(1489) + (char)(1468)).ToString(), "bb"); // beth + dagesh

                    tString = tString.Replace((char)(1489), 'b'); // beth

                    tString = tString.Replace(((char)(1490) + (char)(1468)).ToString(), "gg"); // gimel + dagesh

                    tString = tString.Replace((char)(1490), 'g'); // gimel

                    tString = tString.Replace(((char)(1491) + (char)(1468)).ToString(), "dd"); // daleth + dagesh

                    tString = tString.Replace((char)(1491), 'd'); // daleth

                    tString = tString.Replace(((char)(1492) + (char)(1468)).ToString(), "h"); // he + dagesh

                    tString = tString.Replace((char)(1492), 'h'); // he

                    tString = tString.Replace(((char)(1493) + (char)(1468)).ToString(), "ww"); // waw + dagesh

                    tString = tString.Replace((char)(1493), 'w'); // waw

                    tString = tString.Replace(((char)(1494) + (char)(1468)).ToString(), "zz"); // zayin + dagesh

                    tString = tString.Replace((char)(1494), 'z'); // zayin

                    tString = tString.Replace((char)(1495), (char)(7717)); // cheth (never with dagesh)

                    tString = tString.Replace(((char)(1496) + (char)(1468)).ToString(),
                        ((char)(7789) + (char)(7789)).ToString()); // teth + dagesh

                    tString = tString.Replace((char)(1496), (char)(7789)); // theth

                    tString = tString.Replace(((char)(1497) + (char)(1468)).ToString(), "yy"); // yod + dagesh

                    tString = tString.Replace(((char)(1497)).ToString(), "y"); // yod

                    tString = tString.Replace(((char)(1498)).ToString(), "k"); // kaph final+ dagesh

                    tString = tString.Replace(((char)(1499) + (char)(1468)).ToString(), "kk"); // kaph

                    tString = tString.Replace(((char)(1499)).ToString(), "k"); // kaph

                    tString = tString.Replace(((char)(1500) + (char)(1468)).ToString(), "ll"); // lamed + dagesh

                    tString = tString.Replace(((char)(1500)).ToString(), "l"); // lamed

                    tString = tString.Replace(((char)(1501)).ToString(), "m"); // mem final

                    tString = tString.Replace(((char)(1502) + (char)(1468)).ToString(), "mm"); // mem  + dagesh

                    tString = tString.Replace(((char)(1502)).ToString(), "m"); // mem

                    tString = tString.Replace(((char)(1503)).ToString(), "n"); // nun final

                    tString = tString.Replace(((char)(1504) + (char)(1468)).ToString(), "nn"); // nun + dagesh

                    tString = tString.Replace(((char)(1504)).ToString(), "n"); // nun

                    tString = tString.Replace(((char)(1505) + (char)(1468)).ToString(), "ss"); // samekh + dagesh

                    tString = tString.Replace(((char)(1505)).ToString(), "s"); // samekh

                    tString = tString.Replace((char)(1506), (char)(703)); // ain (never with dagesh)

                    tString = tString.Replace(((char)(1507)).ToString(), "p"); // pe final

                    tString = tString.Replace(((char)(1508) + (char)(1468)).ToString(), "pp"); // pe + dagesh

                    tString = tString.Replace(((char)(1508)).ToString(), "p"); // pe

                    tString = tString.Replace((char)(1509), (char)(7779)); // tsade final

                    tString = tString.Replace(((char)(1510) + (char)(1468)).ToString(),
                        ((char)(7779) + (char)(7779)).ToString()); // tsade + dagesh

                    tString = tString.Replace((char)(1510), (char)(7779)); // tsade

                    tString = tString.Replace(((char)(1511) + (char)(1468)).ToString(), "qq"); // qoph + dagesh

                    tString = tString.Replace(((char)(1511)).ToString(), "q"); // qoph

                    tString = tString.Replace(((char)(1512) + (char)(1468)).ToString(), "rr"); // resh + dagesh

                    tString = tString.Replace(((char)(1512)).ToString(), "r"); // resh

                    tString = tString.Replace(((char)(1513) + (char)(1468) + (char)(1474)).ToString(),
                        ((char)(347) + (char)(347)).ToString()); // sin + dagesh

                    tString = tString.Replace(((char)(1513) + (char)(1474) + (char)(1468)).ToString(),
                        ((char)(347) + (char)(347)).ToString()); // sin + dagesh (alternative order)

                    tString = tString.Replace(((char)(1513) + (char)(1474)).ToString(),
                        ((char)(347)).ToString()); // sin

                    tString = tString.Replace(((char)(1513) + (char)(1468) + (char)(1473)).ToString(),
                        ((char)(353) + (char)(353)).ToString()); // shin + dagesh

                    tString = tString.Replace(((char)(1513) + (char)(1473) + (char)(1468)).ToString(),
                        ((char)(353) + (char)(353)).ToString()); // shin + dagesh (alternative order)

                    tString = tString.Replace(((char)1513 + (char)1473).ToString(), ((char)(353)).ToString()); // sin

                    tString = tString.Replace(((char)(1513)).ToString(), ""); // sin without dot

                    tString = tString.Replace(((char)(1514) + (char)(1468)).ToString(), "tt"); // taw + dagesh

                    tString = tString.Replace((char)(1514), 't'); // taw

                    // vowels
                    tString = tString.Replace((char)(1456), '%'); // shewa

                    tString = tString.Replace((char)(1457), (char)(277)); // hataf segol

                    tString = tString.Replace((char)(1458), (char)(259)); // hataph patach

                    tString = tString.Replace((char)(1459), (char)(335)); // hataph qamets

                    tString = tString.Replace((char)(1460), 'i'); // chireq

                    tString = tString.Replace((char)(1461), (char)(275)); // tsere

                    tString = tString.Replace((char)(1462), 'e'); // segol

                    tString = tString.Replace((char)(1463), 'a'); // patach

                    tString = tString.Replace((char)(1464), (char)(257)); // qamets

                    tString = tString.Replace((char)(1465), 'o'); // cholem

                    tString = tString.Replace((char)(1467), 'u'); // qibbuts

                    tString = tString.Replace("~", " "); // tilde

                    // cheth with patach furtivum
                    tString = Regex.Replace(tString, (char)(7717) + "a([@ #])", "a" + (char)(7717) + "$1");

                    // ayin with patach furtivum
                    tString = Regex.Replace(tString, (char)(703) + "a([@ #])", "a" + (char)(703) + "$1");

                    tString = Regex.Replace(tString, "([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])wo", "$1" + (char)(244));

                    tString = Regex.Replace(tString, "([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])ww", "$1" + (char)(251));

                    // open syllable with qamets followed by doubled consonant
                    tString = Regex.Replace(tString, "ābb", "obb");

                    tString = Regex.Replace(tString, "āgg", "ogg");

                    tString = Regex.Replace(tString, "ādd", "odd");

                    tString = Regex.Replace(tString, "āzz", "ozz");

                    tString = Regex.Replace(tString, "āṭṭ", "oṭṭ");

                    tString = Regex.Replace(tString, "āyy", "oyy");

                    tString = Regex.Replace(tString, "ākk", "okk");

                    tString = Regex.Replace(tString, "āll", "oll");

                    tString = Regex.Replace(tString, "āmm", "omm");

                    tString = Regex.Replace(tString, "ānn", "onn");

                    tString = Regex.Replace(tString, "āss", "oss");

                    tString = Regex.Replace(tString, "āpp", "opp");

                    tString = Regex.Replace(tString, "āṣṣ", "oṣṣ");

                    tString = Regex.Replace(tString, "āqq", "oqq");

                    tString = Regex.Replace(tString, "āśś", "ośś");

                    tString = Regex.Replace(tString, "āšš", "ošš");

                    tString = Regex.Replace(tString, "ātt", "ott");

                    // closed syllable with qamets followed by syllable starting with begadkepat, either or not with dagesh
                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%bb",
                        "o$1b"); // begadkepat with dagesh

                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%gg",
                        "o$1g"); // begadkepat with dagesh

                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%dd",
                        "o$1d"); // begadkepat with dagesh

                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%kk",
                        "o$1k"); // begadkepat with dagesh

                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%pp",
                        "o$1p"); // begadkepat with dagesh

                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%tt",
                        "o$1t"); // begadkepat with dagesh

                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%([bgdkpt])",
                        "ā$1$2"); // begadkepat without dagesh

                    // closed syllable with qamets followed by syllable starting with non-begadkepat or maqqef
                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%([ʾhwzḥṭylmnsʿṣqrśš@])", "o$1$2");

                    // closed syllable with qamets followed by maqqef
                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])(@)", "o$1$2");

                    // open syllable with qamets followed by open syllable with hataph qamets
                    tString = Regex.Replace(tString, "ā([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])" + (char)(335), "o$1" + (char)(335));

                    // remove meteg
                    tString = Regex.Replace(tString, "!", "");

                    // begadkepat at beginning word or after shewa
                    tString = Regex.Replace(tString, "([%@ #])bb", "$1b");

                    tString = Regex.Replace(tString, "([%@ #])gg", "$1g");

                    tString = Regex.Replace(tString, "([%@ #])dd", "$1d");

                    tString = Regex.Replace(tString, "([%@ #])kk", "$1k");

                    tString = Regex.Replace(tString, "([%@ #])pp", "$1p");

                    tString = Regex.Replace(tString, "([%@ #])tt", "$1t");

                    // remove shewa at end of closed syllable
                    tString = Regex.Replace(tString,
                        "([āaēeiôoûu][ʾbgdhwzḥṭyklmnsʿpṣqrśšt])%([ʾbgdhwzḥṭyklmnsʿpṣqrśšt])",
                        "$1$2");

                    // remove shewa at end of word
                    tString = Regex.Replace(tString, "%([#@ ])", "$1");

                    // replace shewa with inverted e
                    tString = tString.Replace('%', (char)(477));

                    tString = Regex.Replace(tString, "ēy([ʾbgdhwzḥṭklmnsʿpṣqrśšt-])", (char)(234) + "$1");

                    tString = Regex.Replace(tString, "iy([ʾbgdhwzḥṭklmnsʿpṣqrśšt-])", (char)(238) + "$1");

                    if (tString.EndsWith("iy"))
                        tString = tString.Substring(0, tString.Length - 2) + (char)(238);

                    if (tString.EndsWith((char)(275) + "y"))
                        tString = tString.Substring(0, tString.Length - 2) + (char)(234);

                    tString = tString.Replace("@", "-");

                    return tString.Substring(1, tString.Length - 2);
                }

            }
        }
    }
}
