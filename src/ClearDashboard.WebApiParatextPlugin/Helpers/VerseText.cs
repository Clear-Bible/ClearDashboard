using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class VerseText
    {
        public static string LookupVerseText(IProject mProject, int BookNum, int ChapterNum, int VerseNum)
        {
            var _lastBookCode = "";
            var _lastChapterNum = 0;
            var _lastScript = new List<string>();

            IEnumerable<IUSFMToken> tokens = mProject.GetUSFMTokens(BookNum, ChapterNum);
            if (tokens is null)
            {
                return "";
            }

            List<string> lines = new List<string>();
            foreach (var token in tokens)
            {
                if (token is IUSFMMarkerToken marker)
                {
                    if (marker.Type == MarkerType.Verse)
                    {
                        // skip if the verses are beyond what we are looking for
                        int i = 0;
                        bool result = int.TryParse(marker.Data, out i);

                        if (result)
                        {
                            if (Convert.ToInt16(marker.Data) > VerseNum)
                            {
                                break;
                            }
                            lines.Add($"Verse [{marker.Data}]");
                        }
                        else
                        {
                            // verse span so bust up the verse span
                            string[] nums = marker.Data.Split('-');
                            if (nums.Length > 1)
                            {
                                if (int.TryParse(nums[0], out i))
                                {
                                    if (int.TryParse(nums[1], out i))
                                    {
                                        int start = Convert.ToInt16(nums[0]);
                                        int end = Convert.ToInt16(nums[1]);
                                        for (int j = start; j < end + 1; j++)
                                        {
                                            lines.Add($"Verse [{j}]");
                                        }
                                    }
                                }
                            }
                        }


                    }
                }
                else if (token is IUSFMTextToken textToken)
                {
                    if (token.IsScripture)
                    {
                        lines.Add("Text Token: " + textToken.Text);
                    }
                }
            }

            string sText = "";

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains($"Verse [{VerseNum}]"))
                {
                    // get the next lines until the next verse
                    for (int j = i; j < lines.Count; j++)
                    {
                        if (lines[j].StartsWith("Text Token: "))
                        {
                            sText += lines[j].Substring(12);
                        }
                    }
                    break;
                }
            }

            return sText;
        }
    }
}
