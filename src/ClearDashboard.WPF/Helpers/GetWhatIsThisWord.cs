using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClearDashboard.Common.Models;
using Unidecode.NET;

namespace ClearDashboard.Wpf.Helpers
{
    public class GetWhatIsThisWord
    {

        public List<MARBLEresource> GetSemanticDomainData(BookChapterVerse bcv)
        {
            // Load up NIV84+ for this verse
            string filename = GetFilenameFromMarbleBook(bcv.BookNum);

            if (filename == "")
            {
                return new List<MARBLEresource>();
            }

            filename = "MARBLELinks-" + filename + ".XML";

            // get the XML file to load
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            string linksFilePath = Path.Combine(startupPath, @"resources\marble-indexes-full\", filename);

            if (!File.Exists(linksFilePath))
            {
                return new List<MARBLEresource>();
            }


            List<MARBLEresource> mr = GetLemmaListFromMarbleIndexes(linksFilePath, bcv);

            return mr;
        }

        /// <summary>
        /// Function takes in Marble filename for the index and the BCV reference
        /// Returns the list of MARBLE resources for that verse
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bcv"></param>
        /// <returns></returns>
        private List<MARBLEresource> GetLemmaListFromMarbleIndexes(string filename, BookChapterVerse bcv)
        {
            Dictionary<string, LexicalLookUp> SDBG = new Dictionary<string, LexicalLookUp>();
            Dictionary<string, LexicalLookUp> SDBH = new Dictionary<string, LexicalLookUp>();

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            //XmlNodeList prop = doc.SelectNodes($"//verse[@chapter='{bcv.BookNum}' and @pubnumber='{bcv.Verse}']");
            string bbbcccvvv = bcv.GetVerseId().PadLeft(9, '0');
            XmlNodeList prop = doc.SelectNodes($"//MARBLELink[starts-with(@Id,'{bbbcccvvv}')]/LexicalLinks/LexicalLink");

            // loop through the XML getting al the LexicalLinks for this verse
            List<MarbleIndexLinks> lexicalLinks = new List<MarbleIndexLinks>();
            foreach (XmlNode item in prop)
            {
                var link = item.InnerText;

                string[] s = link.Split(':');

                if (s.Length == 3)
                {
                    lexicalLinks.Add(new MarbleIndexLinks
                    {
                        DictionaryName = s[0],
                        SenseID = s[2],
                        Lemma = s[1],
                    });
                }
            }

            List<MARBLEresource> resourceList = new List<MARBLEresource>();

            // load up the resources
            foreach (var lexicalLink in lexicalLinks)
            {
                LexicalLookUp item = null;

                // get the database name
                if (lexicalLink.DictionaryName == "SDBG")
                {
                    if (SDBG.Count == 0)
                    {
                        SDBG = ReadInLookupList("SDBG");
                    }

                    if (SDBG.ContainsKey(lexicalLink.Lemma))
                    {
                        item = SDBG[lexicalLink.Lemma];
                    }

                }
                else
                {
                    if (SDBH.Count == 0)
                    {
                        SDBH = ReadInLookupList("SDBH");
                    }

                    if (SDBH.ContainsKey(lexicalLink.Lemma))
                    {
                        item = SDBH[lexicalLink.Lemma];
                    }
                }

                if (item != null)
                {


                    // read in the XML file and go to the line number
                    // read in the lookup file and put into a dictionary lookup
                    string startupPath = AppDomain.CurrentDomain.BaseDirectory;
                    string fileName = Path.Combine(startupPath, $@"resources\{lexicalLink.DictionaryName}\{item.DictionaryName}.xml");

                    if (!File.Exists(fileName))
                    {
                        return new List<MARBLEresource>();
                    }

                    // read the file into a string array
                    string[] lines = File.ReadAllLines(fileName);
                    if (item.LineNum < lines.Length)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(lines[item.LineNum - 1]);

                        for (int i = item.LineNum; i < lines.Length; i++)
                        {
                            if (lines[i].Trim().ToLower() == "</lexicon_entry>")
                            {
                                sb.AppendLine(lines[i]);
                                break;
                            }
                            else
                            {
                                sb.AppendLine(lines[i]);
                            }
                        }

                        string xmlSnippet = sb.ToString();

                        if (xmlSnippet.Length > 0)
                        {
                            doc = new XmlDocument();
                            doc.LoadXml(xmlSnippet);

                            XmlNodeList nodeList = doc.SelectNodes($"//LEXMeanings/LEXMeaning");

                            foreach (XmlNode node in nodeList)
                            {
                                string senseID = node.Attributes["Id"].Value;
                                senseID = senseID.Substring(9, 3);

                                int iSenseID = Convert.ToInt32(senseID);
                                int ilinkSenseID = Convert.ToInt32(lexicalLink.SenseID);

                                if (iSenseID - 1 == ilinkSenseID)
                                {
                                    XmlNodeList nodeEntry = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]");
                                    MARBLEresource mr = new MARBLEresource();

                                    if (nodeEntry.Count > 0)
                                    {
                                        // get the domain
                                        var nodes = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXDomains/LEXDomain");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.Domains += nodeInner.InnerText + ", ";
                                        }

                                        // get the subdomain
                                        nodes = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSubDomains");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.SubDomains += nodeInner.InnerText + ", ";
                                        }

                                        // get the definition long
                                        nodes = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='en']/DefinitionLong");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.DefinitionLong += nodeInner.InnerText + ", ";
                                        }

                                        // get the definition short
                                        nodes = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='en']/DefinitionShort");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.DefinitionShort += nodeInner.InnerText + ", ";
                                        }

                                        // get the glosses
                                        nodes = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='en']/Glosses/Gloss");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.Glosses += nodeInner.InnerText + ", ";
                                        }

                                        // get the comments
                                        nodes = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='en']/Comments");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.Comment += nodeInner.InnerText + ", ";
                                        }

                                        // get the Strong
                                        nodes = doc.SelectNodes($"//StrongCodes");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.Strong += nodeInner.InnerText + ", ";
                                        }

                                        // get the PoS
                                        nodes = doc.SelectNodes($"//BaseForms/BaseForm/PartsOfSpeech/PartOfSpeech");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            mr.PoS += nodeInner.InnerText + ", ";
                                        }

                                        // remove the commas
                                        if (mr.Comment != "")
                                        {
                                            mr.Comment = mr.Comment.Trim();
                                            mr.Comment = mr.Comment.Substring(0, mr.Comment.Length - 1);
                                        }
                                        if (mr.DefinitionLong != "")
                                        {
                                            mr.DefinitionLong = mr.DefinitionLong.Trim();
                                            mr.DefinitionLong = mr.DefinitionLong.Substring(0, mr.DefinitionLong.Length - 1);
                                        }
                                        if (mr.DefinitionShort != "")
                                        {
                                            mr.DefinitionShort = mr.DefinitionShort.Trim();
                                            mr.DefinitionShort = mr.DefinitionShort.Substring(0, mr.DefinitionShort.Length - 1);
                                        }
                                        if (mr.Domains != "")
                                        {
                                            mr.Domains = mr.Domains.Trim();
                                            mr.Domains = mr.Domains.Substring(0, mr.Domains.Length - 1);
                                        }
                                        if (mr.SubDomains != "")
                                        {
                                            mr.SubDomains = mr.SubDomains.Trim();
                                            mr.SubDomains = mr.SubDomains.Substring(0, mr.SubDomains.Length - 1);
                                        }
                                        if (mr.Glosses != "")
                                        {
                                            mr.Glosses = mr.Glosses.Trim();
                                            mr.Glosses = mr.Glosses.Substring(0, mr.Glosses.Length - 1);
                                        }
                                        if (mr.Strong != "")
                                        {
                                            mr.Strong = mr.Strong.Trim();
                                            mr.Strong = mr.Strong.Substring(0, mr.Strong.Length - 1);
                                        }
                                        mr.SenseId = senseID;
                                        mr.PoS = mr.PoS.Trim();
                                        mr.Word = lexicalLink.Lemma;

                                        mr.LogosRef = Uri.EscapeDataString(mr.Word).Replace('%', '$');

                                        // TODO will need to check if this is enough for Hebrew too
                                        if (bcv.BookNum < 40)
                                        {
                                            // hebrew word
                                            mr.WordTransliterated = Hebrew.Transliterate(lexicalLink.Lemma);
                                        }
                                        else
                                        {
                                            // greek word
                                            mr.WordTransliterated = lexicalLink.Lemma.Unidecode();
                                        }

                                        resourceList.Add(mr);
                                    }

                                }
                            }
                        }
                    }
                }


            }


            return resourceList;
        }


        /// <summary>
        /// Function to read in a CSV lookup for the SDBG/H databases that have pointers to where in
        /// the XML files the lemma is located.
        /// </summary>
        /// <param name="sDictionaryName"></param>
        /// <returns></returns>
        private Dictionary<string, LexicalLookUp> ReadInLookupList(string sDictionaryName)
        {
            Dictionary<string, LexicalLookUp> lookup = new Dictionary<string, LexicalLookUp>();

            // read in the lookup file and put into a dictionary lookup
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = Path.Combine(startupPath, $@"resources\{sDictionaryName}\lemmaLU.csv");

            using (StreamReader reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] s = line.Split(',');
                    if (s.Length == 3)
                    {
                        lookup.Add(s[0], new LexicalLookUp
                        {
                            DictionaryName = s[1],
                            Lemma = s[0],
                            LineNum = Convert.ToInt32(s[2])
                        });
                    }
                }
            }

            return lookup;
        }

        /// <summary>
        /// A function that takes in a book number and translatest that to a book name
        /// </summary>
        /// <param name="bookNum"></param>
        /// <returns></returns>
        private string GetFilenameFromMarbleBook(int bookNum)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add(1, "GEN");
            dic.Add(2, "EXO");
            dic.Add(3, "LEV");
            dic.Add(4, "NUM");
            dic.Add(5, "DEU");
            dic.Add(6, "JOS");
            dic.Add(7, "JDG");
            dic.Add(8, "RUT");
            dic.Add(9, "1SA");
            dic.Add(10, "2SA");
            dic.Add(11, "1KI");
            dic.Add(12, "2KI");
            dic.Add(13, "1CH");
            dic.Add(14, "2CH");
            dic.Add(15, "EZR");
            dic.Add(16, "NEH");
            dic.Add(17, "EST");
            dic.Add(18, "JOB");
            dic.Add(19, "PSA");
            dic.Add(20, "PRO");
            dic.Add(21, "ECC");
            dic.Add(22, "SNG");
            dic.Add(23, "ISA");
            dic.Add(24, "JER");
            dic.Add(25, "LAM");
            dic.Add(26, "EZK");
            dic.Add(27, "DAN");
            dic.Add(28, "HOS");
            dic.Add(29, "JOL");
            dic.Add(30, "AMO");
            dic.Add(31, "OBA");
            dic.Add(32, "JON");
            dic.Add(33, "MIC");
            dic.Add(34, "NAM");
            dic.Add(35, "HAB");
            dic.Add(36, "ZEP");
            dic.Add(37, "HAG");
            dic.Add(38, "ZEC");
            dic.Add(39, "MAL");
            dic.Add(40, "MAT");
            dic.Add(41, "MRK");
            dic.Add(42, "LUK");
            dic.Add(43, "JHN");
            dic.Add(44, "ACT");
            dic.Add(45, "ROM");
            dic.Add(46, "1CO");
            dic.Add(47, "2CO");
            dic.Add(48, "GAL");
            dic.Add(49, "EPH");
            dic.Add(50, "PHP");
            dic.Add(51, "COL");
            dic.Add(52, "1TH");
            dic.Add(53, "2TH");
            dic.Add(54, "1TI");
            dic.Add(55, "2TI");
            dic.Add(56, "TIT");
            dic.Add(57, "PHM");
            dic.Add(58, "HEB");
            dic.Add(59, "JAS");
            dic.Add(60, "1PE");
            dic.Add(61, "2PE");
            dic.Add(62, "1JN");
            dic.Add(63, "2JN");
            dic.Add(64, "3JN");
            dic.Add(65, "JUD");
            dic.Add(66, "REV");

            if (dic.ContainsKey(bookNum))
            {
                return dic[bookNum];
            }

            return "";
        }
    }
}
