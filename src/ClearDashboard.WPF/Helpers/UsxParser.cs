using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using System.Xml.Xsl;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Helpers
{
    public class ParsedXML
    {
        public string VerseID { get; set; }
        public Inline Inline { get; set; }
    }

    public static class UsxParser
    {
        private static XmlReader usfx;
        private static bool inToc1, inToc2, inToc3;
        private static string toc1;
        private static string toc2;
        private static string toc3;

        public static List<ParsedXML> ParseXMLToList(string xml)
        {
            string style;
            string caller;
            string code;
            string number;
            string noteCaller = "";
            string currentChapter = "";

            string ret = "";

            List<ParsedXML> inlinesText = new List<ParsedXML>();

            usfx = XmlReader.Create(new StringReader(xml));
            while (usfx.Read())
            {
                if (usfx.NodeType == XmlNodeType.Element)
                {
                    style = usfx.GetAttribute("style");
                    caller = usfx.GetAttribute("caller");
                    code = usfx.GetAttribute("code");
                    number = usfx.GetAttribute("number");

                    switch (usfx.Name)
                    {
                        case "book":
                            ret += $@"\{style} {code} ";
                            break;
                        case "chapter":
                            if (style != null)
                            {
                                ret += $@"\{style} {number}" + "\n";
                                currentChapter = number.PadLeft(3, '0');
                            }
                            break;
                        case "verse":
                            if (style != null)
                            {
                                ret += "VERSEID=" + currentChapter + number.PadLeft(3, '0');
                                ret += "\n";
                                ret += $@"\{style} {number} ";
                            }
                            else
                            {
                                ret += "\n";

                            }
                            break;
                        case "para":
                            if (style == "p")
                            {
                                ret += $@"\{style} " + "\n";
                            }
                            else
                            {
                                ret += $@"\{style} ";
                            }

                            break;
                        case "note":
                            noteCaller = style;
                            ret += $@"\{style} {caller}";
                            break;
                        case "char":
                            ret += $@"\{style} ";
                            break;
                        default:
                            break;
                    }

                }
                else if (usfx.NodeType == XmlNodeType.EndElement)
                {
                    //handle closing XML element
                    switch (usfx.Name)
                    {
                        case "book":
                            ret += "\n";
                            break;
                        case "c":
                            ret += "\n";
                            break;
                        case "para":
                            ret += "\n";
                            break;

                        case "verse":
                            break;
                        case "char":
                            ret += $@"\{noteCaller}*";
                            break;
                    }

                }
                else if (((usfx.NodeType == XmlNodeType.Text) || (usfx.NodeType == XmlNodeType.SignificantWhitespace) || (usfx.NodeType == XmlNodeType.Whitespace)))
                {
                    // handle text
                    if (usfx.Value.Trim() != "")
                    {
                        ret += usfx.Value;
                    }
                }
            }

            usfx.Close();
            usfx.Dispose();


            try
            {
                // iterate through and make new runs for each line
                string[] lines = ret.Split('\n');
                foreach (var line in lines)
                {
                    List<RegExGroups> resultList = new List<RegExGroups>();
                    try
                    {
                        Regex regexObj = new Regex(@"\\([^\s]+)");
                        Match matchResult = regexObj.Match(line);
                        while (matchResult.Success)
                        {
                            resultList.Add(new RegExGroups
                            {
                                value = matchResult.Groups[1].Value,
                                index = matchResult.Index,
                                length = matchResult.Length
                            });
                            matchResult = matchResult.NextMatch();
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        // Syntax error in the regular expression
                    }

                    string verseID = "";
                    if (line.StartsWith("VERSEID="))
                    {
                        verseID = line;
                    }

                    if (resultList.Count == 0)
                    {
                        inlinesText.Add( new ParsedXML{Inline = new Run(line + "\n"), VerseID= verseID});
                    }
                    else
                    {
                        // loop through backwards
                        List<Inline> inlinesTmp = new List<Inline>();
                        string tmp = "";
                        int lastChar = line.Length;


                        for (int i = resultList.Count - 1; i >= 0; i--)
                        {
                            //Debug.WriteLine(resultList[i].value);

                            int index = resultList[i].index;
                            int length = resultList[i].length;
                            int startIndex = index + length;
                            string sLastPart = "";
                            if (startIndex > lastChar)
                            {
                                sLastPart = line.Substring(index);
                            }
                            else if (startIndex + 1 == lastChar)
                            {
                                sLastPart = line.Substring(startIndex);
                            }
                            else
                            {
                                sLastPart = line.Substring(startIndex, lastChar - startIndex);
                            }

                            // add in the plain text
                            inlinesTmp.Add(new Run(sLastPart));

                            // add in the tag part
                            if (index + length + 1 > lastChar)
                            {
                                inlinesTmp.Add(new Run(line.Substring(index))
                                {
                                    FontStyle = FontStyles.Italic,
                                    Foreground = Brushes.LightSkyBlue,
                                });
                            }
                            else
                            {
                                inlinesTmp.Add(new Run(line.Substring(index, length + 1))
                                {
                                    FontStyle = FontStyles.Italic,
                                    Foreground = Brushes.LightSkyBlue,
                                });
                            }

                            lastChar = index;
                        }

                        // reverse the inlines and add them to the list<>
                        inlinesTmp.Reverse();
                        foreach (var inline in inlinesTmp)
                        {
                            //var p = (System.Windows.Documents.Run)inline;
                            //Debug.WriteLine(p.Text);
                            inlinesText.Add(new ParsedXML { Inline = inline, VerseID = verseID });
                        }
                        inlinesText.Add( new ParsedXML { Inline = new Run("\n"), VerseID = verseID });
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            return inlinesText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputXml">The XML text to process</param>
        /// <param name="xsltPath">The path to the xslt file</param>
        /// <returns></returns>
        public static string TransformXMLToHTML(string inputXml, string xsltPath)
        {
            StringWriter results = new StringWriter();
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xsltPath);

            XmlDocument source = new XmlDocument();

            var stringStream = inputXml.ToStream();
            source.Load(stringStream);
            XmlReader xmlReadB = new XmlTextReader(new StringReader(source.DocumentElement.OuterXml));
            xslt.Transform(xmlReadB, null, results);

            stringStream.Close();
            stringStream.Dispose();

            return results.ToString();
        }

        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string ParseXMLstring(string xml)
        {
            string style;
            string caller;
            string code;
            string number;
            string noteCaller = "";

            string ret = "";

            usfx = XmlReader.Create(new StringReader(xml));

            while (usfx.Read())
            {
                if (usfx.NodeType == XmlNodeType.Element)
                {
                    style = usfx.GetAttribute("style");
                    caller = usfx.GetAttribute("caller");
                    code = usfx.GetAttribute("code");
                    number = usfx.GetAttribute("number");

                    switch (usfx.Name)
                    {
                        case "book":
                            ret += $@"\{style} {code} ";
                            break;
                        case "chapter":
                            if (style != null)
                            {
                                ret += $@"\{style} {number}" + "\n";
                            }
                            break;
                        case "verse":
                            if (style != null)
                            {
                                ret += $@"\{style} {number} ";
                            }
                            else
                            {
                                ret += "\n";
                            }
                            break;
                        case "para":
                            if (style == "p")
                            {
                                ret += $@"\{style} " + "\n";
                            }
                            else
                            {
                                ret += $@"\{style} ";
                            }

                            break;
                        case "note":
                            noteCaller = style;
                            ret += $@"\{style} {caller}";
                            break;
                        case "char":
                            ret += $@"\{style} ";
                            break;
                        default:
                            break;
                    }

                }
                else if (usfx.NodeType == XmlNodeType.EndElement)
                {
                    //handle closing XML element
                    switch (usfx.Name)
                    {
                        case "book":
                            ret += "\n";
                            break;
                        case "c":
                            ret += "\n";
                            break;
                        case "para":
                            ret += "\n";
                            break;

                        case "verse":
                            break;
                        case "char":
                            ret += $@"\{noteCaller}*";
                            break;
                    }

                }
                else if (((usfx.NodeType == XmlNodeType.Text) || (usfx.NodeType == XmlNodeType.SignificantWhitespace) || (usfx.NodeType == XmlNodeType.Whitespace)))
                {
                    // handle text
                    if (usfx.Value.Trim() != "")
                    {
                        ret += usfx.Value;
                    }
                }
            }

            usfx.Close();
            usfx.Dispose();
            return ret;
        }

        public static string ConvertXMLToHTML(string xml, string currentBook, string fontFamily, double fontSize)
        {
            string style;
            string caller;
            string code;
            string number;
            string noteCaller = "";
            string currentChapter = "";

            StringBuilder sb = new StringBuilder();

            List<string> inlinesText = new List<string>();

            usfx = XmlReader.Create(new StringReader(xml));
            while (usfx.Read())
            {
                if (usfx.NodeType == XmlNodeType.Element)
                {
                    style = usfx.GetAttribute("style");
                    caller = usfx.GetAttribute("caller");
                    code = usfx.GetAttribute("code");
                    number = usfx.GetAttribute("number");

                    switch (usfx.Name)
                    {
                        case "book":
                            sb.AppendLine($@"<span class='book'>\{style} {code} </span>");
                            break;
                        case "chapter":
                            if (style != null)
                            {
                                sb.AppendLine($@"<span class='chapter'>\{style} {number}</span><p/>");
                                currentChapter = number.PadLeft(3, '0');
                            }
                            break;
                        case "verse":
                            if (style != null)
                            {
                                string verseID = currentChapter + number.PadLeft(3, '0');
                                sb.AppendLine($@"<span class='verse' id='{currentBook}{verseID}'>\{style} {number}</span>");
                            }
                            else
                            {
                                sb.AppendLine("<p/>");
                            }
                            break;
                        case "para":
                            if (style == "p")
                            {
                                sb.AppendLine($@"<span class='tag'>\{style}</span><p/>");
                            }
                            else
                            {
                                sb.AppendLine($@"<span class='tag'>\{style}</span>");
                            }

                            break;
                        case "note":
                            noteCaller = style;
                            sb.AppendLine($@"<span class='tag'>\{style} {caller}</span>");
                            break;
                        case "char":
                            sb.AppendLine($@"<span class='tag'>\{style}</span>");
                            break;
                        default:
                            break;
                    }

                }
                else if (usfx.NodeType == XmlNodeType.EndElement)
                {
                    //handle closing XML element
                    switch (usfx.Name)
                    {
                        case "book":
                            sb.AppendLine("<p/>");
                            break;
                        case "c":
                            sb.AppendLine("<p/>");
                            break;
                        case "para":
                            sb.AppendLine("<p/>");
                            break;
                        case "verse":
                            break;
                        case "char":
                            sb.AppendLine($@"<span class='tag'>\{noteCaller}* </span>");
                            break;
                    }

                }
                else if (((usfx.NodeType == XmlNodeType.Text) || (usfx.NodeType == XmlNodeType.SignificantWhitespace) || (usfx.NodeType == XmlNodeType.Whitespace)))
                {
                    // handle text
                    if (usfx.Value.Trim() != "")
                    {
                        string text = usfx.Value;
                        text = text.Replace("<", "&lt;");
                        text = text.Replace(">", "&gt;");

                        sb.AppendLine(text);
                    }
                }
            }

            usfx.Close();
            usfx.Dispose();

            string html = "";
            html += "<html>\n";
            html += "  <head>\n";
            html += "    <META http-equiv='Content - Type' content='text/html; charset=utf-16'>\n";
            html += "    <meta content='utf-8'>\n";
            html += "<style>\n";
            html += "	body {\n";
            html += "	font-family: '" + fontFamily +  "';\n";
            html += "	font-size: " + fontSize + "rem;\n";
            html += "	background-color: #292929;\n";
            html += "	color: white;\n";
            html += "	}\n\n";
            html += "	.book {\n";
            html += "	color: red;\n";
            html += "	font-family: sans-serif;}\n\n";
            html += "	.chapter {\n";
            html += "	color: lime;\n";
            html += "	font-family: sans-serif;}\n";
            html += "	.verse {\n\n";
            html += "	color: orange;\n";
            html += "	font-family: sans-serif;}\n\n";
            html += "	.tag {\n";
            html += "	color: yellow;\n";
            html += "	font-family: sans-serif;}\n";
            html += "</style>\n";
            html += "  </head>\n";
            html += "  <body>\n";

            html += sb.ToString();

            html += "  </body>\n";
            html += "</html>\n";

            return html;
        }
    }
}
