using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClearDashboard.DataAccessLayer.Models
{
    public static class ENums
    {
        /// <summary>
        /// Used to keep track of what type of object a NoteTagModel is. 
        /// </summary>
        public enum NoteTagType
        {
            [Description("Note")]
            [Display(Name = "Note")]
            Note = 1,

            [Description("Note")]
            [Display(Name = "Tag")]
            Tag = 2
        }
    }
}
