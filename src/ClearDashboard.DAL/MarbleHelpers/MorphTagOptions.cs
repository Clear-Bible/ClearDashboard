using System.Collections.Generic;
using System.Linq;

namespace ClearDashboard.DataAccessLayer.MarbleHelpers
{
    // This class represents a set of morphological tags
    // in which each morphological category may have zero or
    // more values.
    // 
    // The idea is to be able to have the user select any number
    // of values for each morphological category, then store the
    // result in this class, and in the end convert the
    // values to something suitable for use in an Emdros query.
    // 
    // You can use the MorphTagsProvider.GetAllCategories() Function
    // to know which categories are possible.
    // 
    // Once you know the possible categories, you can use the
    // MorphTagOptions.GetValuesOnOrOff() Function to obtain a Dictionary
    // which tells you, for each Category: 
    // a) All of the possible values for that category, and
    // b) Whether the MorphTagOptions object has that value on or off.
    // 
    // This, in turn, can be used to populate the UI.
    // 
    // Once the UI has been changed, you can use either of the following APIs
    // to update this:
    // 
    // a) MorphTagOptions.Add    (individual check mark)
    // b) MorphTagOptions.Remove (individual check mark)
    // c) MorphTagOptions.SetValuesOnOrOff (all values)
    // 
    // Once the user is satisfied, you can use the MorphTagOptions.ToRegEx()
    // API to obtain a regular expression suitable for querying the tag 
    // feature of the underlying Emdros database.
    // 
    public class MorphTagOptions
    {
        private Dictionary<string, HashSet<string>> _options;

        public MorphTagOptions()
        {
            _options = new Dictionary<string, HashSet<string>>();
        }

        // Adds the Value to the Category.
        public void Add(string category, string value)
        {
            if (!_options.ContainsKey(category))
                _options.Add(category, new HashSet<string>());
            _options[category].Add(value);
        }

        // Removes the Value from the Category, if present.
        // If not present, does nothing.
        public void Remove(string category, string value)
        {
            if (_options.ContainsKey(category))
            {
                if (_options[category].Contains(value))
                    _options[category].Remove(value);
            }
        }


        // Return a Boolean indicating whether the given Value in the given Category
        // is 'On' (True) or 'Off' (False) in the MorphTagOptions.
        public bool IsValueOn(string category, string value)
        {
            if (_options.ContainsKey(category))
            {
                if (_options[category].Contains(value))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        // For the given Category, returns a List(Of String) containing
        // the values that are currently 'on'.
        public List<string> GetValuesOn(string category)
        {
            List<string> result = new List<string>();
            if (_options.ContainsKey(category))
            {
                foreach (var value in _options[category])
                    result.Add(value);
            }
            else
            {
            }
            return result;
        }

        // For the given Category, returns a Dictionary telling which of the possible values
        // are on (True) or off (False).  All of the possible values for the category
        // are in the Dictionary as keys.
        public Dictionary<string, bool> GetValuesOnOrOff(ref MorphTagsProvider provider, string category)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            List<string> valueList = provider.GetPossibleCategoryValues(category);
            foreach (string value in valueList)
            {
                bool valueIsOn = IsValueOn(category, value);
                result.Add(category, valueIsOn);
            }

            return result;
        }

        // Set the given values of the given category on or off.
        // The Category String must be a valid category from the tag_structure JSON.
        // The Values Dictionary must contain keys which are valid values for the given category.
        // The ones that, in the Dictionary, have the value 'False' are turned off.
        // The ones that, in the dictionary, have the value 'True', are turned on.
        public void SetValuesOnOrOff(string category, Dictionary<string, bool> values)
        {
            foreach (string valueKey in values.Keys)
            {
                bool onOrOff = values[valueKey];
                if (onOrOff)
                    Add(category, valueKey);
                else
                    Remove(category, valueKey);
            }
        }

        // Converts the current state into a regex suitable for use in querying Emdros.
        // The result is a regex which can be compared with the MQL ~ operator against 
        // the tag feature.
        // 
        // More than one value for a given category results in a logical "OR" between 
        // each value in the Emdros query.
        // 
        // Zero values for a given, non-part-of-speech category results in 
        // that category being underspecified, i.e., "any".  In the regex, this will be 
        // a sequence Of dots.
        // 
        // Zero values for the "part_of_speech" category results in
        // a regex which has all possible parts of speech, with the pertinent morphological
        // categories specified or (if absent) under-specified (with dots).
        public string ToRegEx(ref MorphTagsProvider provider)
        {
            string tagRootCategory = provider.GetTagRootCategory();
            List<string> tagRootList = GetValuesOn(tagRootCategory);
            if (tagRootList.Count() == 0)
                tagRootList = provider.GetPossibleCategoryValues(tagRootCategory);

            List<string> resultList = new List<string>();
            foreach (var tagRoot in tagRootList)
                resultList.Add(TagRootToRegEx(ref provider, tagRoot));

            string resultInnards;
            if (resultList.Count() == 1)
                // there is only one result. Take it.
                resultInnards = resultList[0];
            else
            {
                // There are either 0, or 2 or more.
                // Add each inner result with surrounding parentheses.
                List<string> innerList = new List<string>();
                foreach (string innerResult in resultList)
                    innerList.Add("(" + innerResult + ")");

                // Join them with "|" in between.
                resultInnards = string.Join("|", innerList);
            }

            string result = "^" + resultInnards + "$";
            return result;
        }

        // A private Function, which returns a RegEx fragment based on the tagRoot parameter
        // and the current state.
        private string TagRootToRegEx(ref MorphTagsProvider provider, string tagRoot)
        {
            List<string> resultList = new List<string>();
            resultList.Add(tagRoot);
            List<string> tagSequence = provider.GetTagSequence(tagRoot);
            foreach (string category in tagSequence)
            {
                List<string> realValueList;
                List<string> valueList = GetValuesOn(category);
                if (valueList.Count() == 0)
                    // No values on. Turn all on by getting all possible values.
                    realValueList = provider.GetPossibleCategoryValues(category);
                else
                    // At least one value on. Only select the one(s) on.
                    realValueList = valueList;

                // Make categoryList of (parenthesis-surrounded) values. 
                List<string> categoryList = new List<string>();
                if (realValueList.Count() == 1)
                    // There is only one value.
                    // Add it without parentheses.
                    categoryList.Add(realValueList[0]);
                else
                    // There is either 0, or 2 or more.
                    // Add each with surrounding parentheses.
                    foreach (string value in realValueList)
                    {
                        string valueString = "(" + value + ")";
                        categoryList.Add(valueString);
                    }

                // Join them all by "OR"
                if (categoryList.Count() == 1)
                    // There is precisely one. Just add it to resultList.
                    resultList.Add(categoryList[0]);
                else
                {
                    // There is either 0, or 2 or more.
                    // Join with "|" in between.
                    string categoryResult = string.Join("|", categoryList);

                    // Surround with "(" and ")", and add to resultList
                    resultList.Add("(" + categoryResult + ")");
                }
            }
            string result = string.Join("", resultList);
            return result;
        }
    }


}
