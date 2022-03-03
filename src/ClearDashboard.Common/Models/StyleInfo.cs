using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ClearDashboard.Common.Models
{
    public class StyleInfo
    {
        public string Name { set; get; }
        public Style Style { set; get; }

        public StyleInfo(string name, Style style)
        {
            Name = name;
            Style = style;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object val)
        {
            if (val is StyleInfo sf)
            {
                return Name == sf.Name;
            }
            else return false;
        }

        public static bool operator ==(StyleInfo left, StyleInfo right)
        {
            if (Object.ReferenceEquals(left, right)) return true;
            if (Object.ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(StyleInfo left, StyleInfo right)
        {
            return !(left == right);
        }
    }
}
