using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using System.Xml.Xsl;
using ClearDashboard.Common.Models;

namespace ClearDashboard.Wpf.Helpers
{
    public static class UsxParser
    {
        private static XmlReader usfx;
        private static bool inToc1, inToc2, inToc3;
        private static string toc1;
        private static string toc2;
        private static string toc3;

        public static List<Inline> ParseXMLToList(string xml)
        {
            string style;
            string caller;
            string code;
            string number;
            string noteCaller = "";

            string ret = "";

            List<Inline> inlinesText = new List<Inline>();

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

                    if (resultList.Count == 0)
                    {
                        inlinesText.Add(new Run(line + "\n"));
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
                            inlinesText.Add(inline);
                        }
                        inlinesText.Add(new Run("\n"));
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
    }
}
