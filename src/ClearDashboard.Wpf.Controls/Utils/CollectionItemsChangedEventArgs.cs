﻿using System;
using System.Collections;

namespace ClearDashboard.Wpf.Controls.Utils
{
    /// <summary>
    /// Arguments to the ItemsAdded and ItemsRemoved events.
    /// </summary>
    public class CollectionItemsChangedEventArgs : EventArgs
    {
        public CollectionItemsChangedEventArgs(ICollection items)
        {
            Items = items;
        }

        /// <summary>
        /// The collection of items that changed.
        /// </summary>
        public ICollection Items { get; }
    }
}
