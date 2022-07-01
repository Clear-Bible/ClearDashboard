
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

            ProjectPath = (string.IsNullOrEmpty(FullFilePath) ? string.Empty : new FileInfo(FullFilePath).DirectoryName) ?? string.Empty;

            BaseTargetName = TargetProject?.Name;
            BaseTargetFullName = TargetProject?.FullName;
        }

       
        public string? ProjectName { get; set; }

        public string ProjectPath { get; set; } 



        public DateTimeOffset Modified { get; set; }
       
        public string? FullFilePath { get; set; }


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
    }
}
