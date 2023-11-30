using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using SIL.Extensions;
using SIL.Linq;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public static class Extensions
    {
        private const string EXTERNAL_LABELS_FILENAME = "CommentTags.xml";
        public static ExternalNote GetExternalNote(this IProjectNote projectNote, IProject project, ILogger logger)
        {
            var verseRef = new VerseRef(projectNote.Anchor.VerseRefStart.BookNum, projectNote.Anchor.VerseRefStart.ChapterNum, projectNote.Anchor.VerseRefStart.VerseNum);
            var (versePlainText, plainTextTokensWithIndexes) = project.GetUSFMTokens(verseRef.BookNum, verseRef.ChapterNum, verseRef.VerseNum)
                .GetPlainTextTokensAndIndexes();

            int? indexOfSelectedPlainTextInVersePainText;
            if (projectNote.Anchor.Offset == 0) // a note applied to the whole verse
            {
                indexOfSelectedPlainTextInVersePainText = null;
            }
            else
            {
                var tokenOfLastSmallerOrEqualUsfmIndex = plainTextTokensWithIndexes
                        .OrderBy(i => i.indexOfTokenInVerseRawUsfm)
                        .Where(i => i.indexOfTokenInVerseRawUsfm <= projectNote.Anchor.Offset)
                        .Last();
                indexOfSelectedPlainTextInVersePainText = tokenOfLastSmallerOrEqualUsfmIndex.indexOfTokenInVersePlainText +
                    (projectNote.Anchor.Offset - tokenOfLastSmallerOrEqualUsfmIndex.indexOfTokenInVerseRawUsfm);
            }

            var body = projectNote.GetProjectNoteBody(project.GetUSFM(verseRef.BookNum, verseRef.ChapterNum));

            string externalNoteId = null;
            IEnumerable<int> tagIds = null;

            try
            {
                (externalNoteId, tagIds) = projectNote.GetExternalNoteIdAndLabelIds();
            }
            catch (InvalidCastException)
            {
                logger.LogError("Invalid cast in project note when trying to obtain thread, thread.Id, or thread.TagIds.");
            }

            return new ExternalNote()
            {
                ExternalNoteId = externalNoteId,
                ExternalProjectId = project.ID,
                ExternalLabelIds = new HashSet<int>(tagIds), //ensure only unique tag ids are added.
                VersePlainText = versePlainText,
                SelectedPlainText = projectNote.Anchor.SelectedText,
                IndexOfSelectedPlainTextInVersePainText = indexOfSelectedPlainTextInVersePainText,
                VerseRefString = verseRef.ToString(),
                Body = body.SerializeNoteBodyXml(),
                Message = body.GetMessage()
            };
        }


        /// <summary>
        /// Extracts externalNoteId and labelIds, 
        /// including converting from external system note id to externalNoteId.
        /// </summary>
        /// <param name="projectNote"></param>
        /// <returns></returns>
        public static (string externalNoteId, IEnumerable<int> labelIds) GetExternalNoteIdAndLabelIds(this IProjectNote projectNote)
        {
            string externalNoteId = null;
            IEnumerable<int> tagIds = null;

            MemberInfo[] threadMemberInfos = projectNote.GetType().GetMember("thread", BindingFlags.Instance | BindingFlags.NonPublic);
            if (threadMemberInfos.Length > 0)
            {
                var thread = ((FieldInfo)threadMemberInfos[0]).GetValue(projectNote);
                if (thread != null)
                {
                    MemberInfo[] idMemberInfos = thread.GetType().GetMember("Id");
                    if (idMemberInfos.Length > 0)
                    {
                        externalNoteId = ((PropertyInfo)idMemberInfos[0]).GetValue(thread) as string;
                    }
                    MemberInfo[] tagIdsMemberInfos = thread.GetType().GetMember("TagIds");
                    if (tagIdsMemberInfos.Length > 0)
                    {
                        tagIds = ((PropertyInfo)tagIdsMemberInfos[0]).GetValue(thread) as IEnumerable<int>;
                    }
                }
            }
            return (externalNoteId, tagIds);
        }

        /// <summary>
        /// converts from externalNote id to the external system's note id
        /// </summary>
        /// <param name="externalNote"></param>
        /// <returns></returns>
        static string GetExternalSystemNoteId(this ExternalNote externalNote)
        {
            return externalNote.ExternalNoteId;
        }

        public static ExternalNote SetExternalLabelsFromLabelTexts(
            this ExternalNote externalNote,
            ParatextProjectMetadata paratextProjectMetadata,
            ILogger logger,
            List<string> labelTexts)
        {
            var externalLabels = GetExternalLabelsFromExternalSystem(paratextProjectMetadata, logger);

            if (externalLabels == null) // project in external system doesn't have any external labels available.
                return externalNote;

            var externalLabelsToAdd = externalLabels
                .Where(el => labelTexts.Contains(el.ExternalText));

            if (externalLabelsToAdd.Count() == 0)
                return externalNote; //no labels to set

            externalNote.ExternalLabelIds = new HashSet<int>(externalLabelsToAdd
                    .Select(el => el.ExternalLabelId));

            externalNote.ExternalLabels = new HashSet<ExternalLabel>(externalLabelsToAdd);

            return externalNote;
        }

        public static ExternalNote SetExternalLabelsFromExternalLabelIds(
            this ExternalNote externalNote,
            ParatextProjectMetadata paratextProjectMetadata,
            ILogger logger)
        {
            var externalLabels = GetExternalLabelsFromExternalSystem(paratextProjectMetadata, logger);

            if (externalLabels == null) // project in external system doesn't have any external labels available.
                return externalNote;

            externalNote.ExternalLabels = new HashSet<ExternalLabel>(externalNote.ExternalLabelIds
                .Select(elid => externalLabels
                    .FirstOrDefault(el => el.ExternalLabelId == elid) ?? new ExternalLabel()
                    {
                        ExternalLabelId = elid,
                        ExternalProjectId = externalNote.ExternalProjectId,
                        ExternalProjectName = paratextProjectMetadata.GetExternalProjectName(),
                        ExternalText = $"<external label id {elid} does not have label text>",
                        ExternalTemplate = string.Empty 
                    }));

            return externalNote;
        }

        public static ExternalNote SetExternalLabelsOnExternalSystem(
            this ExternalNote externalNote, 
            ParatextProjectMetadata paratextProjectMetadata, 
            IUserInfo userInfo,
            ILogger logger)
        {
            if (externalNote.ExternalLabels == null || externalNote.ExternalLabels.Count() == 0)
                return externalNote;

            var notesFileName = $"Notes_{userInfo.Name}.xml";
            var notesFilePath = Path.Combine(paratextProjectMetadata.ProjectPath, notesFileName);
            if (!File.Exists(notesFilePath))
            {
                var message = $"Have labels to add to ExternalNoteId {externalNote.ExternalNoteId} but could not find file {notesFilePath}";
                logger.LogError(message);
                throw new Exception(message);
            }

            XElement root = XElement.Load(notesFilePath); //element CommentList
            IEnumerable<XElement> comments =
                from el in root.Elements("Comment")
                where (string)el.Attribute("Thread") == externalNote.GetExternalSystemNoteId()
                select el;

            if (comments.Count() == 0)
            {
                var message = $"Have labels to add to ExternalNoteId {externalNote.ExternalNoteId} and found file {notesFilePath} but could not find Comment with Thread={externalNote.GetExternalSystemNoteId()}";
                logger.LogError(message);
                throw new Exception(message);
            }
            else if (comments.Count() > 1)
            {
                var message = $"Have labels to add to ExternalNoteId {externalNote.ExternalNoteId} and found file {notesFilePath} but found more than one Comment with Thread={externalNote.GetExternalSystemNoteId()}";
                logger.LogError(message);
                throw new Exception(message);
            }

            comments
                .First()
                .Elements("TagAdded")
                .Select(t => t.Name = "TagRemoved")
                .ToList();
                
            comments
                .First()
                .Add(externalNote.ExternalLabels
                    .Select(el => new XElement("TagAdded", el.ExternalLabelId)));

            root.Save(notesFilePath);

            return externalNote;
        }

        public static string GetExternalProjectName(this ParatextProjectMetadata paratextProjectMetadata)
        {
            return paratextProjectMetadata.Name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paratextProjectMetadata"></param>
        /// <param name="logger"></param>
        /// <returns>null means external project doesn't have external labels available.</returns>
        public static List<ExternalLabel> GetExternalLabelsFromExternalSystem(this ParatextProjectMetadata paratextProjectMetadata, ILogger logger)
        {
            if (paratextProjectMetadata.ProjectPath == null)
            {
                var message = "GetExternalLabels paratextProjectMetadata parameter ProjectPath is null";  
                logger.LogError(message);
                throw new Exception(message);
            }

            var labelsFilePath = Path.Combine(paratextProjectMetadata.ProjectPath, EXTERNAL_LABELS_FILENAME);
            if (!File.Exists(labelsFilePath))
                return null;

            XElement root = XElement.Load(labelsFilePath);

            return
                (from t in root.Elements("Tag")
                        select new ExternalLabel()
                        {
                            ExternalLabelId = int.Parse(t.Attribute("Id").Value),
                            ExternalProjectId = paratextProjectMetadata.Id,
                            ExternalProjectName = paratextProjectMetadata.GetExternalProjectName(),
                            ExternalText = t.Attribute("Name").Value,
                            ExternalTemplate = t.Element("Template")?.Value ?? string.Empty
                        }
                )
                .ToList();
        }
        internal static Body GetProjectNoteBody(this IProjectNote projectNote, string verseUsfmText)
        {
            var body = new Body();
            body.AssignedUserName = projectNote.AssignedUser?.Name ?? "";
            body.ReplyToUserName = projectNote.ReplyToUser?.Name ?? "";
            body.IsRead = projectNote.IsRead;
            body.IsResolved = projectNote.IsResolved;
            body.VerseUsfmBeforeSelectedText = projectNote.Anchor.BeforeContext;
            body.VerseUsfmAfterSelectedText = projectNote.Anchor.AfterContext;
            body.VerseUsfmText = verseUsfmText;
            body.Comments = new List<BodyComment>();
            projectNote.Comments.ForEach(comment =>
            {
                var bodyComment = new BodyComment();
                bodyComment.Created = comment.Created.ToString();
                bodyComment.Language = comment.Language.Id;
                bodyComment.AssignedUserName = comment.AssignedUser.Name;
                bodyComment.Author = comment.Author.Name;

                bodyComment.Paragraphs = new List<string>();

                if (comment.Contents != null)
                {
                    foreach (var paragraph in comment.Contents)
                    {
                        bodyComment.Paragraphs.Add(paragraph.ToString());
                    }
                }
                body.Comments.Add(bodyComment);
            });
            return body;
        }
        public static (string versePlainText, List<(string tokenPlainText, int indexOfTokenInVersePlainText, int indexOfTokenInVerseRawUsfm)>) GetPlainTextTokensAndIndexes(
            this IEnumerable<IUSFMToken> usfmTokens)
        {
            var plainTextTokenTuples = new List<(string tokenPlainText, int indexOfTokenInVersePlainText, int indexOfTokenInVerseRawUsfm)>();
            var versePlainText = new StringBuilder();
            int indexInPlainText = 0;
            
            usfmTokens.ForEach(usfmToken =>
            {
                if (usfmToken is IUSFMTextToken textToken &&
                    textToken.IsScripture)
                {
                    plainTextTokenTuples.Add((textToken.Text, indexInPlainText, textToken.VerseOffset));

                    versePlainText.Append(textToken.Text);
                    indexInPlainText += textToken.Text.Length; //FIXME: ask dirk about TrimStart()
                }
            });
            return (versePlainText.ToString().Trim(), plainTextTokenTuples);
        }
    }
}
