using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Markup;
using ClearDashboard.DataAccessLayer.Models;
using SIL.Extensions;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;
        public Type EnumType
        {
            get { return _enumType; }
            set
            {
                if (value != _enumType)
                {
                    if (null != value)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum)
                            throw new ArgumentException("Type must be for an Enum.");
                    }

                    _enumType = value;
                }
            }
        }

        private string _excludedValue;
        public string ExcludedValue
        {
            get { return _excludedValue; }
            set
            {
                _excludedValue = value;
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public EnumBindingSourceExtension(Type enumType, string exludedValue)
        {
            EnumType = enumType;
            ExcludedValue = exludedValue;
        }

        static Array GetValuesWithoutItem(Type enumType, Array originalValues, object itemToRemove)
        {
            Array newArray = Array.CreateInstance(enumType, originalValues.Length - 1);

            int newIndex = 0;
            foreach (var value in originalValues)
            {
                if (!value.Equals(itemToRemove))
                {
                    newArray.SetValue(value, newIndex);
                    newIndex++;
                }
            }

            return newArray;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == this._enumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
            
            object excludedEnumValue;
            Enum.TryParse(actualEnumType, ExcludedValue, out excludedEnumValue);

            Array enumValues;
            if (excludedEnumValue != null)
            {
                enumValues = GetValuesWithoutItem(actualEnumType, Enum.GetValues(actualEnumType), excludedEnumValue);
            }
            else
            {
                enumValues = Enum.GetValues(actualEnumType);
            }

            if (actualEnumType == this._enumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
    public class EnumDefaultValueTypeConverter : EnumConverter
    {
        public EnumDefaultValueTypeConverter(Type type) : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DefaultValueAttribute[])fi.GetCustomAttributes(typeof(DefaultValueAttribute), false);
                        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Value.ToString()))) ? attributes[0].Value : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
