using System.Globalization;
using System.Windows;
using ClearDashboard.Wpf.Application.Converters;
using Xunit;

namespace ClearDashboard.WPF.Application.Abstractions.Tests
{
    public class EditModeToVisibilityConverterTests
    {

        [Fact]
        public void ReturnsVisibleWhenValueIsMainViewOnlyForMainViewOnlyParameterTest()
        {
            var converter = new EditModeToVisibilityConverter();

            var visibility = (Visibility)converter.Convert(EditMode.MainViewOnly, typeof(EditMode), EditMode.MainViewOnly,
                CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Visible, visibility);
        }

        [Fact]
        public void ReturnsVisibleWhenValueIsEditorViewOnlyForEditorViewOnlyParameterTest()
        {
            var converter = new EditModeToVisibilityConverter();

            var visibility = (Visibility)converter.Convert(EditMode.EditorViewOnly, typeof(EditMode), EditMode.EditorViewOnly,
                CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Visible, visibility);
        }

        [Fact]
        public void ReturnsVisibleWhenValueIsMainViewOnlyForMaiViewOnlyOrEditorViewOnlyParameterTest()
        {
            var converter = new EditModeToVisibilityConverter();

            var visibility = (Visibility)converter.Convert(EditMode.MainViewOnly, typeof(EditMode), EditMode.MainViewOnly | EditMode.EditorViewOnly,
                CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Visible, visibility);
        }

        [Fact]
        public void ReturnsVisibleWhenValueIsEditorViewOnlyForMaiViewOnlyOrEditorViewOnlyParameterTest()
        {
            var converter = new EditModeToVisibilityConverter();

            var visibility = (Visibility)converter.Convert(EditMode.EditorViewOnly, typeof(EditMode), EditMode.MainViewOnly | EditMode.EditorViewOnly,
                CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Visible, visibility);
        }

        [Fact]
        public void ReturnsCollapsedWhenValueIsManualToggleForMainViewOnlyOrEditorViewOnlyTest()
        {
            var converter = new EditModeToVisibilityConverter();

            var visibility = (Visibility)converter.Convert(EditMode.ManualToggle, typeof(EditMode), EditMode.MainViewOnly | EditMode.EditorViewOnly,
                CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, visibility);
        }
    }
}