using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ClearDashboard.Common.Models;

namespace ClearDashboard.Wpf.ViewModels
{
    public class PinsViewModel : ToolViewModel
    {

        #region Member Variables

        private string _paratextInstallPath = "";

        private string _paratextProjectPath = "";
        public string ParatextProjectPath
        {
            get => _paratextProjectPath;
            set
            {
                _paratextProjectPath = value;
                NotifyOfPropertyChange(() => ParatextProjectPath);
            }
        }

        private int _projectSelectedId = -1;
        public int ProjectSelectedId
        {
            get => _projectSelectedId;
            set
            {
                _projectSelectedId = value;
                if (_projectSelectedId > 0)
                {
                    // combobox changed
                    string proj = Projects[_projectSelectedId];
                    ProjectChangedEvent(proj);
                }
                NotifyOfPropertyChange(() => ProjectSelectedId);
            }
        }

        public List<string> lexiconfiles = new List<string>();
        public List<string> bookfiles = new List<string>();
        public List<string> projects = new List<string>();
        public List<tabledt> thedata = new List<tabledt>();

        private List<tabledt> _thedata;
        public List<tabledt> TheData
        {
            get => _thedata;
            set
            {
                _thedata = value;
                NotifyOfPropertyChange(() => TheData);
            }
        }

        public ICollectionView GridCollectionView { get; set; }


        public string[] pt; // project biblical terms
        public string[] at; // key biblical terms
        public string[] s; // lexicon data

        public List<string> pbt = new List<string>();
        public List<string> abt = new List<string>();
        public List<string> LexiconToRead;
        public string language = "";

        private List<string> _projects = new List<string>();
        public List<string> Projects
        {
            get => _projects;
            set
            {
                _projects = value;

                NotifyOfPropertyChange(() => Projects);
            }
        }

        private string _filterString;
        public string FilterString
        {
            get
            {
                return _filterString;
            }
            set
            {
                _filterString = value;
                NotifyOfPropertyChange(() => FilterString);

                if (TheData != null)
                {
                    GridCollectionView.Refresh();
                }
            }
        }


        private List<tabledt> _gridData;
        public List<tabledt> GridData
        {
            get => _gridData;
            set
            {
                _gridData = value;
                NotifyOfPropertyChange(() => GridData);
            }
        }


        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties

        #region Constructor
        public PinsViewModel()
        {
            this.Title = "⍒ PINS";
            this.ContentId = "PINS";
        }

        #endregion //Constructor

        #region Methods

        private void PT_version_selected()
        {
            // Find all "Lexicon.xml" files in ProjectDir
            lexiconfiles = Directory.GetFiles(_paratextProjectPath, "Lexicon.xml", SearchOption.AllDirectories).ToList();
            bookfiles = Directory.GetFiles(_paratextProjectPath, "Interlinear_*.xml", SearchOption.AllDirectories).ToList();

            // Load projects with lexicons into the Project Dropdown field
            Projects = PathToProjName(lexiconfiles);
        }

        private List<string> PathToProjName(List<string> projlist)
        {
            projlist = projlist
                .Select(s => s[_paratextProjectPath.Length..])   // remove Projects root path
                .Select(s => s[..s.IndexOf("\\")])    // remove file names so that only project (subdirectory) names remain
                .Distinct().ToList();                  // remove duplicates
            return projlist;
        }

        private void ProjectChangedEvent(string projectName)
        {
            LoadLexemes(projectName);
        }

        private void LoadLexemes(string projectName) // Mother lexicon for project
        {
            // load Paratext Biblical Terms 
            var xmlPath = Path.Combine(_paratextProjectPath, projectName, "ProjectBiblicalTerms.xml");
            if (!File.Exists(xmlPath))
            {
                return;
            }
            pt = File.ReadAllLines(xmlPath);
            pbt = pt.ToList();

            // fix file name
            string path = Path.GetFullPath(Path.Combine(_paratextInstallPath, @"Terms\Lists\AllBiblicalTerms.xml"));
            if (!File.Exists(path))
            {
                Debug.WriteLine("");
            }
            at = File.ReadAllLines(path);
            abt = at.ToList();

            // possibly populate bib terms into Lexicon and interlinear files

            //            thedata.Add(new tabledt { Source = lexform, Lform = lextype, Gloss = gloss, Lang = glosslang, Refs = refs, Code = SenseID, Match = SenseID + lexform, Notes = notes, SimpRefs = refs, Phrase = (lextype == "Phrase") ? "Phr" : "", Word = (lextype == "Word") ? "Wrd" : "", Prefix = (lextype == "Prefix") ? "pre-" : "", Stem = (lextype == "Stem") ? "Stem" : "", Suffix = (lextype == "Suffix") ? "-suf" : "" });

            PathToProjLexLangs(bookfiles, projectName);
            string filterstr = @$"\{projectName}\";
            LexiconToRead = lexiconfiles.Where(s => s.ToString().Contains(filterstr)).ToList();
            language = "";
            s = File.ReadAllLines(LexiconToRead[0]);

            FilterString = "";
            // string stage = "MetaData";
            string lextype = "", lexform = "", refs = "", notes = "";
            string SenseID, gloss, glosslang;

            for (int i = 0; i < s.GetUpperBound(0); i++)        // loop through all lines lines lexicon building datatable
            {
                //  Analyses... item... string... ArrayOfLexeme... Lexeme [Type (Prefix, Stem, Suffix), Form, Homogrph]
                //  Entries... Entry... Sense [Id]... Gloss [Language]...
                switch (GetFirstTag(s[i]))
                {
                    case "Language":
                        language = GetTagValue(s[i]);
                        break;
                    case "Analyses":
                        //  stage = "Analyses";
                        break;
                    case "/Analyses":
                        break;
                    case "Entries":
                        //  stage = "Entries";
                        break;
                    case "/Entries":
                        //LoadGrid();
                        break;
                    //  case "item": parsingitem = true; break;    // discrete semantic unit start: XML level 1
                    //  case "/item": parsingitem = false; break;  // discrete semantic unit end
                    case "string":              // vernacular spelling: XML level 2
                        break;
                    //  case "ArrayOfLexeme": arrayoflexeme = true; break;         // XML level 3
                    case "/ArrayOfLexeme":      // arrayoflexeme = false;                      
                        break;
                    case "Lexeme":              // < Lexeme Type = "Stem" Form = "shaar" Homograph = "1" />
                        lextype = GetProperty(s[i], "Type");
                        lexform = GetProperty(s[i], "Form");
                        break;
                    case "Entry":               // entry = true;  // XML level
                        break;
                    case "/Entry":              // entry = false;
                        break;
                    case "Sense":               // XML level 
                        SenseID = GetProperty(s[i], "Id");
                        i++;                    // skip tag close
                        gloss = GetTagValue(s[i]);
                        glosslang = GetProperty(s[i], "Language");
                        thedata.Add(new tabledt
                        {
                            Source = lexform,
                            Lform = lextype,
                            Gloss = gloss,
                            Lang = glosslang,
                            Refs = refs,
                            Code = SenseID,
                            Match = SenseID + lexform,
                            Notes = notes,
                            SimpRefs = refs,
                            Phrase = (lextype == "Phrase") ? "Phr" : "",
                            Word = (lextype == "Word") ? "Wrd" : "",
                            Prefix = (lextype == "Prefix") ? "pre-" : "",
                            Stem = (lextype == "Stem") ? "Stem" : "",
                            Suffix = (lextype == "Suffix") ? "-suf" : ""
                        });
                        //                             thedata.Add(new tabledt { Source = lexform, Lform = lextype, Gloss = gloss, Lang = glosslang, Refs = refs, Code = SenseID, Match = SenseID + lexform, Notes = notes, SimpRefs = refs, Phrase = "" , Word = "", Prefix = "", Stem = "", Suffix = "" });
                        // Phr, Wrd, pre-Stem-suf
                        i++;                    // skip tag close
                        break;//
                }
            }

            TheData = thedata;

            GridCollectionView = CollectionViewSource.GetDefaultView(TheData);
            GridCollectionView.Filter = new Predicate<object>(FiterTerms);
            NotifyOfPropertyChange(() => GridCollectionView);
        }


        private string GetFirstTag(string t)
        {
            string ttrim = t.TrimStart(' ', '<');            // remove leading blanks and <
            string[] words = ttrim.Split(' ', '-', '>');      // split at first space, dash or >
            return words[0];                                // return first word only
        }
        private string GetTagValue(string t)
        {
            string ttrim = t.TrimStart(' ', '<');            // remove leading blanks and < leaving "tagname>VALUE</tagname>"
            string[] words = ttrim.Split('<', '>');         // value of simple tag is words[1] (tagname, VALUE, /tagname)
            return words[1];                                // return second word only
        }
        private string GetProperty(string t, string property)
        {
            // // < Lexeme Type = "Stem" Form = "shaar" Homograph = "1" />
            string srch = property + "=";
            int pos = t.IndexOf(srch) + srch.Length;
            string result = t.Substring(pos);
            string[] words = result.Split('"');             // value of simple tag is words[1] (tagename, VALUE, /tagname)
            return words[1];                                // return second word only
        }

        private void PathToProjLexLangs(List<string> lexlangslist, string proj)
        {
            lexlangslist = lexlangslist
                .Where(s => s.ToString().Contains("\\" + proj + "\\"))              // filter list to current project
                .Select(s => s[(_paratextProjectPath.Length + proj.Length + 13)..])               // remove up to Lexicon language
                .Select(s => s[..s.IndexOf("\\")].ToLower())                        // remove after the lexicon language
                .Distinct().ToList();                                               // remove duplicates
        }

        private void ClearFiterTerms()
        {
            FilterString = "";
        }

        private bool FiterTerms(object item)
        {
            tabledt? itemDT = item as tabledt;

            return (itemDT.Source.Contains(_filterString) || itemDT.Gloss.Contains(_filterString) ||
                    itemDT.Notes.Contains(_filterString));
        }

        private async Task FetchRefs()
        {
            Dictionary<string, string> LexMatRef = new Dictionary<string, string>();
            string tref, lx, lt, li;
            int bcnt = 0;
            string lexlang = Projects[this.ProjectSelectedId];
            var bookfilesfiltered = bookfiles.Where(s => s.ToString().Contains("\\" + lexlang)).ToList();

            foreach (string f in bookfilesfiltered)             // loop through books f
            {
                var b = File.ReadAllLines(f);                       // read file and check
                for (int k = 0; k < b.Count(); k++)
                {
                    if (b[k].Contains("<item>"))
                    {
                        if (b[++k].Contains("<string>"))
                        {
                            tref = GetTagValue(b[k]);
                            do
                            {
                                if (b[++k].Contains("<Lexeme"))  // build a dictionary where key = lexeme+gloss, and where value = references
                                {
                                    lx = lt = li = "";
                                    Lexparse(b[k], ref lx, ref lt, ref li);
                                    if (LexMatRef.ContainsKey(li + lx))     // key already exists so add references to previous value
                                        LexMatRef[li + lx] = LexMatRef[li + lx] + ", " + tref;
                                    else                                    // this is a new key so create a new key, value pair
                                        LexMatRef.Add(li + lx, tref);
                                }
                            }
                            while (!b[k].Contains("</item>"));
                        }
                    }
                }
            }

            List<string> rs;
            string ky, vl;
            int ndx2;
            int pndx = 0;
            foreach (var LMR in LexMatRef)
            {
                rs = LMR.Value.Split(',').ToList();   // change dictionary values from comma delimited string to List for sorting
                ky = LMR.Key;
                SortRefs(ref rs);                     // sort the List  
                vl = String.Join(", ", rs);           // change List back to comma delimited string

                if (!vl.Contains("missing"))
                {
                    ndx2 = thedata.FindIndex(s => s.Match == ky);
                    if (ndx2 >= 0) thedata[ndx2].Refs = vl;
                }
            }

            string L, G, formtemp;
            string simrefs = "";
            List<tabledt> results = new List<tabledt>();
            tabledt datrow;

            // join rows of that have the same lexeme and gloss
            for (int i = 0; i < thedata.Count; i++) // cannot use foreach because various items are removed causing for each loop error
            {
                datrow = thedata[i];
                if (datrow.Lform == "Phrase")
                    datrow.Phrase = "Phr";
                L = datrow.Source; // lexeme
                G = datrow.Gloss;  // gloss

                results = thedata.Where(s => (s.Source == L) && (s.Gloss == G)).ToList();  // create results List where lexemes and glosses match
                if (results.Count == 1) // no need to join two rows, but still need to Simplify
                {
                    SimplifyRefs(datrow.Refs.Split(',').ToList(), ref simrefs);
                    datrow.SimpRefs = simrefs;
                }
                else
                {   // skip first result because this is the same as the current datrow (thedata[i])
                    results.Remove(results[0]);
                    foreach (tabledt datrow2 in results)
                    {
                        datrow.Lform += " " + datrow2.Lform;  // merge lexical form data (Word, Stem, Prefix, Suffix, Phrase)
                        rs = (datrow.Refs + ", " + datrow2.Refs).Split(',').ToList();
                        SortRefs(ref rs);
                        datrow.Refs = String.Join(", ", rs);
                        SimplifyRefs(rs, ref simrefs);
                        datrow.SimpRefs = simrefs;
                        datrow.Code += " " + datrow2.Code;
                        datrow.Match += " " + datrow2.Match;
                        thedata.Remove(datrow2);
                    }
                    //                    datrow.Phrase = datrow.Lform.Contains("Phrase") ? "Phr" : "";
                    //                    datrow.Word = datrow.Lform.Contains("Word") ? "Wrd" : "";
                    //                    datrow.Prefix = datrow.Lform.Contains("Prefix") ? "pre-" : "";
                    //                    datrow.Stem = datrow.Lform.Contains("Stem") ? "Stem" : "";
                    //                    datrow.Suffix = datrow.Lform.Contains("Suffix") ? "-suf" : "";
                }

            }

            TheData = thedata;

            GridCollectionView = CollectionViewSource.GetDefaultView(TheData);
            GridCollectionView.Filter = new Predicate<object>(FiterTerms);
        }

        private void SimplifyRefs(List<string> rlst, ref string outputRefs)
        {
            List<string> rlst1 = new List<string>();
            List<string> rlst2 = new List<string>();
            List<string> rlstout = new List<string>();
            rlst = rlst.Select(s => s.Trim()).ToList();
            rlst = rlst.Where(s => s.Length > 0).ToList();
            rlst1 = rlst.Select(s => s.Substring(0, 3)).Distinct().ToList();    // find all unique book names in rlst
            foreach (string r in rlst1)                                         // loop through each unique book name
            {
                rlst2 = rlst.Where(s => s.Substring(0, 3).Equals(r)).ToList();  // find all references in this book
                rlst2 = rlst2.Select(s => s.Substring(4)).ToList();             // remove book name on all references
                rlst2[0] = r + " " + rlst2[0];                                  // replace book name on first reference in list
                rlstout.AddRange(rlst2);
            }
            outputRefs = String.Join(", ", rlstout);
        }

        private void SortRefs(ref List<string> refs)
        {
            refs = refs
                .Select(s => s.Trim()) //.ToList();    // trim leading and trailing to ensure valid BBB CCC:VVV
                .Where(s => s.Length > 0).ToList();    // remove references = "" 
            List<string> t_list = new List<string>();
            List<string> t_listOut = new List<string>();
            string[] book = { "01GEN", "02EXO", "03LEV", "04NUM", "05DEU", "06JOS", "07JDG", "08RUT", "091SA", "102SA", "111KI", "122KI", "131CH", "142CH", "15EZR", "16NEH", "17EST", "18JOB", "19PSA", "20PRO", "21ECC", "22SNG", "23ISA", "24JER", "25LAM", "26EZK", "27DAN", "28HOS", "29JOL", "30AMO", "31OBA", "32JON", "33MIC", "34NAM", "35HAB", "36ZEP", "37HAG", "38ZEC", "39MAL", "41MAT", "42MRK", "43LUK", "44JHN", "45ACT", "46ROM", "471CO", "482COR", "49GAL", "50EPH", "51PHP", "52COL", "531TH", "542TH", "551TI", "562TI", "57TIT", "58PHM", "59HEB", "60JAS", "611PE", "622PE", "631JN", "642JN", "653JN", "66JUD", "67REV", "70TOB", "71JDT", "72ESG", "73WIS", "74SIR", "75BAR", "76LJE", "77S3Y", "78SUS", "79BEL", "80MAN", "81PS2" };

            var dictbk = book.ToDictionary(item => item[2..5], item => item[..2]);
            // formerly book.ToDictionary(item => item.Substring(2, 3), item => item.Substring(0, 2));

            // convert back to sortable form (bbBBBcccvvv)  
            // lookup 0:2 to get bb from Dictionary dictbk and append 0:2
            // take 4: ':' and pad 0's to get ccc
            // take ':' to end and pad 0's to get vvv
            t_list = refs
                .Select(s => dictbk[s[..3]] + s[..3]
                + s[4..s.IndexOf(':')].PadLeft(3, '0')
                + s[(s.IndexOf(':') + 1)..].PadLeft(3, '0'))
                .ToList();
            // formerly s.Substring(0, 3)   s.Substring(4, s.IndexOf(':') - 4)  s.Substring(s.IndexOf(':') + 1)
            t_list.Sort();
            t_list = t_list.Distinct().ToList();

            // convert back to standard form (BBB ccc:vvv)  
            // skip 0:1 because don't need book number anymore, leaving BBB in 3:4
            // take 5:7 convert to number to lose leading zeros, then convert back to string for CCC
            // take 8:10 convert to number to lose leading zeros, then convert back to string for VVV
            t_listOut = t_list
                .Select(s => s[2..5] + " "
                + Convert.ToInt32(s[5..8]).ToString() + ":"
                + Convert.ToInt32(s[8..11]).ToString()).ToList();
            // formerly s.Substring(2, 3) s.Substring(5, 3) s.Substing(8, 3)
            refs = t_listOut;
        }
        private void Lexparse(string lexin, ref string lex, ref string lext, ref string lexi)
        {
            var tmp = lexin.Split('"');
            if (tmp.Count() > 3)
                lexi = tmp[3];
            else
                lexi = "missing";
            if (tmp.Count() > 1)
            {
                var tmp2 = tmp[1].Split(':');
                lext = tmp2[0];
                lex = tmp2[1];
            }
            else
            {
                lext = "missing";
                lex = "missing";
            }
        }

    }

    #endregion // Methods
}
