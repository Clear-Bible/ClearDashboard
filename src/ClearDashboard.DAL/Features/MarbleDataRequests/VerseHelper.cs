﻿using ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{
    public static class VerseHelper
    {
        private static Dictionary<int, ParatextBookFileName> BookNames = new Dictionary<int, ParatextBookFileName>
        {
            {01, new ParatextBookFileName{code="GEN", abbr = "Gen", shortname ="Genesis", longname ="Genesis", fileID="01", BBB="01"}},
            {02, new ParatextBookFileName{code="EXO", abbr = "Exod", shortname ="Exodus", longname ="Exodus", fileID="02", BBB="02"}},
            {03, new ParatextBookFileName{code="LEV", abbr = "Lev", shortname ="Leviticus", longname ="Leviticus", fileID="03", BBB="03"}},
            {04, new ParatextBookFileName{code="NUM", abbr = "Num", shortname ="Numbers", longname ="Numbers", fileID="04", BBB="04"}},
            {05, new ParatextBookFileName{code="DEU", abbr = "Deut", shortname ="Deuteronomy", longname ="Deuteronomy", fileID="05", BBB="05"}},
            {06, new ParatextBookFileName{code="JOS", abbr = "Josh", shortname ="Joshua", longname ="Joshua", fileID="06", BBB="06"}},
            {07, new ParatextBookFileName{code="JDG", abbr = "Judg", shortname ="Judges", longname ="Judges", fileID="07", BBB="07"}},
            {08, new ParatextBookFileName{code="RUT", abbr = "Ruth", shortname ="Ruth", longname ="Ruth", fileID="08", BBB="08"}},
            {09, new ParatextBookFileName{code="1SA", abbr = "1 Sam", shortname ="1 Samuel", longname ="1 Samuel", fileID="09", BBB="09"}},
            {10, new ParatextBookFileName{code="2SA", abbr = "2 Sam", shortname ="2 Samuel", longname ="2 Samuel", fileID="10", BBB="10"}},
            {11, new ParatextBookFileName{code="1KI", abbr = "1 Kgs", shortname ="1 Kings", longname ="1 Kings", fileID="11", BBB="11"}},
            {12, new ParatextBookFileName{code="2KI", abbr = "2 Kgs", shortname ="2 Kings", longname ="2 Kings", fileID="12", BBB="12"}},
            {13, new ParatextBookFileName{code="1CH", abbr = "1 Chr", shortname ="1 Chronicles", longname ="1 Chronicles", fileID="13", BBB="13"}},
            {14, new ParatextBookFileName{code="2CH", abbr = "2 Chr", shortname ="2 Chronicles", longname ="2 Chronicles", fileID="14", BBB="14"}},
            {15, new ParatextBookFileName{code="EZR", abbr = "Ezra", shortname ="Ezra", longname ="Ezra", fileID="15", BBB="15"}},
            {16, new ParatextBookFileName{code="NEH", abbr = "Neh", shortname ="Nehemiah", longname ="Nehemiah", fileID="16", BBB="16"}},
            {17, new ParatextBookFileName{code="EST", abbr = "Esth", shortname ="Esther", longname ="Esther", fileID="17", BBB="17"}},
            {18, new ParatextBookFileName{code="JOB", abbr = "Job", shortname ="Job", longname ="Job", fileID="18", BBB="18"}},
            {19, new ParatextBookFileName{code="PSA", abbr = "Ps(s)", shortname ="Psalms", longname ="Psalms", fileID="19", BBB="19"}},
            {20, new ParatextBookFileName{code="PRO", abbr = "Prov", shortname ="Proverbs", longname ="Proverbs", fileID="20", BBB="20"}},
            {21, new ParatextBookFileName{code="ECC", abbr = "Eccl", shortname ="Ecclesiastes", longname ="Ecclesiastes", fileID="21", BBB="21"}},
            {22, new ParatextBookFileName{code="SNG", abbr = "Song", shortname ="Song of Songs", longname ="The Song of Songs", fileID="22", BBB="22"}},
            {23, new ParatextBookFileName{code="ISA", abbr = "Isa", shortname ="Isaiah", longname ="Isaiah", fileID="23", BBB="23"}},
            {24, new ParatextBookFileName{code="JER", abbr = "Jer", shortname ="Jeremiah", longname ="Jeremiah", fileID="24", BBB="24"}},
            {25, new ParatextBookFileName{code="LAM", abbr = "Lam", shortname ="Lamentations", longname ="Lamentations", fileID="25", BBB="25"}},
            {26, new ParatextBookFileName{code="EZK", abbr = "Ezek", shortname ="Ezekiel", longname ="Ezekiel", fileID="26", BBB="26"}},
            {27, new ParatextBookFileName{code="DAN", abbr = "Dan", shortname ="Daniel", longname ="Daniel", fileID="27", BBB="27"}},
            {28, new ParatextBookFileName{code="HOS", abbr = "Hos", shortname ="Hosea", longname ="Hosea", fileID="28", BBB="28"}},
            {29, new ParatextBookFileName{code="JOL", abbr = "Joel", shortname ="Joel", longname ="Joel", fileID="29", BBB="29"}},
            {30, new ParatextBookFileName{code="AMO", abbr = "Amos", shortname ="Amos", longname ="Amos", fileID="30", BBB="30"}},
            {31, new ParatextBookFileName{code="OBA", abbr = "Obad", shortname ="Obadiah", longname ="Obadiah", fileID="31", BBB="31"}},
            {32, new ParatextBookFileName{code="JON", abbr = "Jonah", shortname ="Jonah", longname ="Jonah", fileID="32", BBB="32"}},
            {33, new ParatextBookFileName{code="MIC", abbr = "Micah", shortname ="Mic", longname ="Micah", fileID="33", BBB="33"}},
            {34, new ParatextBookFileName{code="NAM", abbr = "Nah", shortname ="Nahum", longname ="Nahum", fileID="34", BBB="34"}},
            {35, new ParatextBookFileName{code="HAB", abbr = "Hab", shortname ="Habakkuk", longname ="Habakkuk", fileID="35", BBB="35"}},
            {36, new ParatextBookFileName{code="ZEP", abbr = "Zeph", shortname ="Zephaniah", longname ="Zephaniah", fileID="36", BBB="36"}},
            {37, new ParatextBookFileName{code="HAG", abbr = "Hag", shortname ="Haggai", longname ="Haggai", fileID="37", BBB="37"}},
            {38, new ParatextBookFileName{code="ZEC", abbr = "Zech", shortname ="Zechariah", longname ="Zechariah", fileID="38", BBB="38"}},
            {39, new ParatextBookFileName{code="MAL", abbr = "Mal", shortname ="Malachi", longname ="Malachi", fileID="39", BBB="39"}},
            // 40 - intentionally omitted
            {41, new ParatextBookFileName{code="MAT", abbr = "Matt", shortname ="Matthew", longname ="Matthew", fileID="41", BBB="40"}},
            {42, new ParatextBookFileName{code="MRK", abbr = "Mark", shortname ="Mark", longname ="Mark", fileID="42", BBB="41"}},
            {43, new ParatextBookFileName{code="LUK", abbr = "Luke", shortname ="Luke", longname ="Luke", fileID="43", BBB="42"}},
            {44, new ParatextBookFileName{code="JHN", abbr = "John", shortname ="John", longname ="John", fileID="44", BBB="43"}},
            {45, new ParatextBookFileName{code="ACT", abbr = "Acts", shortname ="Acts", longname ="Acts", fileID="45", BBB="44"}},
            {46, new ParatextBookFileName{code="ROM", abbr = "Rom", shortname ="Romans", longname ="Romans", fileID="46", BBB="45"}},
            {47, new ParatextBookFileName{code="1CO", abbr = "1 Cor", shortname ="1 Corinthians", longname ="1 Corinthians", fileID="47", BBB="46"}},
            {48, new ParatextBookFileName{code="2CO", abbr = "2 Cor", shortname ="2 Corinthians", longname ="2 Corinthians", fileID="48", BBB="47"}},
            {49, new ParatextBookFileName{code="GAL", abbr = "Galatians", shortname ="Galatians", longname ="Galatians", fileID="49", BBB="48"}},
            {50, new ParatextBookFileName{code="EPH", abbr = "Eph", shortname ="Ephesians", longname ="Ephesians", fileID="50", BBB="49"}},
            {51, new ParatextBookFileName{code="PHP", abbr = "Phil", shortname ="Philippians", longname ="Philippians", fileID="51", BBB="50"}},
            {52, new ParatextBookFileName{code="COL", abbr = "Col", shortname ="Colossians", longname ="Colossians", fileID="52", BBB="51"}},
            {53, new ParatextBookFileName{code="1TH", abbr = "1 Thess", shortname ="1 Thessalonians", longname ="1 Thessalonians", fileID="53", BBB="52"}},
            {54, new ParatextBookFileName{code="2TH", abbr = "2 Thess", shortname ="2 Thessalonians", longname ="2 Thessalonians", fileID="54", BBB="53"}},
            {55, new ParatextBookFileName{code="1TI", abbr = "1 Tim", shortname ="1 Timothy", longname ="1 Timothy", fileID="55", BBB="54"}},
            {56, new ParatextBookFileName{code="2TI", abbr = "2 Tim", shortname ="2 Timothy", longname ="2 Timothy", fileID="56", BBB="55"}},
            {57, new ParatextBookFileName{code="TIT", abbr = "Titus", shortname ="Titus", longname ="Titus", fileID="57", BBB="56"}},
            {58, new ParatextBookFileName{code="PHM", abbr = "Phlm", shortname ="Philemon", longname ="Philemon", fileID="58", BBB="57"}},
            {59, new ParatextBookFileName{code="HEB", abbr = "Heb", shortname ="Hebrews", longname ="Hebrews", fileID="59", BBB="58"}},
            {60, new ParatextBookFileName{code="JAS", abbr = "Jas", shortname ="James", longname ="James", fileID="60", BBB="59"}},
            {61, new ParatextBookFileName{code="1PE", abbr = "1 Pet", shortname ="1 Peter", longname ="1 Peter", fileID="61", BBB="60"}},
            {62, new ParatextBookFileName{code="2PE", abbr = "2 Pet", shortname ="2 Peter", longname ="2 Peter", fileID="62", BBB="61"}},
            {63, new ParatextBookFileName{code="1JN", abbr = "1 John", shortname ="1 John", longname ="1 John", fileID="63", BBB="62"}},
            {64, new ParatextBookFileName{code="2JN", abbr = "2 John", shortname ="2 John", longname ="2 John", fileID="64", BBB="63"}},
            {65, new ParatextBookFileName{code="3JN", abbr = "3 John", shortname ="3 John", longname ="3 John", fileID="65", BBB="64"}},
            {66, new ParatextBookFileName{code="JUD", abbr = "Jude", shortname ="Jude", longname ="Jude", fileID="66", BBB="65"}},
            {67, new ParatextBookFileName{code="REV", abbr = "Rev", shortname ="Revelation", longname ="Revelation", fileID="67", BBB="66"}},

            {68, new ParatextBookFileName{code="TOB", abbr = "TOB", shortname ="Tobit", longname ="Tobit", fileID="68", BBB="67"}},
            {69, new ParatextBookFileName{code="JDT", abbr = "JDT", shortname ="Judith", longname ="Judith", fileID="69", BBB="68"}},
            {70, new ParatextBookFileName{code="ESG", abbr = "ESG", shortname ="Esther Greek", longname ="Esther Greek", fileID="70", BBB="69"}},
            {71, new ParatextBookFileName{code="WIS", abbr = "WIS", shortname ="Wisdom of Solomon", longname ="Wisdom of Solomon", fileID="70", BBB="70"}},

            {72, new ParatextBookFileName{code="SIR", abbr = "SIR", shortname ="Sirach (Ecclesiasticus)", longname ="Sirach (Ecclesiasticus)", fileID="72", BBB="71"}},
            {73, new ParatextBookFileName{code="BAR", abbr = "BAR", shortname ="Baruch", longname ="Baruch", fileID="73", BBB="72"}},
            {74, new ParatextBookFileName{code="LJE", abbr = "LJE", shortname ="Letter of Jeremiah", longname ="Letter of Jeremiah", fileID="74", BBB="73"}},
            {75, new ParatextBookFileName{code="S3Y", abbr = "S3Y", shortname ="Song of 3 Young Men", longname ="Song of 3 Young Men", fileID="75", BBB="74"}},
            {76, new ParatextBookFileName{code="SUS", abbr = "SUS", shortname ="Susanna", longname ="Susanna", fileID="76", BBB="75"}},
            {77, new ParatextBookFileName{code="BEL", abbr = "BEL", shortname ="Bel and the Dragon", longname ="Bel and the Dragon", fileID="77", BBB="76"}},
            {78, new ParatextBookFileName{code="1MA", abbr = "1MA", shortname ="1 Maccabees", longname ="1 Maccabees", fileID="78", BBB="77"}},
            {79, new ParatextBookFileName{code="2MA", abbr = "2MA", shortname ="2 Maccabees", longname ="2 Maccabees", fileID="79", BBB="78"}},
            {80, new ParatextBookFileName{code="3MA", abbr = "3MA", shortname ="3 Maccabees", longname ="3 Maccabees", fileID="80", BBB="79"}},
            {81, new ParatextBookFileName{code="4MA", abbr = "4MA", shortname ="4 Maccabees", longname ="4 Maccabees", fileID="81", BBB="80"}},

            {82, new ParatextBookFileName{code="1ES", abbr = "1ES", shortname ="1 Esdras (Greek)", longname ="1 Esdras (Greek)", fileID="82", BBB="81"}},
            {83, new ParatextBookFileName{code="2ES", abbr = "2ES", shortname ="2 Esdras (Latin)", longname ="2 Esdras (Latin)", fileID="83", BBB="82"}},
            {84, new ParatextBookFileName{code="MAN", abbr = "MAN", shortname ="Prayer of Manasseh", longname ="Prayer of Manasseh", fileID="84", BBB="83"}},
            {85, new ParatextBookFileName{code="PS2", abbr = "PS2", shortname ="Psalm 151", longname ="Psalm 151", fileID="85", BBB="84"}},
            {86, new ParatextBookFileName{code="ODA", abbr = "ODA", shortname ="Odes", longname ="Odes", fileID="86", BBB="85"}},
            {87, new ParatextBookFileName{code="PSS", abbr = "PSS", shortname ="Psalms of Solomon", longname ="Psalms of Solomon", fileID="87", BBB="86"}},
            {88, new ParatextBookFileName{code="JSA", abbr = "JSA", shortname ="Joshua A. *obsolete*", longname ="Joshua A. *obsolete*", fileID="88", BBB="87"}},
            {89, new ParatextBookFileName{code="JDB", abbr = "JDB", shortname ="Judges B. *obsolete*", longname ="Judges B. *obsolete*", fileID="89", BBB="88"}},
            {90, new ParatextBookFileName{code="TBS", abbr = "TBS", shortname ="Tobit S. *obsolete*", longname ="Tobit S. *obsolete*", fileID="90", BBB="89"}},
            {91, new ParatextBookFileName{code="SST", abbr = "SST", shortname ="Susanna Th. *obsolete*", longname ="Susanna Th. *obsolete*", fileID="91", BBB="90"}},

            {92, new ParatextBookFileName{code="DNT", abbr = "DNT", shortname ="Daniel Th. *obsolete*", longname ="Daniel Th. *obsolete*", fileID="92", BBB="91"}},
            {93, new ParatextBookFileName{code="BLT", abbr = "BLT", shortname ="Bel Th. *obsolete*", longname ="Bel Th. *obsolete*", fileID="93", BBB="92"}},
            {94, new ParatextBookFileName{code="XXA", abbr = "XXA", shortname ="Extra A", longname ="Extra A", fileID="94", BBB="93"}},
            {95, new ParatextBookFileName{code="XXB", abbr = "XXB", shortname ="Extra B", longname ="Extra B", fileID="95", BBB="94"}},
            {96, new ParatextBookFileName{code="XXC", abbr = "XXC", shortname ="Extra C", longname ="Extra C", fileID="96", BBB="95"}},
            {97, new ParatextBookFileName{code="XXD", abbr = "XXD", shortname ="Extra D", longname ="Extra D", fileID="97", BBB="96"}},
            {98, new ParatextBookFileName{code="XXE", abbr = "XXE", shortname ="Extra E", longname ="Extra E", fileID="98", BBB="97"}},
            {99, new ParatextBookFileName{code="XXF", abbr = "XXF", shortname ="Extra F", longname ="Extra F", fileID="99", BBB="98"}},
            {100, new ParatextBookFileName{code="XXG", abbr = "XXG", shortname ="Extra G", longname ="Extra G", fileID="100", BBB="99"}},
            {101, new ParatextBookFileName{code="FRT", abbr = "FRT", shortname ="Front Matter", longname ="Front Matter", fileID="101", BBB="100"}},

            {102, new ParatextBookFileName{code="BAK", abbr = "BAK", shortname ="Back Matter", longname ="Back Matter", fileID="102", BBB="101"}},
            {103, new ParatextBookFileName{code="OTH", abbr = "OTH", shortname ="Other Matter", longname ="Other Matter", fileID="103", BBB="102"}},
            {104, new ParatextBookFileName{code="3ES", abbr = "3ES", shortname ="3 Ezra *obsolete*", longname ="3 Ezra *obsolete*", fileID="104", BBB="103"}},
            {105, new ParatextBookFileName{code="EZA", abbr = "EZA", shortname ="Apocalypse of Ezra", longname ="Apocalypse of Ezra", fileID="105", BBB="104"}},
            {106, new ParatextBookFileName{code="5EZ", abbr = "5EZ", shortname ="5 Ezra (Latin Prologue)", longname ="5 Ezra (Latin Prologue)", fileID="106", BBB="105"}},
            {107, new ParatextBookFileName{code="6EZ", abbr = "6EZ", shortname ="6 Ezra (Latin Epilogue)", longname ="6 Ezra (Latin Epilogue)", fileID="107", BBB="106"}},
            {108, new ParatextBookFileName{code="INT", abbr = "INT", shortname ="Introduction", longname ="Introduction", fileID="108", BBB="107"}},
            {109, new ParatextBookFileName{code="CNC", abbr = "CNC", shortname ="Concordance", longname ="Concordance", fileID="109", BBB="108"}},
            {110, new ParatextBookFileName{code="GLO", abbr = "GLO", shortname ="Glossary", longname ="Glossary", fileID="110", BBB="109"}},
            {111, new ParatextBookFileName{code="TDX", abbr = "TDX", shortname ="Topical Index", longname ="Topical Index", fileID="111", BBB="110"}},

            {112, new ParatextBookFileName{code="NDX", abbr = "NDX", shortname ="Names Index", longname ="Names Index", fileID="112", BBB="111"}},
            {113, new ParatextBookFileName{code="DAG", abbr = "DAG", shortname ="Daniel Greek", longname ="Daniel Greek", fileID="113", BBB="112"}},
            {114, new ParatextBookFileName{code="PS3", abbr = "PS3", shortname ="Psalms 152-155", longname ="Psalms 152-155", fileID="114", BBB="113"}},
            {115, new ParatextBookFileName{code="2BA", abbr = "2BA", shortname ="2 Baruch (Apocalypse)", longname ="2 Baruch (Apocalypse)", fileID="115", BBB="114"}},
            {116, new ParatextBookFileName{code="LBA", abbr = "LBA", shortname ="Letter of Baruch", longname ="Letter of Baruch", fileID="116", BBB="115"}},
            {117, new ParatextBookFileName{code="JUB", abbr = "JUB", shortname ="Jubilees", longname ="Jubilees", fileID="117", BBB="116"}},
            {118, new ParatextBookFileName{code="ENO", abbr = "ENO", shortname ="Enoch", longname ="Enoch", fileID="118", BBB="117"}},
            {119, new ParatextBookFileName{code="1MQ", abbr = "1MQ", shortname ="1 Meqabyan", longname ="1 Meqabyan", fileID="119", BBB="118"}},
            {120, new ParatextBookFileName{code="2MQ", abbr = "2MQ", shortname ="2 Meqabyan", longname ="2 Meqabyan", fileID="120", BBB="119"}},
            {121, new ParatextBookFileName{code="3MQ", abbr = "3MQ", shortname ="3 Meqabyan", longname ="3 Meqabyan", fileID="121", BBB="120"}},

            {122, new ParatextBookFileName{code="REP", abbr = "REP", shortname ="Reproof (Proverbs 25-31)", longname ="Reproof (Proverbs 25-31)", fileID="122", BBB="121"}},
            {123, new ParatextBookFileName{code="4BA", abbr = "4BA", shortname ="4 Baruch (Rest of Baruch)", longname ="4 Baruch (Rest of Baruch)", fileID="123", BBB="122"}},
            {124, new ParatextBookFileName{code="LAO", abbr = "LAO", shortname ="Laodiceans", longname ="Laodiceans", fileID="124", BBB="123"}},


        };


        public static string ConvertVerseIdToReference(string verseId)
        {
            var book = Convert.ToInt32(verseId.Substring(0, 3));
            var chapter = Convert.ToInt32(verseId.Substring(3, 3));
            var verse = Convert.ToInt32(verseId.Substring(6, 3));

            if (BookNames.ContainsKey(book))
            {
                return BookNames[book].abbr + " " + chapter + ":" + verse;
            }
            else
            {
                return verseId;
            }
        }

    }
}
