using ClearDashboard.Wpf.Application.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace ClearDashboard.Wpf.Application.UserControls
{


    /// <summary>
    /// Control for showing MARBLE links within the text for the short description field
    /// </summary>
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    public class MarbleLinkControl : Control
    {
        #region Member Variables

        // regex for finding groups of {}
        private static readonly Regex _regex = new(@"\{([^}]+)\}", RegexOptions.Compiled);
        private TextBlock _textBlock;
        private enum LinkType
        {
            TextOnly,
            ActiveLink,
        }

        #endregion //Member Variables


        #region Commands

        public ICommand LinkCommand
        {
            get => (ICommand)GetValue(LinkCommandProperty);
            set => SetValue(LinkCommandProperty, value);
        }

        #endregion


        #region Public Properties

        public static readonly DependencyProperty LinkCommandProperty =
            DependencyProperty.Register("LinkCommand", typeof(ICommand), typeof(MarbleLinkControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MarbleLinkControl),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        #endregion //Public Properties


        #region Observable Properties
        
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion //Observable Properties


        #region Constructor

        static MarbleLinkControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MarbleLinkControl),
                new FrameworkPropertyMetadata(typeof(MarbleLinkControl)));
        }

        #endregion //Constructor


        #region Methods


        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lControl = d as MarbleLinkControl;
            if (!(d is MarbleLinkControl) || lControl._textBlock == null)
            {
                return;
            }
            lControl.ResetText();
        }

        private void ResetText()
        {
            _textBlock.Inlines.Clear();
            _textBlock.TextWrapping = TextWrapping.Wrap;
            if (string.IsNullOrWhiteSpace(Text))
            {
                return;
            }

            // find all the {} links
            var matches = _regex.Matches(Text);
            var cursor = 0;

            if (matches.Count == 0)
            {
                _textBlock.Inlines.Add(new Run(Text));
            }
            else
            {
                // iterate over each link checking their type
                foreach (var match in matches.OfType<Match>())
                {
                    // add in the previous text
                    var text = Text.Substring(cursor, match.Index - cursor);
                    _textBlock.Inlines.Add(new Run(text));

                    // remove brackets
                    string displayText = "";
                    string linkText = "";
                    var innerText = match.Value.Replace("{", "");
                    innerText = innerText.Replace("}", "");
                    LinkType linkType = LinkType.ActiveLink;

                    Debug.WriteLine(innerText.Substring(0, 2).ToUpperInvariant());

                    switch (innerText.Substring(0, 2).ToUpperInvariant())
                    {
                        case "L:": // link
                            displayText = innerText.Substring(2, innerText.IndexOf("<", StringComparison.Ordinal) - 2);
                            linkText = innerText.Substring(innerText.IndexOf("<", StringComparison.Ordinal));
                            linkText = linkText.Substring(6);
                            linkText = "L:" + linkText.Replace(">", "");
                            break;
                        case "A:": // abbreviation to Anchor Bible Dictionary
                            displayText = innerText.Substring(2);
                            linkText = "A:" + innerText[2..];
                            linkType = LinkType.TextOnly; // just show that it is different but not linkable
                            break;
                        case "S:": // scripture reference
                            displayText = innerText.Substring(2);
                            linkText = "S:" + innerText[2..];
                            break;
                        case "N:": // footnote caller
                            break;
                        case "D:"
                            : // SDBG refers to certain entries using the domain number found EntryCode '<LEXMeaning Id = "004903001001000" EntryCode = "7.11">'
                            displayText = innerText.Substring(2);
                            linkText = innerText;
                            break;
                    }

                    if (linkType == LinkType.ActiveLink)
                    {
                        // create the new link
                        var link = new Hyperlink();
                        link.Inlines.Add(new TextBlock { Text = displayText });
                        link.Tag = linkText;
                        link.Click += OnLinkClick;
                        _textBlock.Inlines.Add(link);
                    }
                    else
                    {
                        // just regular blue text without a hyperlink
                        _textBlock.Inlines.Add(new Run(displayText) { Foreground = Brushes.Blue });
                    }

                    cursor = match.Index + match.Length;
                }
            }
            // add on the rest of the text

        }

        private void OnLinkClick(object sender, RoutedEventArgs e)
        {
            var linkVal = (sender as Hyperlink)?.Tag;
            if (linkVal == null || LinkCommand == null)
            {
                return;
            }

            var split = linkVal?.ToString()!.Split(':');
            if (split.Count() != 2)
            {
                return;
            }

            // TODO Raise events

            switch (split![0])
            {
                case "L": // link
                    LinkCommand.Execute(split![1]);
                    break;
                case "S":
                    break;
                case "N":
                    break;
                case "D":
                    break;


            }



            //LinkCommand.Execute(linkVal);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _textBlock = (GetTemplateChild("PART_TextBlock") as TextBlock)!;  // see \Themes\Generic.xaml
            ResetText();
        }

        #endregion // Methods
    }
}
