using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ClearDashboard.DataAccessLayer.MarbleHelpers
{
    public class MorphTagsProvider
    {
        private JObject _tagStructure;
        private JObject _tagTranslations;

        // Constructor.
        // The TagStructureJSON must be a String into which the tag_structure JSON 
        // has been loaded.
        // The TagTranslationsJSON must be a String into whic the tag_translations JSON
        // has been loaded.
        public MorphTagsProvider(string tagStructureJson, string tagTranslationsJson)
        {
            _tagStructure = JObject.Parse(tagStructureJson);
            _tagTranslations = JObject.Parse(tagTranslationsJson);
        }

        // Return the category that is at the root of the tag.
        // Usually this is the part of speech.
        public string GetTagRootCategory()
        {
            string result = _tagStructure.GetValue("tag_root").ToString();
            return result;
        }

        private string GetOptionValue(string tag, string optionName)
        {
            List<string> optionList = GetPossibleSecondaryCategoryValues(optionName);

            foreach (string tagOption in optionList)
            {
                if (tag.Length >= tagOption.Length)
                {
                    string actualOption = tag.Substring(0, tagOption.Length);
                    if (actualOption == tagOption)
                        return tagOption;
                }
            }

            // We did not find the option.

            // Return "@NOTFOUND@", since we did not find it.
            return "@NOTFOUND@";
        }

        // Transform a tag into a Dictionary(Of String, String).
        // The keys are tag categories.
        // The values are the parts of the tag that describes that category.
        // For example: "part_of_speech" -> "v", "person" -> "1", "gender" -> "m", etc.
        // These examples may or may not be valid for particular tagging schemes; 
        // they are just examples.
        public Dictionary<string, string> TagToDictionary(string tag)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string tagRootCategory = GetTagRootCategory();
            string tagRoot = GetOptionValue(tag, tagRootCategory);

            if (tagRoot == "@NOTFOUND@")
                // Root not found. Tag malformed. Return empty dictionary.
                return result;
            else
            {
                // Chop off the tagRoot
                tag = tag.Substring(tagRoot.Length);

                result.Add(tagRootCategory, tagRoot);

                List<string> tagCategories = GetTagSequence(tagRoot);

                foreach (string tagCategory in tagCategories)
                {
                    string tagOption = GetOptionValue(tag, tagCategory);

                    // Chop off the tagOption
                    if (tagOption != "@NOTFOUND@")
                        tag = tag.Substring(tagOption.Length);

                    result.Add(tagCategory, tagOption);
                }

                return result;
            }
        }

        // Converts the Dict parameter to a tooltip String.
        // The Dict must have been constructed with (a process similar to) the TagToDictionary function
        // in this class.
        // The Language parameter is used as the top-level key into the TagTranslations JSON.
        // Thus you must know which top-level entries are present in that JSON in order
        // to use this.
        public string DictionaryToToolTip(Dictionary<string, string> dict, string valueSeparator, string language)
        {
            List<string> resultList = new List<string>();

            string tagRootCategory = GetTagRootCategory();

            if (!dict.ContainsKey(tagRootCategory))
                // Dict does not contain tag root category. Might as well return the empty string.
                return "";

            string tagRoot = dict[tagRootCategory];

            string translation = GetTranslationOfCategoryAndValue(tagRootCategory, tagRoot, language);
            resultList.Add(translation);

            List<string> tagSequence = GetTagSequence(tagRoot);

            foreach (var category in tagSequence)
            {
                if (dict.ContainsKey(category))
                {
                    string value = dict[category];
                    var valueTranslation = GetTranslationOfCategoryAndValue(category, value, language);
                    if (valueTranslation.Length != 0)
                        resultList.Add(valueTranslation);
                }
            }

            string result = string.Join(valueSeparator, resultList);
            return result;
        }

        // Retrieves from the TagTranslations JSON the translation of the combination of Category and Value,
        // using the Language parameter as the top-level key into the TagTranslations JSON.
        // Thus you must know which top-level keys are available in the TagTranslations JSON in order to
        // use this function.
        public string GetTranslationOfCategoryAndValue(string category, string value, string language)
        {
            if (_tagTranslations.ContainsKey(language))
            {
                JToken translations = _tagTranslations[language];
                JObject translationsDict = translations.ToObject<JObject>();
                string key = category + "-" + value;
                if (translationsDict.ContainsKey(key))
                {
                    string result = translationsDict[key].ToString();
                    return result;
                }
                else
                    return "";
            }
            else
                return "@TRANSLATION_LANGUAGE_NOT_FOUND@";
        }

        // Get a list of the categories for the tag root in any given kind
        // of tag, determined by the tag root.  The tag root category is included.
        public List<string> GetTagSequence(string tagRoot)
        {
            List<string> result = new List<string>();

            JToken tagSequenceJToken = _tagStructure.SelectToken("tag_sequence").SelectToken(tagRoot);
            foreach (JToken tagCategory in tagSequenceJToken)
            {
                if (tagCategory.Type == JTokenType.String)
                    result.Add(tagCategory.ToString());
            }

            return result;
        }

        // Get a list of all of the categories present in the
        // tagging scheme, including the tag root.
        public List<string> GetAllCategories()
        {
            List<string> result = new List<string>();
            JToken optionList = _tagStructure.GetValue("tag_options");
            foreach (JToken category in optionList.ToList())
                result.Add(category.ToString());
            return result;
        }
        public List<string> GetAllPartsOfSpeech()
        {
            List<string> result = new List<string>();
            JToken options = _tagStructure.GetValue("tag_options");
            JToken categoryOptions = options.SelectToken("part_of_speech");
            foreach (JToken category in categoryOptions.ToList())
                result.Add(category.ToString());
            return result;
        }


        // Get a List of the possible values for the given Category.
        public List<string> GetPossibleCategoryValues(string category)
        {
            List<string> result = new List<string>();
            JToken tagOptions = _tagStructure.SelectToken("tag_sequence");
            if (tagOptions is not null)
            {
                JToken categoryOptions = tagOptions.SelectToken(category);
                if (categoryOptions is not null)
                {
                    foreach (JToken tagOptionJToken in categoryOptions)
                    {
                        if (tagOptionJToken.Type == JTokenType.String)
                        {
                            string tagOption = tagOptionJToken.ToString();
                            result.Add(tagOption);
                        }
                    }
                }
            }
            return result;
        }

        // Get a List of the possible values for the given Category.
        public List<string> GetPossibleSecondaryCategoryValues(string category)
        {
            List<string> result = new List<string>();
            JToken tagOptions = _tagStructure.SelectToken("tag_options");
            JToken categoryOptions = tagOptions.SelectToken(category);
            if (categoryOptions is not null)
            {
                foreach (JToken tagOptionJToken in categoryOptions)
                {
                    if (tagOptionJToken.Type == JTokenType.String)
                    {
                        string tagOption = tagOptionJToken.ToString();
                        result.Add(tagOption);
                    }
                }
            }
            return result;
        }
    }
}
