using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Unidecode.NET;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{
 
        public record GetWhatIsThisWordByBcvQuery(BookChapterVerseViewModel bcv, string languageCode) : IRequest<RequestResult<List<MarbleResource>>>;

        public class GetWhatIsThisWordByBCVHandler : XmlReaderRequestHandler<GetWhatIsThisWordByBcvQuery,
            RequestResult<List<MarbleResource>>, List<MarbleResource>>
        {
            private BookChapterVerseViewModel _bcv;
            private string _languageCode;

            public GetWhatIsThisWordByBCVHandler(ILogger<GetWhatIsThisWordByBCVHandler> logger) : base(logger)
            {
                //no-op
            }


            protected override string ResourceName { get; set; } = "";

            public override Task<RequestResult<List<MarbleResource>>> Handle(GetWhatIsThisWordByBcvQuery request,
                CancellationToken cancellationToken)
            {
                _bcv = request.bcv;
                _languageCode = request.languageCode;

                ResourceName = GetFilenameFromMarbleBook(_bcv.BookNum);
                ResourceName = @"marble-indexes-full\MARBLELinks-" + ResourceName + ".XML";

                var queryResult = ValidateResourcePath(new List<MarbleResource>());
                if (queryResult.Success == false)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the MARBLE databases for data with verseId : '{_bcv.VerseLocationId}'");
                    return Task.FromResult(queryResult);
                }

                try
                {
                    queryResult.Data = LoadXmlAndProcessData(null);
                }
                catch (Exception ex)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the '{ResourceName}' database for data with verseId : '{_bcv.VerseLocationId}'",
                        ex);
                }

                return Task.FromResult(queryResult);
            }

            protected override List<MarbleResource> ProcessData()
            {
                return GetLemmaListFromMarbleIndexes(ResourcePath, _bcv, _languageCode);
            }


            /// <summary>
            /// Function takes in Marble filename for the index and the BCV reference
            /// Returns the list of MARBLE resources for that verse
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="bcv"></param>
            /// <returns></returns>
            private List<MarbleResource> GetLemmaListFromMarbleIndexes(string filename, BookChapterVerseViewModel bcv,
                string languageCode)
            {
                Dictionary<string, LexicalLookUp> SDBG = new Dictionary<string, LexicalLookUp>();
                Dictionary<string, LexicalLookUp> SDBH = new Dictionary<string, LexicalLookUp>();

                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                //XmlNodeList prop = doc.SelectNodes($"//verse[@chapter='{bcv.BookNum}' and @pubnumber='{bcv.Verse}']");
                string bbbcccvvv = bcv.VerseLocationId.PadLeft(9, '0');
                XmlNodeList prop =
                    doc.SelectNodes($"//MARBLELink[starts-with(@Id,'{bbbcccvvv}')]/LexicalLinks/LexicalLink");

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

                List<MarbleResource> resourceList = new List<MarbleResource>();

                // load up the resources
                int ID = 0;
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
                        string fileName = Path.Combine(startupPath,
                            $@"resources\{lexicalLink.DictionaryName}\{item.DictionaryName}.xml");

                        if (!File.Exists(fileName))
                        {
                            return new List<MarbleResource>();
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

                                    //if (iSenseID - 1 == ilinkSenseID)
                                    //{
                                    XmlNodeList nodeEntry = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]");
                                    MarbleResource marbleResource = new MarbleResource();

                                    if (nodeEntry.Count > 0)
                                    {
                                        // get the domain
                                        var nodes = doc.SelectNodes(
                                            $"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXDomains/LEXDomain");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.Domains += nodeInner.InnerText + ", ";
                                        }

                                        // get the subdomain
                                        nodes = doc.SelectNodes(
                                            $"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSubDomains");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.SubDomains += nodeInner.InnerText + ", ";
                                        }

                                        // get the definition long
                                        nodes = doc.SelectNodes(
                                            $"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='{languageCode}']/DefinitionLong");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.DefinitionLong += nodeInner.InnerText + ", ";
                                        }

                                        // get the definition short
                                        nodes = doc.SelectNodes(
                                            $"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='{languageCode}']/DefinitionShort");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.DefinitionShort += nodeInner.InnerText + ", ";
                                        }

                                        // get the glosses
                                        nodes = doc.SelectNodes(
                                            $"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='{languageCode}']/Glosses/Gloss");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.Glosses += nodeInner.InnerText + ", ";
                                        }

                                        // get the comments
                                        nodes = doc.SelectNodes(
                                            $"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"].Value}]/LEXSenses/LEXSense[@LanguageCode='{languageCode}']/Comments");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.Comment += nodeInner.InnerText + ", ";
                                        }

                                        // get the Strong
                                        nodes = doc.SelectNodes($"//StrongCodes");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.Strong += nodeInner.InnerText + ", ";
                                        }

                                        // get the PoS
                                        nodes = doc.SelectNodes($"//BaseForms/BaseForm/PartsOfSpeech/PartOfSpeech");
                                        foreach (XmlNode nodeInner in nodes)
                                        {
                                            marbleResource.PoS += nodeInner.InnerText + ", ";
                                        }

                                        // remove the commas
                                        if (marbleResource.Comment != "")
                                        {
                                            marbleResource.Comment = marbleResource.Comment.Trim();
                                            marbleResource.Comment = marbleResource.Comment.Substring(0, marbleResource.Comment.Length - 1);
                                        }

                                        if (marbleResource.DefinitionLong != "")
                                        {
                                            marbleResource.DefinitionLong = marbleResource.DefinitionLong.Trim();
                                            marbleResource.DefinitionLong =
                                                marbleResource.DefinitionLong.Substring(0, marbleResource.DefinitionLong.Length - 1);
                                        }

                                        if (marbleResource.DefinitionShort != "")
                                        {
                                            marbleResource.DefinitionShort = marbleResource.DefinitionShort.Trim();
                                            marbleResource.DefinitionShort =
                                                marbleResource.DefinitionShort.Substring(0, marbleResource.DefinitionShort.Length - 1);
                                        }

                                        if (marbleResource.Domains != "")
                                        {
                                            marbleResource.Domains = marbleResource.Domains.Trim();
                                            marbleResource.Domains = marbleResource.Domains.Substring(0, marbleResource.Domains.Length - 1);
                                        }

                                        if (marbleResource.SubDomains != "")
                                        {
                                            marbleResource.SubDomains = marbleResource.SubDomains.Trim();
                                            marbleResource.SubDomains = marbleResource.SubDomains.Substring(0, marbleResource.SubDomains.Length - 1);
                                        }

                                        if (marbleResource.Glosses != "")
                                        {
                                            marbleResource.Glosses = marbleResource.Glosses.Trim();
                                            marbleResource.Glosses = marbleResource.Glosses.Substring(0, marbleResource.Glosses.Length - 1);
                                        }

                                        if (marbleResource.Strong != "")
                                        {
                                            marbleResource.Strong = marbleResource.Strong.Trim();
                                            marbleResource.Strong = marbleResource.Strong.Substring(0, marbleResource.Strong.Length - 1);
                                        }

                                        marbleResource.SenseId = senseID;
                                        marbleResource.PoS = marbleResource.PoS.Trim();
                                        marbleResource.Word = lexicalLink.Lemma;

                                        marbleResource.LogosRef = Uri.EscapeDataString(marbleResource.Word).Replace('%', '$');

                                        // TODO will need to check if this is enough for Hebrew too
                                        if (bcv.BookNum < 40)
                                        {
                                            // hebrew word
                                            marbleResource.WordTransliterated = Hebrew.Transliterate(lexicalLink.Lemma);
                                        }
                                        else
                                        {
                                            // greek word
                                            marbleResource.WordTransliterated = lexicalLink.Lemma.Unidecode();
                                        }

                                        marbleResource.Id = ID;

                                        if (iSenseID - 1 == ilinkSenseID)
                                        {
                                            marbleResource.IsSense = true;
                                        }

                                        resourceList.Add(marbleResource);
                                    }

                                    //}

                                }
                            }
                        }
                    }

                    ID++;

                }

                // group by to get the totals

                var groups = resourceList.GroupBy(p => p.Id);
                foreach (var group in groups)
                {
                    int count = group.Count();

                    foreach (var item in group)
                    {
                        item.TotalSenses = count;
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
