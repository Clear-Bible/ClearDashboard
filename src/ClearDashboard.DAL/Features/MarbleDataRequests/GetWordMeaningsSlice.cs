using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;
using Unidecode.NET;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{

    public record GetWordMeaningsQuery(BookChapterVerseViewModel bcv, string languageCode, string word, List<SemanticDomainLookup> lookup) : IRequest<RequestResult<ObservableCollection<Senses>>>;

    public class GetWordMeaningsSliceHandler : XmlReaderRequestHandler<GetWordMeaningsQuery,
        RequestResult<ObservableCollection<Senses>>, ObservableCollection<Senses>>
    {
        private readonly ILogger<GetWordMeaningsSliceHandler> _logger;
        private BookChapterVerseViewModel _bcv;
        private string _languageCode;
        private string _word;
        private List<SemanticDomainLookup> _lookup;
        private ObservableCollection<Senses> Senses = new();

        public GetWordMeaningsSliceHandler(ILogger<GetWordMeaningsSliceHandler> logger) : base(logger)
        {
            _logger = logger;
            //no-op
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<ObservableCollection<Senses>>> Handle(GetWordMeaningsQuery request,
            CancellationToken cancellationToken)
        {
            _bcv = request.bcv;
            _languageCode = request.languageCode;
            _word = request.word;
            _lookup = request.lookup;

            if (_lookup is null)
            {
                var error = new RequestResult<ObservableCollection<Senses>>();
                error.Success = false;
                error.Message = "Semantic Domain Lookup is null";

                return Task.FromResult(error);
            }

            var result = _lookup.FirstOrDefault(x => x.Word.Equals(_word));
            if (result is null)
            {
                var error = new RequestResult<ObservableCollection<Senses>>();
                error.Success = false;
                error.Message = $"Semantic Domain Lookup does not contain word: {_word}";

                return Task.FromResult(error);
            }


            if (_bcv.BookNum < 40)
            {
                ResourceName = @$"SDBH\{result.FileName}.XML";
            }
            else
            {
                ResourceName = @$"SDBG\{result.FileName}.XML";
            }
            
            var queryResult = ValidateResourcePath(new ObservableCollection<Senses>());
            if (queryResult.Success == false)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the MARBLE databases for data with verseId : '{_bcv.BBBCCCVVV}'");
                return Task.FromResult(queryResult);
            }

            try
            {
                queryResult.Data = FileLoadXmlAndProcessData();
            }
            catch (Exception ex)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the '{ResourceName}' database for data with verseId : '{_bcv.BBBCCCVVV}'",
                    ex);
            }

            return Task.FromResult(queryResult);
        }

        protected override ObservableCollection<Senses> ProcessData()
        {
            var entry = ResourceElements.Elements("Lexicon_Entry")
                .Where(x => x.Attribute("Lemma").Value.Equals(_word))
                .ToList();

            GatherDonutGraphSenses(entry);

            return Senses;
        }

        private void GatherDonutGraphSenses(List<XElement> entry)
        {
            Senses.Clear();

            var strongCodes = entry.Elements("StrongCodes")
                .Elements("Strong")
                .Select(x => x.Value)
                .ToList();

            var baseform = entry.Elements("BaseForms")
                .Elements("BaseForm");

            var partsOfSpeech = baseform.Elements("PartsOfSpeech")
                .Elements("PartOfSpeech")
                .Select(x => x.Value)
                .ToList();

            var relatedLemmas = baseform.Elements("RelatedLemmas")
                .Elements("RelatedLemma")
                .Select(x => x.Value)
                .ToList();

            // check each lemma to see if it is in the CSV list
            List<RelatedLemma> relatedLemmasList = CheckIfInCSV(relatedLemmas);


            var lexMeanings = entry.Elements("BaseForms")
                .Elements("BaseForm")
                .Elements("LEXMeanings")
                .Elements("LEXMeaning")
                .ToList();

            var totalVerses = 0;

            ObservableCollection<Senses> temp = new();

            foreach (var meaning in lexMeanings)
            {
                var totalSenseVerseCount = meaning.Elements("LEXReferences")
                    .Elements("LEXReference")
                    .ToList();
                Console.WriteLine($"Total Sense Verses: {totalSenseVerseCount.Count}");
                totalVerses += totalSenseVerseCount.Count;

                List<string> verses = new();
                foreach (var verse in totalSenseVerseCount)
                {
                    var verseRef = VerseHelper.ConvertVerseIdToReference(verse.Value);
                    verses.Add(verseRef);
                }


                var senses = meaning.Elements("LEXSenses").ToList();


                var lexSense = senses.Elements("LEXSense")
                    .FirstOrDefault(x => x.Attribute("LanguageCode").Value.Equals("en"));

                var definitionLong = lexSense.Element("DefinitionLong").Value;
                var definationShort = lexSense.Element("DefinitionShort").Value;


                var glosses = lexSense.Elements("Glosses")
                    .Elements("Gloss")
                    .Select(x => x.Value)
                    .ToList();

                var domains = meaning.Element("LEXDomains")
                    .Elements("LEXDomain")
                    .Select(x => x.Value)
                    .ToList();

                var subDomains = meaning.Element("LEXSubDomains")
                    .Elements("LEXSubDomain")
                    .Select(x => x.Value)
                    .ToList();

                var synonyms = meaning.Element("LEXSynonyms")
                    .Elements("LEXSynonym")
                    .Select(x => x.Value)
                    .ToList();

                var antonyms = meaning.Element("LEXAntonyms")
                    .Elements("LEXAntonym")
                    .Select(x => x.Value)
                    .ToList();

                var collocations = meaning.Element("LEXCollocations")
                    .Elements("LEXCollocation")
                    .Select(x => x.Value)
                    .ToList();

                var valencies = meaning.Element("LEXValencies")
                    .Elements("LEXValency")
                    .Select(x => x.Value)
                    .ToList();

                var coreDomains = meaning.Element("LEXCoreDomains")
                    .Elements("LEXCoreDomain")
                    .Select(x => x.Value)
                    .ToList();

                var contextualMeaning = meaning.Element("CONMeanings")
                    .Elements("ContextualMeaning").ToList();

                //List<ContextualMeaning> contextualMeanings = new();
                ObservableCollection<TreeNode> root = new();
                foreach (var conMeaning in contextualMeaning)
                {
                    var meaningId = conMeaning.Attribute("Id").Value.Substring(9);

                    var conForms = conMeaning.Elements("CONForms")
                        .Elements("CONForm")
                        .Select(x => x.Value)
                        .ToList();

                    var conCollocations = conMeaning.Elements("CONCollocations")
                        .Elements("CONCollocation")
                        .Select(x => x.Value)
                        .ToList();

                    var conValencies = conMeaning.Elements("CONValencies")
                        .Elements("CONValency")
                        .Select(x => x.Value)
                        .ToList();

                    var conReferences = conMeaning.Elements("CONReferences")
                        .Elements("CONReference")
                        .Select(x => x.Value)
                        .ToList();

                    for (var index = 0; index < conReferences.Count; index++)
                    {
                        var verse = conReferences[index];
                        conReferences[index] = VerseHelper.ConvertVerseIdToReference(verse);
                    }


                    var conDomains = conMeaning.Elements("CONDomains")
                        .Elements("CONDomain")
                        .Select(x => x.Value)
                        .ToList();

                    ObservableCollection<TreeNode> memberNode = new();




                    // Get the sense definition for this contextual meaning
                    var conSenses = conMeaning.Elements("CONSenses").ToList();
                    var sensesList = conSenses.Elements("CONSense").ToList();
                    var english = sensesList.Where(x =>
                    {
                        if (x.Attribute("LanguageCode").Value == "en")
                        {
                            return true;
                        };
                        return false;
                    });
                    if (english.Any())
                    {
                        var DefinitionLong = english.Elements("DefinitionLong").FirstOrDefault()?.Value;
                        var DefinitionShort = english.Elements("DefinitionShort").FirstOrDefault()?.Value;
                        var Glosses = english.Elements("Glosses")
                            .Elements("Gloss")
                            .Select(x => x.Value)
                            .ToList();

                        if (DefinitionLong != "" || DefinitionShort != "")
                        {
                            Console.WriteLine();
                        }

                        memberNode = AddToTreeNode(Glosses, "Glosses", memberNode);
                    }

                    memberNode = AddToTreeNode(conForms, "Forms", memberNode);
                    memberNode = AddToTreeNode(conDomains, "Domains", memberNode);
                    memberNode = AddToTreeNode(conCollocations, "Collocations", memberNode);
                    memberNode = AddToTreeNode(conValencies, "Valencies", memberNode);
                    memberNode = AddToTreeNode(conReferences, "References", memberNode);

                    root.Add(new TreeNode
                    {
                        NodeName = meaningId,
                        ChildNodes = memberNode
                    });
                }



                Senses sense = new Senses()
                {
                    Sense = String.Join("; ", glosses),
                    DescriptionLong = definitionLong,
                    DescriptionShort = definationShort,
                    Glosses = glosses,
                    VerseTotal = totalSenseVerseCount.Count,
                    Verses = verses,
                    CoreDomains = coreDomains,
                    Domains = domains,
                    SubDomains = subDomains,
                    Synonyms = synonyms,
                    Antonyms = antonyms,
                    ManuscriptWord = _word,
                    Collocations = collocations,
                    Valencies = valencies,
                    StrongCodes = strongCodes,
                    RelatedLemmas = relatedLemmas,
                    RelatedLemmaList = relatedLemmasList,
                    PartsOfSpeech = partsOfSpeech,
                    AlphabetTreeNodes = root,
                    //LexicalLinks = _lexicalLinks,
                    //SelectedLexicalLink = _lexicalLinks[0],
                };

                temp.Add(sense);
            }

            //Console.WriteLine($"Total Overall Verses: {totalVerses}");
            var sortedList = temp.OrderByDescending(x => x.VerseTotal).ToList();
            foreach (var item in sortedList)
            {
                // add in percents
                item.VersePercent = (double)item.VerseTotal / totalVerses * 100;

                Senses.Add(item);
            }
        }


        private List<RelatedLemma> CheckIfInCSV(List<string> relatedLemmas)
        {
            var temp = new List<RelatedLemma>();
            if (_lookup is null)
            {
                foreach (var lemma in relatedLemmas)
                {
                    temp.Add(new RelatedLemma
                    {
                        Lemma = lemma,
                        IsAvailable = true
                    });
                }
                return temp;
            }

            foreach (var relatedLemma in relatedLemmas)
            {
                var result = _lookup.FirstOrDefault(x => x.Word.Equals(relatedLemma));
                if (result is null)
                {
                    temp.Add(new RelatedLemma
                    {
                        Lemma = relatedLemma,
                        IsAvailable = false
                    });
                }
                else
                {
                    temp.Add(new RelatedLemma
                    {
                        Lemma = relatedLemma,
                        IsAvailable = true
                    });
                }
            }
            return temp;
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
            string bbbcccvvv = bcv.BBBCCCVVV.PadLeft(9, '0');
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

                            try
                            {
                                //Debug.WriteLine(xmlSnippet);
                                foreach (XmlNode node in nodeList)
                                {
                                    string senseID = node.Attributes["Id"].Value;
                                    if (senseID.Length > 9)
                                    {
                                        senseID = senseID.Substring(9, 3);
                                    }
                                    else
                                    {
                                        senseID = "0";
                                    }


                                    int iSenseID = Convert.ToInt32(senseID);
                                    int ilinkSenseID = Convert.ToInt32(lexicalLink.SenseID);

                                    //if (iSenseID - 1 == ilinkSenseID)
                                    //{
                                    XmlNodeList nodeEntry = doc.SelectNodes($"//LEXMeanings/LEXMeaning[@Id={node.Attributes["Id"]?.Value}]")!;
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
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.Message);
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


        private static ObservableCollection<TreeNode> AddToTreeNode<T>(List<T> list, string title, ObservableCollection<TreeNode> treeRoot)
        {
            if (list.Count == 0)
            {
                return treeRoot;
            }

            TreeNode node = new TreeNode
            {
                NodeName = title,
                ChildNodes = new(),
                IsExpanded = true,
            };

            foreach (var item in list)
            {
                node.ChildNodes.Add(new TreeNode
                {
                    NodeName = item.ToString(),
                    ChildNodes = new(),
                    IsExpanded = true,
                });
            }

            treeRoot.Add(node);

            return treeRoot;
        }


    }

}
