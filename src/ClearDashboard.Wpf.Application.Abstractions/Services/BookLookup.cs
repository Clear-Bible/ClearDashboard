using System;
using System.Collections.Generic;
using System.Linq;
using static ClearBible.Engine.Persistence.FileGetBookIds;

namespace ClearDashboard.Wpf.Application.Services
{
    public static class BookLookup
    {
        /// <summary>
        /// A dictionary mapping book numbers to book abbreviations, cached for direct indexing by integer.
        /// </summary>
        private static string[] BookAbbreviations { get; }

        /// <summary>
        /// Gets the three-character canonical abbreviation for a book number.
        /// </summary>
        /// <param name="bookNumber">The SIL canonical book number.</param>
        /// <returns>The three-character canonical abbreviation for the book.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided book number is out of range of the valid books.</exception>
        public static string GetBookAbbreviation(int bookNumber)
        {
            return BookAbbreviations[bookNumber];
        }

        static BookLookup()
        {
            var maxBookNumber = BookIds.Max(id => int.Parse(id.silCannonBookNum));
            BookAbbreviations = new string[maxBookNumber + 1];
            foreach (var bookId in BookIds)
            {
                BookAbbreviations[int.Parse(bookId.silCannonBookNum)] = bookId.silCannonBookAbbrev;
            }
        }
    }
}
