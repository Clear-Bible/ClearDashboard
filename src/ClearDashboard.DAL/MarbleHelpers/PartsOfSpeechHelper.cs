using System.Collections.Generic;
using System.IO;

namespace ClearDashboard.DataAccessLayer.MarbleHelpers
{
    /// <summary>
    /// This class is used to help with the Marble project and was provided by Reinier
    /// along with the data
    /// </summary>
    public static class PartsOfSpeechHelper
    {

        private static MorphTagsProvider LoadProvider(string tagStructureFileName, string tagTranslationsFileName)
        {
            string tagStructureJson = LoadFileAsString(tagStructureFileName);
            string tagTranslationsJson = LoadFileAsString(tagTranslationsFileName);
            MorphTagsProvider provider = new MorphTagsProvider(tagStructureJson, tagTranslationsJson);
            return provider;
        }
        public static MorphTagsProvider GetGreekProvider(string tPath)
        {
            string tagStructureFileName = Path.Combine(tPath, "tag_structure_gnt_simple.json");
            string tagTranslationsFileName = Path.Combine(tPath, "tag_translations_gnt.json");
            MorphTagsProvider provider = LoadProvider(tagStructureFileName, tagTranslationsFileName);
            return provider;
        }
        public static MorphTagsProvider GetHebrewProvider(string tPath)
        {
            string tagStructureFileName = Path.Combine(tPath, "tag_structure_bhs.json");
            string tagTranslationsFileName = Path.Combine(tPath, "tag_translations_bhs.json");
            MorphTagsProvider provider = LoadProvider(tagStructureFileName, tagTranslationsFileName);
            return provider;
        }

        public static string LoadFileAsString(string fileName)
        {
            string result;
            if (File.Exists(fileName))
                result = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
            else
                result = "{}";
            return result;
        }

        public static string DecodeTag(MorphTagsProvider provider, string tTag, string tMorphLanguage)
        {
            Dictionary<string, string> dict = provider.TagToDictionary(tTag);
            string actualToolTip = provider.DictionaryToToolTip(dict, " ", tMorphLanguage);

            return actualToolTip;
        }
    }
}
