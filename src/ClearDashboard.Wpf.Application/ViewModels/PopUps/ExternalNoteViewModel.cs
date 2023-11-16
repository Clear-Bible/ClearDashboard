using Autofac;
using Caliburn.Micro;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using ClearDashboard.ParatextPlugin.CQRS.Features.Users;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class ExternalNoteViewModel : DashboardApplicationScreen
    {
        private readonly ILogger<AboutViewModel> _logger;
        private readonly IMediator _mediator;

        #region Member Variables   

        private string _externalProjectId;
        private string _externalNoteId;
        private string _verseRef;

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private ObservableCollection<string> _assignableUsers = new();
        public ObservableCollection<string> AssignableUsers
        {
            get => _assignableUsers;
            set
            {
                _assignableUsers = value;
                NotifyOfPropertyChange(() => AssignableUsers);
            }
        }


        private string _selectedAssignableUser = string.Empty;
        public string? SelectedAssignableUser
        {
            get => _selectedAssignableUser;
            set
            {
                _selectedAssignableUser = value;
                NotifyOfPropertyChange(() => SelectedAssignableUser);
            }
        }


        private BindableCollection<ExternalNoteExtended> _externalNotes = new();
        public BindableCollection<ExternalNoteExtended> ExternalNotes
        {
            get => _externalNotes;
            set => Set(ref _externalNotes, value);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string ReplyText { get; set; } = string.Empty;

        #endregion //Observable Properties


        #region Constructor

        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ExternalNoteViewModel()
        {
            //no-op
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public ExternalNoteViewModel(INavigationService navigationService,
            ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Initialize(BindableCollection<ExternalNote> externalNotes)
        {
            foreach (var externalNote in externalNotes)
            {
                var items = ExtractBody(externalNote.Body);

                var inlines = GenerateVerseInlines(externalNote);

                _externalNoteId = externalNote.ExternalNoteId;
                _externalProjectId = externalNote.ExternalProjectId;
                _verseRef = externalNote.VerseRefString;

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
                    Inlines = inlines
                });
            }

            NotifyOfPropertyChange(nameof(ExternalNotes));

            var result = await ExecuteRequest(new GetAllEditableProjectUsersQuery(_externalProjectId), CancellationToken.None);
            if (result.Success && result.HasData)
            {
                if (result.Data != null) 
                    AssignableUsers = new ObservableCollection<string>(result.Data);
            }

            NotifyOfPropertyChange(nameof(AssignableUsers));
            SelectedAssignableUser = null;
        }

        #endregion //Constructor


        #region Methods

        /// <summary>
        /// do the verse context highlighting
        /// </summary>
        /// <param name="externalNote"></param>
        /// <returns></returns>
        private ObservableCollection<Inline> GenerateVerseInlines(ExternalNote externalNote)
        {
            var inlines = new ObservableCollection<Inline>();

            var firstPart = externalNote.VersePlainText.Substring(0, (int)externalNote.IndexOfSelectedPlainTextInVersePainText!);
            var secondPart = externalNote.VersePlainText.Substring((int)externalNote.IndexOfSelectedPlainTextInVersePainText + externalNote.SelectedPlainText.Length);


            inlines.Add(new Run(firstPart) { FontWeight = FontWeights.Normal, Foreground = Brushes.Gray });
            inlines.Add(new Run(externalNote.SelectedPlainText) { FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
            inlines.Add(new Run(secondPart) { FontWeight = FontWeights.Normal, Foreground = Brushes.Gray });

            return inlines;
        }


        /// <summary>
        /// Extracts components from the xml string
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        private Tuple<string, List<BodyComment>, bool> ExtractBody(string xmlString)
        {
            var str = XElement.Parse(xmlString);

            // get the value of the AssignedUserName element
            var assignedUser = str.Element("Body")!.Element("AssignedUserName")!.Value;
            if (string.IsNullOrEmpty(assignedUser))
            {
                assignedUser = "Unassigned";
            }

            var isResolved = str.Element("Body")!.Element("IsResolved")!.Value;
            // ReSharper disable once ReplaceWithSingleAssignment.False
            bool isResolvedBool = false;
            if (isResolved == "true")
            {
                isResolvedBool = true;
            }

            // get all the body comments
            var comments = str.Element("Body")!.Element("Comments")!.Elements("BodyComment").ToList();

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


        /// <summary>
        /// Saves the external note and or selected assigned user
        /// </summary>
        public async void Ok()
        {
            if (string.IsNullOrEmpty(ReplyText) == false || SelectedAssignableUser != "")
            {
                var comment = string.Empty;
                if (string.IsNullOrEmpty(ReplyText))
                {
                    comment = $"- Assigned to user {SelectedAssignableUser} -";
                }
                else
                {
                    comment = ReplyText;
                }

                var result = await ExternalNoteManager.AddNewCommentToExternalNote(_mediator, _externalProjectId,
                    _externalNoteId, _verseRef, comment, SelectedAssignableUser, _logger);

                if (result)
                {
                    await EventAggregator.PublishOnUIThreadAsync(new RefreshExternalNotesMessage(_externalProjectId));

                    await this.TryCloseAsync();
                }
            }
        }


        /// <summary>
        /// Closes the dialog
        /// </summary>
        public async void Close()
        {
            await this.TryCloseAsync();
        }


        /// <summary>
        /// Resolves the external note
        /// </summary>
        public async void Resolve()
        {
            var result = await ExternalNoteManager.ResolveExternalNote(_mediator, _externalProjectId, _externalNoteId, _verseRef, _logger);

            if (result)
            {
                await EventAggregator.PublishOnUIThreadAsync(new RefreshExternalNotesMessage(_externalProjectId));

                await this.TryCloseAsync();
            }
        }

        #endregion // Methods
    }


    public class ExternalNoteExtended : ExternalNote, INotifyPropertyChanged
    {
        private ObservableCollection<Inline> _inlines = new();
        public ObservableCollection<Inline> Inlines
        {
            get => _inlines;
            set
            {
                _inlines = value;
                OnPropertyChanged();
            }
        }

        public string AssignedUser { get; set; } = string.Empty;
        public List<BodyComment> BodyComments { get; set; } = new();
        public bool IsResolved { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    [Serializable()]
    [DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class BodyComment
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public BodyComment(XElement element)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Created = element.Element("Created")!.Value;
            Language = element.Element("Language")!.Value;
            Author = element.Element("Author")!.Value;

            var bodyStrings = element.Element("Paragraphs")!.Elements("string").ToList();

            string body = "";
            foreach (var bodyString in bodyStrings)
            {
                if (bodyString.Value is not null)
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

        private string _createdField = string.Empty;

        private string _languageField = string.Empty;

        private string _authorField = string.Empty;

        /// <remarks/>
        public BodyCommentParagraphs Paragraphs
        {
            get => this.paragraphsField;
            set => this.paragraphsField = value;
        }

        /// <remarks/>
        public string Created
        {
            get => this._createdField;
            set => this._createdField = value;
        }

        /// <remarks/>
        public string Language
        {
            get => this._languageField;
            set => this._languageField = value;
        }

        /// <remarks/>
        public string Author
        {
            get => this._authorField;
            set => this._authorField = value;
        }
    }

    /// <remarks/>
    [SerializableAttribute()]
    [DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class BodyCommentParagraphs
    {
        private string _stringField = string.Empty;

        /// <remarks/>
        public string @string
        {
            get => this._stringField;
            set => this._stringField = value;
        }
    }



}
