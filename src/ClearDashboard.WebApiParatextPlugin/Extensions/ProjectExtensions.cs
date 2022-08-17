
using System.Collections.Generic;
using System.Linq;
using ClearDashboard.DataAccessLayer.Models;
using Paratext.PluginInterfaces;

namespace ClearDashboard.WebApiParatextPlugin.Extensions
{
    internal static class ProjectExtensions
    {

        //public static Dictionary<string, string> CreateBookChapterVerseDictionary(this IProject project)
        //{
        //    var dictionary = new Dictionary<string, string>();


        //    return dictionary;
        //}

        public static List<BookInfo> GetAvailableBooks(this IProject project)
        {
            return project.AvailableBooks.Select(book => new BookInfo { Code = book.Code, InProjectScope = book.InProjectScope, Number = book.Number, }).ToList();
        }
    }
}
