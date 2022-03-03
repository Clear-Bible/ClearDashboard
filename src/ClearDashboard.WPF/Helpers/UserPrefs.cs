using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using Newtonsoft.Json;

namespace ClearDashboard.Wpf.Helpers
{
    public class UserPrefs
    {
        [JsonProperty] public int ValidationLevel = 0;

        [JsonProperty] public EViewType ViewType { get; set; } = EViewType.Target;
        [JsonProperty] public bool IsOt { get; set; } = true;

        [JsonProperty] public string AlignmentLastVerse { get; set; } = "";

        [JsonProperty] public string AlignmentSuggestionLastVerse { get; set; } = "";

        [JsonProperty] public string SelectedGlossShortName { get; set; } = "";

        [JsonProperty] public List<SelectedBook> SelectedBooks { get; set; } = new List<SelectedBook>();

        [JsonProperty] public List<string> GlossLicenses { get; set; } = new List<string>();

        [JsonProperty] private int _lastContentWordLevel;

        public int LastContentWordLevel
        {
            get => _lastContentWordLevel;
            set
            {
                if (value < 0)
                {
                    _lastContentWordLevel = 0;
                }
                else if (value > 5)
                {
                    _lastContentWordLevel = 5;
                }
                else
                {
                    _lastContentWordLevel = value;
                }
            }
        }


        public UserPrefs LoadUserPrefFile(DashboardProject dashboardProject)
        {
            FileInfo fi = new FileInfo(dashboardProject.FullFilePath);
            string path = Path.Combine(fi.DirectoryName, "prefs.jsn");

            // make a default version
            UserPrefs up = new UserPrefs();
            up.ValidationLevel = 0;

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                up = JsonConvert.DeserializeObject<UserPrefs>(json);

                //check for null meaning that there was a problem with the file
                if (up is null)
                {
                    UserPrefs upNew = new UserPrefs();
                    SaveUserPrefFile(upNew, new DashboardProject
                    {
                        FullFilePath = path,
                    });
                    up = upNew;
                }

                if (up.SelectedBooks is null)
                {
                    up.SelectedBooks = new List<SelectedBook>();
                }

                return up;
            }

            return up;
        }

        public bool SaveUserPrefFile(UserPrefs up, DashboardProject dashboardProject)
        {
            UserPrefs upNew = new UserPrefs();
            upNew.LastContentWordLevel = up.LastContentWordLevel;
            upNew.IsOt = up.IsOt;
            upNew.ValidationLevel = up.ValidationLevel;
            upNew.ViewType = up.ViewType;
            upNew.AlignmentLastVerse = up.AlignmentLastVerse;
            upNew.GlossLicenses = up.GlossLicenses;
            upNew.SelectedGlossShortName = up.SelectedGlossShortName;

            FileInfo fi = new FileInfo(dashboardProject.FullFilePath);
            string path = Path.Combine(fi.DirectoryName, "prefs.jsn");

            try
            {
                // serialize JSON directly to a file
                using (StreamWriter file = File.CreateText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    serializer.Serialize(file, upNew);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}
