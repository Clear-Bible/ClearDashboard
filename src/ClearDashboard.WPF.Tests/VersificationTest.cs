using System.Collections.Generic;
using System.Windows.Documents;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;
using SIL.Scripture;
using Versification = ClearDashboard.Wpf.Helpers.Versification;

namespace ClearDashboard.WPF.Tests
{
    public class VersificationTest
    {
        /// <summary>
        /// Tests the Eng.vrs format
        /// </summary>
        [Fact]
        public void TestEnglish()
        {
            List<VersificationList> list = new List<VersificationList> 
            { 
                new VersificationList{SourceBBBCCCVV="014001018", TargetBBBCCCVV=""}, // 2CH 2:1 = 2CH 1:18
                new VersificationList{SourceBBBCCCVV="014013023", TargetBBBCCCVV=""}, // 2CH 14:1 = 2CH 13:23
                new VersificationList{SourceBBBCCCVV="016010001", TargetBBBCCCVV=""}, // NEH 9:38 = NEH 10:1
                new VersificationList{SourceBBBCCCVV="019006001", TargetBBBCCCVV=""}, // PSA 6:0 - 10 = PSA 6:1 - 11
                new VersificationList{SourceBBBCCCVV="019051001", TargetBBBCCCVV=""}, // PSA 51:0 = PSA 51:1
            };

            string[] correctVerses = new string[]
            {
                "014002001",
                "014014001",
                "016009038",
                "019006000",
                "019051000",
            };

            var newList =Versification.GetVersificationFromOriginal(list, new Project
            {
                ScrVersType = ScrVersType.English,
                IsCustomVersification = false,
            });

            for (int i = 0; i < list.Count; i++)
            {
                Assert.Equal(correctVerses[i], list[i].TargetBBBCCCVV);
            }
            
        }

        /// <summary>
        /// Tests the Org.vrs format
        /// </summary>
        [Fact]
        public void TestOriginal()
        {
            List<VersificationList> list = new List<VersificationList>
            {
                new VersificationList{SourceBBBCCCVV="014001018", TargetBBBCCCVV=""}, 
                new VersificationList{SourceBBBCCCVV="014013023", TargetBBBCCCVV=""}, 
                new VersificationList{SourceBBBCCCVV="016010001", TargetBBBCCCVV=""}, 
                new VersificationList{SourceBBBCCCVV="019006001", TargetBBBCCCVV=""}, 
                new VersificationList{SourceBBBCCCVV="019051001", TargetBBBCCCVV=""}, 
            };

            string[] correctVerses = new string[]
            {
                "014001018",
                "014013023",
                "016010001",
                "019006001",
                "019051001",
            };

            var newList = Versification.GetVersificationFromOriginal(list, new Project
            {
                ScrVersType = ScrVersType.Original,
                IsCustomVersification = false,
            });

            for (int i = 0; i < list.Count; i++)
            {
                // should return the same as the input
                Assert.Equal(correctVerses[i], list[i].TargetBBBCCCVV);
            }

        }

        /// <summary>
        /// Tests the rsc.vrs format RussianProtestant
        /// </summary>
        [Fact]
        public void TestRussianProtestant()
        {
            List<VersificationList> list = new List<VersificationList>
            {
                new VersificationList{SourceBBBCCCVV="001032001", TargetBBBCCCVV=""}, //GEN 31:55 = GEN 32:1
                new VersificationList{SourceBBBCCCVV="002021037", TargetBBBCCCVV=""}, //EXO 22:1 = EXO 21:37
                new VersificationList{SourceBBBCCCVV="003014056", TargetBBBCCCVV=""}, //LEV 14:55 = LEV 14:56
                new VersificationList{SourceBBBCCCVV="019017001", TargetBBBCCCVV=""}, //PSA 16:0 - 15 = PSA 17:0 - 15
                new VersificationList{SourceBBBCCCVV="022001002", TargetBBBCCCVV=""}, //SNG 1:1-16 = SNG 1:2-17
                new VersificationList{SourceBBBCCCVV="047011032", TargetBBBCCCVV=""}, //2CO 11:32 = 2CO 11:32
                new VersificationList{SourceBBBCCCVV="066012018", TargetBBBCCCVV=""}, //REV 13:1 = REV 12:18
            };

            string[] correctVerses = new string[]
            {
                "001031055",
                "002022001",
                "003014055",
                "019016001",
                "022001001",
                "047011032",
                "066013001",
            };

            var newList = Versification.GetVersificationFromOriginal(list, new Project
            {
                ScrVersType = ScrVersType.RussianProtestant,
                IsCustomVersification = false,
            });

            for (int i = 0; i < list.Count; i++)
            {
                // should return the same as the input
                Assert.Equal(correctVerses[i], list[i].TargetBBBCCCVV);
            }

        }

        /// <summary>
        /// Tests the rso.vrs format RussianOrthodox
        /// </summary>
        [Fact]
        public void TestRussianOrthodox()
        {
            List<VersificationList> list = new List<VersificationList>
            {
                new VersificationList{SourceBBBCCCVV="001032001", TargetBBBCCCVV=""}, //GEN 31:55 = GEN 32:1
                new VersificationList{SourceBBBCCCVV="002021037", TargetBBBCCCVV=""}, //EXO 22:1 = EXO 21:37
                new VersificationList{SourceBBBCCCVV="004025019", TargetBBBCCCVV=""}, //NUM 26:1 = NUM 25:19
                new VersificationList{SourceBBBCCCVV="009021001", TargetBBBCCCVV=""}, //1SA 20:43 = 1SA 21:1
                new VersificationList{SourceBBBCCCVV="014001018", TargetBBBCCCVV=""}, //2CH 2:1 = 2CH 1:18
                new VersificationList{SourceBBBCCCVV="014013023", TargetBBBCCCVV=""}, //2CH 14:1 = 2CH 13:23
                new VersificationList{SourceBBBCCCVV="019012001", TargetBBBCCCVV=""}, //PSA 11:0-9 = PSA 12:0-9
                new VersificationList{SourceBBBCCCVV="066012018", TargetBBBCCCVV=""}, //REV 13:1 = REV 12:18
            };

            string[] correctVerses = new string[]
            {
                "001031055",
                "002022001",
                "004026001",
                "009020043",
                "014002001",
                "014014001",
                "019011001",
                "066013001",
            };

            var newList = Versification.GetVersificationFromOriginal(list, new Project
            {
                ScrVersType = ScrVersType.RussianOrthodox,
                IsCustomVersification = false,
            });

            for (int i = 0; i < list.Count; i++)
            {
                // should return the same as the input
                Assert.Equal(correctVerses[i], list[i].TargetBBBCCCVV);
            }

        }

        /// <summary>
        /// Tests the vul.vrs format Vulgate
        /// </summary>
        [Fact]
        public void TestVulgate()
        {
            List<VersificationList> list = new List<VersificationList>
            {
                new VersificationList{SourceBBBCCCVV="001032001", TargetBBBCCCVV=""}, //GEN 31:55 = GEN 32:1
                new VersificationList{SourceBBBCCCVV="002021037", TargetBBBCCCVV=""}, //EXO 22:1 = EXO 21:37
                new VersificationList{SourceBBBCCCVV="004012016", TargetBBBCCCVV=""}, //NUM 13:1 = NUM 12:16
                new VersificationList{SourceBBBCCCVV="006005014", TargetBBBCCCVV=""}, //JOS 5:15 = JOS 5:14
                new VersificationList{SourceBBBCCCVV="012012001", TargetBBBCCCVV=""}, //2KI 11:21 = 2KI 12:1
                new VersificationList{SourceBBBCCCVV="019012001", TargetBBBCCCVV=""}, //PSA 11:0-9 = PSA 12:0-9
                new VersificationList{SourceBBBCCCVV="041009001", TargetBBBCCCVV=""}, //MRK 8:39 = MRK 9:1
                new VersificationList{SourceBBBCCCVV="044007056", TargetBBBCCCVV=""}, //ACT 7:55 = ACT 7:56
            };

            string[] correctVerses = new string[]
            {
                "001031055",
                "002022001",
                "004013001",
                "006005014",
                "012011021",
                "019011001",
                "041008039",
                "044007055",
            };

            var newList = Versification.GetVersificationFromOriginal(list, new Project
            {
                ScrVersType = ScrVersType.Vulgate,
                IsCustomVersification = false,
            });

            for (int i = 0; i < list.Count; i++)
            {
                // should return the same as the input
                Assert.Equal(correctVerses[i], list[i].TargetBBBCCCVV);
            }

        }

    }

}