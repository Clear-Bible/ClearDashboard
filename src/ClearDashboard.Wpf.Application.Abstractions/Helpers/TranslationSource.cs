using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.Helpers
{
    // 
    // from: https://github.com/Jinjinov/wpf-localization-multiple-resource-resx-one-language
    // license: MIT
    //
    // see also: https://github.com/Clear-Bible/GenerateTranslationsForDashboard
    //
    public class TranslationSource : INotifyPropertyChanged
    {
        private readonly Dictionary<string, ResourceManager> resourceManagerDictionary = new Dictionary<string, ResourceManager>();


        private Dictionary<string, string> TempStrings = new()
        {
            {"LexiconEdit_And", "And"},
            {"LexiconEdit_FindAll", "Find All"},
            {"LexiconEdit_Forms", "Forms"},
            {"LexiconEdit_Fully", "Fully"},
            {"LexiconEdit_FullyMatching", "Fully Matching"},
            {"LexiconEdit_Lexeme", "Lexeme"},
            {"LexiconEdit_Matching", "Matching"},
            {"LexiconEdit_Or", "Or"},
            {"LexiconEdit_Partially", "Partially"},
            {"LexiconEdit_Translation", "Translation"},

        };

        public string this[string key]
        {
            get
            {
                if (Thread.CurrentThread.CurrentUICulture.Name != language)
                {
                    Language = language;
                }

                var tuple = SplitName(key);
                string translation = null;
                if (resourceManagerDictionary.ContainsKey(tuple.Item1))
                {
                    translation = resourceManagerDictionary[tuple.Item1]
                        .GetString(tuple.Item2, Thread.CurrentThread.CurrentUICulture) ;
                }

                if (translation == null && TempStrings.ContainsKey(tuple.Item1))
                {
                    translation = TempStrings[tuple.Item1];
                }

                return translation ?? key;
            }
        }

        private string language = Thread.CurrentThread.CurrentUICulture.Name;
        public string Language
        {
            get => language;
            set
            {
                if (language != null)
                {
                    language = value;

                    var cultureInfo = new CultureInfo(value);
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        // WPF bindings register PropertyChanged event if the object supports it and update themselves when it is raised
        public event PropertyChangedEventHandler PropertyChanged;

        public void AddResourceManager(ResourceManager resourceManager)
        {
            if (!resourceManagerDictionary.ContainsKey(resourceManager.BaseName))
            {
                resourceManagerDictionary.Add(resourceManager.BaseName, resourceManager);
            }
        }

        public static Tuple<string, string> SplitName(string local)
        {
            var indexOfLastPeriod = local.LastIndexOf(".", StringComparison.Ordinal);
            var tuple = new Tuple<string, string>(local.Substring(0, indexOfLastPeriod), local.Substring(indexOfLastPeriod + 1));
            return tuple;
        }
    }

    public class Translation : DependencyObject
    {
        public static readonly DependencyProperty ResourceManagerProperty =
            DependencyProperty.RegisterAttached("ResourceManager", typeof(ResourceManager), typeof(Translation));

        public static ResourceManager GetResourceManager(DependencyObject dependencyObject)
        {
            return (ResourceManager)dependencyObject.GetValue(ResourceManagerProperty);
        }

        public static void SetResourceManager(DependencyObject dependencyObject, ResourceManager value)
        {
            dependencyObject.SetValue(ResourceManagerProperty, value);
        }
    }

    public class LocalizationExtension : MarkupExtension
    {
        private TranslationSource TranslationSource { get; set; }
        public string StringName { get; }

        public LocalizationExtension(string stringName)
        {
            StringName = stringName;
        }

        private ResourceManager GetResourceManager(object control)
        {
            if (control is DependencyObject dependencyObject)
            {
                var localValue = dependencyObject.ReadLocalValue(Translation.ResourceManagerProperty);

                // does this control have a "Translation.ResourceManager" attached property with a set value?
                if (localValue != DependencyProperty.UnsetValue)
                {
                    if (localValue is ResourceManager resourceManager)
                    {
                        TranslationSource.AddResourceManager(resourceManager);
                        return resourceManager;
                    }
                }
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {

            TranslationSource = IoC.Get<TranslationSource>();
            // targetObject is the control that is using the LocalizationExtension
            var targetObject = (serviceProvider as IProvideValueTarget)?.TargetObject;

            if (targetObject?.GetType().Name == "SharedDp") // is extension used in a control template?
                return targetObject; // required for template re-binding

            var baseName = GetResourceManager(targetObject)?.BaseName ?? string.Empty;

            if (string.IsNullOrEmpty(baseName))
            {
                // rootObject is the root control of the visual tree (the top parent of targetObject)
                var rootObject = (serviceProvider as IRootObjectProvider)?.RootObject;
                baseName = GetResourceManager(rootObject)?.BaseName ?? string.Empty;
            }

            if (string.IsNullOrEmpty(baseName)) // template re-binding
            {
                if (targetObject is FrameworkElement frameworkElement)
                {
                    baseName = GetResourceManager(frameworkElement.TemplatedParent)?.BaseName ?? string.Empty;
                }
            }

            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath($"[{baseName}.{StringName}]"),
                Source = TranslationSource,
                FallbackValue = StringName
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
