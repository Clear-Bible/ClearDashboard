
namespace ClearDashboard.DataAccessLayer.Models
{
    public class DashboardProject 
    {
        public string? DirectoryPath { get; set; }
        //TODO:  fix this to use the correct CLear.Engine directory path
        // public string ClearEngineDirectoryPath => Path.Combine(TargetProject.DirectoryPath, "ClearEngine");
        public bool HasJsonProjectName { get; set; } = false;

        public DashboardProject()
        {
            LanguageOfWiderCommunicationProjects = new List<ParatextProject>();
            BackTranslationProjects = new List<ParatextProject>();
            InterlinearizerProject = null;

            ParatextProjectPath = (string.IsNullOrEmpty(FullFilePath) ? string.Empty : new FileInfo(FullFilePath).DirectoryName) ?? string.Empty;

            BaseTargetName = TargetProject?.Name;
            BaseTargetFullName = TargetProject?.LongName;
        }

        public bool IsClosed { get; set; } = true;
        public bool IsNew { get; set; }
        public string? ProjectName { get; set; }

        public string ParatextProjectPath { get; set; }

        public string Version { get; set; }

        public string? GitLabSha { get; set; }

        public bool GitLabUpdateNeeded { get; set; } = false;
        public bool IsCompatibleVersion { get; set; } = true;

        public DateTimeOffset Modified { get; set; }
       
        public string? FullFilePath { get; set; }

        public string? CompactFilePath =>
            string.IsNullOrEmpty(FullFilePath) ? string.Empty : ShrinkPath(FullFilePath, 76);

        private string ShrinkPath(string path, int maxLength = 64)
        {
            var parts = path.Split('\\');
            var output = String.Join("\\", parts, 0, parts.Length);
            var endIndex = (parts.Length - 1);
            var startIndex = endIndex / 2;
            var index = startIndex;
            var step = 0;

            while (output.Length >= maxLength && index != 0 && index != endIndex)
            {
                parts[index] = "...";
                output = String.Join("\\", parts, 0, parts.Length);
                if (step >= 0) step++;
                step = (step * -1);
                index = startIndex + step;
            }
            return output;
        }


        /// <summary>
        /// the target project
        /// </summary>

        public ParatextProject? TargetProject { get; set; }

        public ParatextProject? InterlinearizerProject { get; set; }

        /// <summary>
        /// list of LWC projects
        /// </summary>
        public List<ParatextProject> LanguageOfWiderCommunicationProjects { get; set; } = new List<ParatextProject>();

        /// <summary>
        /// List of Back Translation projects
        /// </summary>
        public List<ParatextProject> BackTranslationProjects { get; set; }

        /// <summary>
        /// The Paratext UserID of the creator
        /// </summary>
        public string? ParatextUser { get; set; }

        /// <summary>
        /// Date that this project was created
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        public string? BaseTargetName { get; set; }

        /// <summary>
        /// The Dashboard Project FullName
        /// </summary>
        public string? BaseTargetFullName { get; set; }
        
        public string? ShortFilePath { get; set; }

        public string? JsonProjectName { get; set; }

        public int UserValidationLevel { get; set; }

        public int LastContentWordLevel { get; set; }

        public bool ValidateProjectData()
        {
            if (ProjectName is "" or null)
            {
                return false;
            }

            // check to see if we have at least a target project
            return TargetProject is not null;
        }

        public bool HasProjectPath => !string.IsNullOrEmpty(ProjectName);

        public bool HasFullFilePath => !string.IsNullOrEmpty(FullFilePath);

        public bool NeedsMigrationUpgrade { get; set; } = false;
        public bool IsCollabProject { get; set; }

        public string CollabOwner { get; set; }
        public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.Owner;
        public Guid Id { get; set; }
    }
}
