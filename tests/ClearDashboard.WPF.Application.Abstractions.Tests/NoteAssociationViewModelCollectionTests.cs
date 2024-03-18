using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using Xunit;

namespace ClearDashboard.WPF.Application.Abstractions.Tests
{
    public  class NoteAssociationViewModelCollectionTests
    {


        [Fact]
        public void SortTest()
        {

            var association1 = new NoteAssociationViewModel
            {
                Book = "2",
                Chapter = "13",
                Verse = "17",
                Word = "7",
                Part = "1",
            };

            var association2 = new NoteAssociationViewModel
            {
                Book = "1",
                Chapter = "1",
                Verse = "1",
                Word = "1",
                Part = "1",
            };

            var collection = new NoteAssociationViewModelCollection
            {
               association1,association2
               
            };

            var orderedList = collection.OrderBy(c => c.SortOrder);

            var newCollection = new NoteAssociationViewModelCollection(orderedList);

            Assert.Equal(association2.SortOrder, newCollection[0].SortOrder);
            Assert.Equal(association1.SortOrder, newCollection[1].SortOrder);

        }


        [Fact]
        public void MissingDataSortTest()
        {

            var association1 = new NoteAssociationViewModel
            {
                Book = "2",
                Chapter = "13",
                Verse = "17",
                Part = "1",
            };

            var association2 = new NoteAssociationViewModel
            {
                Book = "1",
                Chapter = "1",
                Verse = "1",
            };

            var collection = new NoteAssociationViewModelCollection
            {
                association1,association2

            };

            var orderedList = collection.OrderBy(c => c.SortOrder);

            var newCollection = new NoteAssociationViewModelCollection(orderedList);

            Assert.Equal(association2.SortOrder, newCollection[0].SortOrder);
            Assert.Equal(association1.SortOrder, newCollection[1].SortOrder);

        }
    }
}
