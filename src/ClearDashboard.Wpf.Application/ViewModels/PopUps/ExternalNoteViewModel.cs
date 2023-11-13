using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MahApps.Metro.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class ExternalNoteViewModel : DashboardApplicationScreen
    {
        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private BindableCollection<ExternalNoteExtended> _externalNotes = new();
        public BindableCollection<ExternalNoteExtended> ExternalNotes
        {
            get => _externalNotes;
            set => Set(ref _externalNotes, value);
        }

        #endregion //Observable Properties


        #region Constructor

        public ExternalNoteViewModel()
        {
              //no-op
        }

        public ExternalNoteViewModel(INavigationService navigationService, ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
                
        }

        public void Initialize(BindableCollection<ExternalNote> externalNotes)
        {
            foreach (var externalNote in externalNotes)
            {
                var items = ExtractBody(externalNote.Body);

                ExternalNotes.Add(new ExternalNoteExtended
                {
                    ExternalNoteId = externalNote.ExternalNoteId,
                    ExternalProjectId = externalNote.ExternalProjectId,
                    LabelIds = externalNote.LabelIds,
                    VersePlainText = externalNote.VersePlainText,
                    SelectedPlainText = externalNote.SelectedPlainText,
                    IndexOfSelectedPlainTextInVersePainText = externalNote.IndexOfSelectedPlainTextInVersePainText,
                    VerseRefString = externalNote.VerseRefString,
                    Message = externalNote.Message,
                    AssignedUser = items.Item1,
                    BodyComments = items.Item2,
                    IsResolved = items.Item3,
                    Inlines = new List<Inline>()
                });
            }

            NotifyOfPropertyChange(nameof(ExternalNotes));
        }

        #endregion //Constructor


        #region Methods

        private Tuple<string, List<BodyComment>, bool> ExtractBody(string xmlString)
        {
            var str = XElement.Parse(xmlString);

            // get the value of the AssignedUserName element
            var assignedUser = str.Element("Body").Element("AssignedUserName").Value;

            var isResolved = str.Element("Body").Element("IsResolved").Value;
            bool isResolvedBool = false;
            if (isResolved == "true")
            {
                isResolvedBool = true;
            }

            // get all the body comments
            var comments = str.Element("Body").Element("Comments").Elements("BodyComment").ToList();

            List<BodyComment> bodyComments = new List<BodyComment>();
            foreach (var element in comments)
            {
                bodyComments.Add(new BodyComment(element));
            }

            // Create a 3-tuple and return it
            var author = new Tuple<string,
                List<BodyComment>, bool>(assignedUser, bodyComments, isResolvedBool);
            return author;
        }


        #endregion // Methods

        #region IHandle

        #endregion // IHandle

    }


    public class ExternalNoteExtended: ExternalNote
    {
        public List<Inline> Inlines { get; set; }
        public string AssignedUser { get; set; }
        public List<BodyComment> BodyComments { get; set; }
        public bool IsResolved { get; set; }

    }


    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class BodyComment
    {
        public BodyComment(XElement element)
        {
            Created = element.Element("Created").Value;
            Language = element.Element("Language").Value;
            Author = element.Element("Author").Value;

            var bodyStrings = element.Element("Paragraphs").Elements("string").ToList();

            string body = "";
            foreach (var bodyString in bodyStrings)
            {
                if (bodyString.Value != null)
                {
                    body += bodyString.Value + Environment.NewLine;
                }
                else
                {
                    body += Environment.NewLine;
                }
            }

            Paragraphs = new BodyCommentParagraphs
            {
                @string = body
            };
        }

        public DateTime CreatedDateTime => DateTime.Parse(Created);

        private BodyCommentParagraphs paragraphsField;

        private string createdField;

        private string languageField;

        private string authorField;

        /// <remarks/>
        public BodyCommentParagraphs Paragraphs
        {
            get => this.paragraphsField;
            set => this.paragraphsField = value;
        }

        /// <remarks/>
        public string Created
        {
            get
            {
                return this.createdField;
            }
            set
            {
                this.createdField = value;
            }
        }

        /// <remarks/>
        public string Language
        {
            get
            {
                return this.languageField;
            }
            set
            {
                this.languageField = value;
            }
        }

        /// <remarks/>
        public string Author
        {
            get
            {
                return this.authorField;
            }
            set
            {
                this.authorField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class BodyCommentParagraphs
    {

        private string stringField;

        /// <remarks/>
        public string @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }
    }



}
