using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class TextUtils
    {
        /// <summary>
        /// Used to create the text used by the richtextbox where the string that we care about is highlighted.
        /// Other instances of the word are dim highlighted
        /// </summary>
        /// <param name="word"></param>
        /// <param name="verseText"></param>
        /// <param name="wordIndex"></param>
        /// <returns></returns>
        public static (string, bool) HighLightWordsRtf(List<string> words, string verseText, List<int>? wordIndex, List<string> puncs, string fontFamily, float fontSize)
        {
            bool found = false;
            bool highlightOnlyOne = wordIndex != null;
            string sTmp = "";

            List<string> value = new List<string>();
            List<int> index = new List<int>();
            List<int> length = new List<int>();
            List<bool> colorList = new List<bool>();


            // make a shadow copy of the verse and replace all the punctuation with spaces so we are not
            // encumbered by the punctuation
            string puncLessVerse = verseText;
            foreach (var punc in puncs)
            {
                // we need to maintain the same verse length so we need to pad
                // out the replacement spaces
                string sBlank = "".PadLeft(punc.Length, ' ');

                puncLessVerse = puncLessVerse.Replace(punc, sBlank);
            }

            foreach (var word in words)
            {
                // do the same for the target word that we are trying to test against
                string puncLessWord = word;
                foreach (var punc in puncs)
                {
                    // we need to maintain the same verse length so we need to pad
                    // out the replacement spaces
                    string sBlank = "".PadLeft(punc.Length, ' ');

                    puncLessWord = puncLessWord.Replace(punc, sBlank);
                }

                try
                {
                    // look for full word and case sensitive
                    Regex pattern = new Regex("(?i)(?<=\\s|^|\\W)" + puncLessWord.ToUpper() + "(?=\\s|$|\\W)");
                    Match matchResults = pattern.Match(puncLessVerse.ToUpper());
                    while (matchResults.Success)
                    {
                        value.Add(matchResults.Value);
                        index.Add(matchResults.Index);
                        length.Add(matchResults.Length);
                        colorList.Add(true);
                        matchResults = matchResults.NextMatch();
                        found = true;
                    }
                }
                catch (ArgumentException ex)
                {
                    // Syntax error in the regular expression
                    Console.WriteLine(ex.Message);
                }

                // remove those indexes where the value isn't correct
                if (highlightOnlyOne && index.Count > 1 && wordIndex.Count != index.Count)
                {
                    // we need to segment this text to find the match
                    List<string> puncList = DefaultPunctuation.GenerateList();

                    DefaultSegmenter segmenter = new DefaultSegmenter();
                    var segs = segmenter.GetSegments(verseText, puncList, "");

                    //find all occurences of this word
                    List<int> hits = new List<int>();
                    for (int i = 0; i < segs.Length; i++)
                    {
                        if (segs[i] == word)
                        {
                            hits.Add(i);
                        }
                    }

                    // look for those items that are missing and flag their color index as false
                    for (int i = 0; i < hits.Count; i++)
                    {
                        if (!wordIndex.Contains(hits[i]))
                        {
                            if (i < colorList.Count)
                            {
                                colorList[i] = false;
                            }
                        }
                    }
                }
            }

            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                sTmp = RtBtoRtf.ConvertToRtf(verseText, value, index, length, colorList, fontFamily, fontSize);
            });


            return (sTmp,found);
        }


        /// <summary>
        /// Removes text that are between the defined characters (inclusive)
        ///
        /// example: "This is [some] text" will remove the text between the two brackets
        /// </summary>
        public static string RemoveBetween(string s, char begin, char end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }


        /// <summary>
        /// From: https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        internal static List<string> LoadPunctuation()
        {
            List<string> puncs = new List<string>();
            puncs.Add(@"(");
            puncs.Add(@")");
            puncs.Add(@"‘");
            puncs.Add(@"[");
            puncs.Add(@"?");
            puncs.Add(@"]");
            puncs.Add(@"!");
            puncs.Add(@")");
            puncs.Add(@":");
            puncs.Add(@"’");
            puncs.Add(@"—");
            puncs.Add(@")");
            puncs.Add(@";");
            puncs.Add(@"”");
            puncs.Add(@"“");
            puncs.Add("\"");
            puncs.Add(@".");
            puncs.Add(@",");
            puncs.Add(@"？");
            puncs.Add(@"！");
            puncs.Add(@"：");
            puncs.Add(@"’");
            puncs.Add(@"”");
            puncs.Add(@"“");
            puncs.Add(@"，");
            puncs.Add(@"——");
            puncs.Add(@"‘");
            puncs.Add(@"。");
            puncs.Add(@"、");
            puncs.Add(@"；");
            puncs.Add(@"（");
            puncs.Add(@"》");
            puncs.Add(@"）");
            puncs.Add(@"《");
            puncs.Add(@"［");
            puncs.Add(@"。");
            puncs.Add(@"、");
            puncs.Add(@"〕");
            puncs.Add(@"」");
            puncs.Add(@"「");
            puncs.Add(@"/");
            puncs.Add(@"『");
            puncs.Add(@"?");
            puncs.Add(@"〔");
            puncs.Add(@"］");
            puncs.Add(@"』");
            puncs.Add(@",,");
            puncs.Add(@"‘‘");
            puncs.Add(@"’’’");
            puncs.Add(@"''");
            puncs.Add(@"-");
            puncs.Add(@"""""");
            puncs.Add(@"&");
            puncs.Add(@"{");
            puncs.Add(@"}");
            puncs.Add(@"*");
            puncs.Add(@"…");
            puncs.Add(@"_");
            puncs.Add(@"_​");
            puncs.Add(@"…");
            puncs.Add(@"°");
            puncs.Add(@"«");
            puncs.Add(@"»");
            puncs.Add(@"<");
            puncs.Add(@">");
            puncs.Add(@"#");
            puncs.Add(@"+");
            puncs.Add(@"=");
            puncs.Add(@"|");
            puncs.Add(@"\");
            puncs.Add(@"@");
            puncs.Add(@".");
            puncs.Add(@"¡!");
            puncs.Add(@"¿?");
            puncs.Add(@"¡");
            puncs.Add(@"¿");
            puncs.Add(@"…");
            puncs.Add(@"~");
            puncs.Add(@"،");
            puncs.Add(@"।");
            puncs.Add(@"॥");
            puncs.Add(@":-");
            puncs.Add(@"·");
            puncs.Add(@"〜");
            puncs.Add(@"【");
            puncs.Add(@"】");
            puncs.Add(@"・");
            puncs.Add(@"⟨");
            puncs.Add(@"⟩");
            puncs.Add(@"„");
            puncs.Add(@"‚");
            puncs.Add(@"–");
            puncs.Add(@"•");
            puncs.Add(@"≈");
            puncs.Add(@"×");
            puncs.Add(@"%");
            puncs.Add(@"<<");
            puncs.Add(@">>");
            puncs.Add(@"<");
            puncs.Add(@">");

            return puncs;
        }
        public static class DefaultPunctuation
        {
            public static List<string> GenerateList()
            {
                List<string> puncs = new List<string>();
                puncs.Add("(");
                puncs.Add(")");
                puncs.Add("‘");
                puncs.Add("[");
                puncs.Add("?");
                puncs.Add("]");
                puncs.Add("!");
                puncs.Add(")");
                puncs.Add(":");
                puncs.Add("’");
                puncs.Add("—");
                puncs.Add(")");
                puncs.Add(";");
                puncs.Add("”");
                puncs.Add("“");
                puncs.Add("\"");
                puncs.Add(".");
                puncs.Add(",");
                puncs.Add("？");
                puncs.Add("！");
                puncs.Add("：");
                puncs.Add("’");
                puncs.Add("”");
                puncs.Add("“");
                puncs.Add("，");
                puncs.Add("——");
                puncs.Add("‘");
                puncs.Add("。");
                puncs.Add("、");
                puncs.Add("；");
                puncs.Add("（");
                puncs.Add("》");
                puncs.Add("）");
                puncs.Add("《");
                puncs.Add("［");
                puncs.Add("。");
                puncs.Add("、");
                puncs.Add("〕");
                puncs.Add("」");
                puncs.Add("「");
                puncs.Add("/");
                puncs.Add("『");
                puncs.Add("?");
                puncs.Add("〔");
                puncs.Add("］");
                puncs.Add("』");
                puncs.Add("");
                puncs.Add("‘‘");
                puncs.Add("’’’");
                puncs.Add("''");
                puncs.Add("-");
                puncs.Add("\"\"\"\"");
                puncs.Add("&");
                puncs.Add("{");
                puncs.Add("}");
                puncs.Add("*");
                puncs.Add("…");
                puncs.Add("_");
                puncs.Add("_​");
                puncs.Add("…");
                puncs.Add("°");
                puncs.Add("«");
                puncs.Add("»");
                puncs.Add("<");
                puncs.Add(">");
                puncs.Add("#");
                puncs.Add("+");
                puncs.Add("=");
                puncs.Add("|");
                puncs.Add(@"\");
                puncs.Add("@");
                puncs.Add(".");
                puncs.Add("¡!");
                puncs.Add("¿?");
                puncs.Add("¡");
                puncs.Add("¿");
                puncs.Add("…");
                puncs.Add("~");
                puncs.Add("،");
                puncs.Add("।");
                puncs.Add("॥");
                puncs.Add(":-");
                puncs.Add("·");
                puncs.Add("〜");
                puncs.Add("【");
                puncs.Add("】");
                puncs.Add("・");
                puncs.Add("⟨");
                puncs.Add("⟩");
                puncs.Add("„");
                puncs.Add("‚");
                puncs.Add("–");
                puncs.Add("•");
                puncs.Add("≈");
                puncs.Add("×");
                puncs.Add("%");
                puncs.Add("<<");
                puncs.Add(">>");
                puncs.Add("<");
                puncs.Add(">");

                return puncs;
            }
        }
    }

    public class DefaultSegmenter //: ISegmenter
    {
        public string[] GetSegments(
            string text,
            List<string> puncs,
            string lang)
        {
            string tokens = "", unusedArgument = "";
            SegPuncs(ref tokens, ref unusedArgument, text.Trim(), puncs, lang);
            return tokens.Split(' ');
        }


        private void SegPuncs(ref string puncText, ref string puncLowerText, string verseText, List<string> puncs, string lang)
        {
            verseText = verseText.Replace("—", " — ");
            verseText = verseText.Replace("-", " - ");
            verseText = verseText.Replace(",“", ", “");
            verseText = verseText.Replace("  ", " ");
            if (lang == "Gbary")
            {
                verseText = verseText.Replace("^", "");
            }
            verseText = verseText.Trim();
            string[] words = verseText.Split(" ".ToCharArray());

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                char punc = FindPunc(word, puncs);
                string[] subWords = word.Split(punc);
                if (subWords.Length == 2)
                {
                    SepPuncs(ref puncText, ref puncLowerText, subWords[0], puncs, lang);
                    SepPuncs(ref puncText, ref puncLowerText, punc.ToString(), puncs, lang);
                    SepPuncs(ref puncText, ref puncLowerText, subWords[1], puncs, lang);
                }
                else
                {
                    SepPuncs(ref puncText, ref puncLowerText, word, puncs, lang);
                }
            }
        }


        private char FindPunc(string word, List<string> puncs)
        {
            Regex r = new Regex("[0-9]+.+[0-9]+");
            Match m = r.Match(word);
            if (m.Success)
            {
                return (char)0;
            }
            char[] chars = word.ToCharArray();
            for (int i = 1; i < chars.Length - 1; i++) // excluding puncs on the peripheral
            {
                if (puncs.Contains(chars[i].ToString()) && chars[i].ToString() != "’" && chars[i].ToString() != "'")
                {
                    return chars[i];
                }
            }

            return (char)0;
        }


        private void SepPuncs(ref string puncText, ref string puncLowerText, string word, List<string> puncs, string lang)
        {
            ArrayList postPuncs = new ArrayList();

            while (word.Length > 0 && (StartsWithPunc(word, puncs) || StartsWithPunc2(word, puncs)))
            {
                if (StartsWithPunc2(word, puncs))
                {
                    string firstChars = word.Substring(0, 2);
                    puncText += " " + firstChars;
                    puncLowerText += " " + firstChars;
                    word = word.Substring(2);
                }
                else
                {
                    string firstChar = word.Substring(0, 1);
                    puncText += " " + firstChar;
                    puncLowerText += " " + firstChar;
                    word = word.Substring(1);
                }
            }

            if (lang == "French")
            {
                string contractedWord = string.Empty;

                if (word.Contains("’"))
                {
                    contractedWord = GetContractedWord(word, "’");
                }
                if (word.Contains("ʼ"))
                {
                    contractedWord = GetContractedWord(word, "ʼ");
                }

                if (contractedWord.Length > 1)
                {
                    puncText += " " + contractedWord;
                    puncLowerText += " " + contractedWord;
                    word = word.Substring(contractedWord.Length);
                }
            }

            while (word.Length > 0 && (EndsWithPunc(word, puncs) || EndsWithPunc2(word, puncs)))
            {
                if (EndsWithPunc2(word, puncs))
                {
                    string lastChars = word.Substring(word.Length - 2);
                    postPuncs.Add(lastChars);
                    word = word.Substring(0, word.Length - 2);
                }
                else
                {
                    string lastChar = word.Substring(word.Length - 1);
                    postPuncs.Add(lastChar);
                    word = word.Substring(0, word.Length - 1);
                }
            }

            puncText += " " + word;
            puncLowerText += " " + word.ToLower();

            if (postPuncs.Count > 0)
            {
                for (int i = postPuncs.Count - 1; i >= 0; i--)
                {
                    string c = (string)postPuncs[i];
                    puncText += " " + c;
                    puncLowerText += " " + c;
                }
            }

            puncText = puncText.Trim();
            puncLowerText = puncLowerText.Trim();
        }


        private bool StartsWithPunc(string word, List<string> puncs)
        {
            string firstChar = word.Substring(0, 1);
            if (puncs.Contains(firstChar))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool StartsWithPunc2(string word, List<string> puncs)
        {
            if (word.Length > 1)
            {
                string firstChars = word.Substring(0, 2);
                if (puncs.Contains(firstChars))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }


        private string GetContractedWord(string word, string apostraphe)
        {
            return word.Substring(0, word.IndexOf(apostraphe) + 1);
        }


        private bool EndsWithPunc(string word, List<string> puncs)
        {
            string lastChar = word.Substring(word.Length - 1);
            if (puncs.Contains(lastChar))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool EndsWithPunc2(string word, List<string> puncs)
        {
            if (word.Length > 1)
            {
                string lastChars = word.Substring(word.Length - 2);
                if (puncs.Contains(lastChars))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
