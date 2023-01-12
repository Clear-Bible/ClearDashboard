using System;
using System.Collections.Generic;
using System.Windows.Media;
using ClearBible.Engine.Corpora;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// Maintains a map between <see cref="CompositeToken"/> instances and the colors used to designate their constituent tokens.
    /// </summary>
    /// <remarks>
    /// These color mappings are only maintained for the lifetime of the application; they are not persisted across application invocations.
    /// </remarks>
    public static class CompositeTokenColors
    {
        private static readonly Dictionary<Guid, Brush> _colorMap = new();
        private static readonly List<Brush> _colorList = new()
        {
            Brushes.YellowGreen,
            Brushes.Violet,
            Brushes.Turquoise,
            Brushes.Sienna,
            Brushes.ForestGreen,
            Brushes.Teal,
            Brushes.Coral,
            Brushes.Magenta,
            Brushes.Lime,
            Brushes.Indigo,
            Brushes.Yellow,
            Brushes.Tomato,
            Brushes.BlueViolet,
            Brushes.CadetBlue,
            Brushes.Chocolate,
            Brushes.DarkKhaki,
            Brushes.DarkSeaGreen,
            Brushes.DarkSlateGray,
            Brushes.Goldenrod,
            Brushes.Olive
        };
        private static int _colorIndex;

        /// <summary>
        /// Retrieves a color to be used for the display of a composite token, mapping a new color if necessary.
        /// </summary>
        /// <param name="key">The <see cref="Guid"/> of the composite token.</param>
        /// <returns>A <see cref="Brush"/> to be used for displaying the composite token's constituents.</returns>
        private static Brush Get(Guid key)
        {
            if (!_colorMap.ContainsKey(key))
            {
                _colorMap[key] = _colorList[_colorIndex++];
                if (_colorIndex >= _colorList.Count)
                {
                    _colorIndex = 0;
                }
            }
            return _colorMap[key];
        }

        /// <summary>
        /// Retrieves a color to be used for the display of a composite token, mapping a new color if necessary.
        /// </summary>
        /// <param name="token">An optional composite token for which to determine an color.</param>
        /// <returns>A <see cref="Brush"/> to be for displaying the composite token's constituents, or <see cref="Brushes.Transparent"/> if no composite token is provided.</returns>
        public static Brush Get(CompositeToken? token)
        {
            return token != null ? Get(token.TokenId.Id) : Brushes.Transparent;
        }
    }
}
