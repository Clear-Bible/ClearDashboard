using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Pipes_Shared.Models
{
    [Serializable]
    public struct Enum<T> : IComparable<Enum<T>> where T : class, IEnumType
    {
        public static readonly Enum<T> Null = new Enum<T>((string)null);
        private static string defaultValue_;
        private static HashSet<string> knownValues_;
        private string _internalValue;

        public Enum(string value) => this._internalValue = value;

        public static void SetDefault(string value) => Enum<T>.defaultValue_ = value;

        [XmlText]
        public string InternalValue
        {
            get => this._internalValue ?? Enum<T>.defaultValue_;
            set => this._internalValue = value;
        }

        public static bool IsKnownValue(Enum<T> val)
        {
            if (Enum<T>.knownValues_ == null)
            {
                Enum<T>.knownValues_ = new HashSet<string>();
                Type type = typeof(T);
                foreach (FieldInfo fieldInfo in ((IEnumerable<FieldInfo>)type.GetFields()).Where<FieldInfo>((Func<FieldInfo, bool>)(f => f.IsStatic && f.IsPublic)))
                {
                    if (fieldInfo.GetValue((object)type) is Enum<T> @enum)
                        Enum<T>.knownValues_.Add(@enum.InternalValue);
                }
            }
            return Enum<T>.knownValues_.Contains(val.InternalValue);
        }

        public override int GetHashCode() => this.InternalValue == null ? 0 : this.InternalValue.GetHashCode();

        public override bool Equals(object obj) => obj is Enum<T> @enum && @enum.InternalValue == this.InternalValue;

        public int CompareTo(Enum<T> other) => string.Compare(this.InternalValue, other.InternalValue, StringComparison.Ordinal);

        public override string ToString() => this.InternalValue;

        public static bool operator ==(Enum<T> se1, Enum<T> se2) => se1.InternalValue == se2.InternalValue;
        public static bool operator !=(Enum<T> se1, Enum<T> se2) => !(se1 == se2);
    }
}
