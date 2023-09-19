using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Paratext.PluginInterfaces;
using SIL.Linq;
using SIL.Scripture;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;


namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public static class Extensions
    {
        public static ExternalNote GetExternalNote(this IProjectNote projectNote, IProject project)
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

            return new ExternalNote()
            {
                VersePlainText = versePlainText,
                SelectedPlainText = projectNote.Anchor.SelectedText,
                IndexOfSelectedPlainTextInVersePainText = indexOfSelectedPlainTextInVersePainText,
                VerseRefString = verseRef.ToString(),
                Body = SerializeNoteBody(projectNote.GetProjectNoteBody(project.GetUSFM(verseRef.BookNum, verseRef.ChapterNum)))
            };
        }

        [DataContract]
        public class BodyComment
        {
            [DataMember]
            public List<string> Paragraphs { get; set; }
            [DataMember]
            public string Created { get; set; }
            [DataMember]
            public string Language { get; set; }
            [DataMember]
            public string AssignedUserName { get; set; }
            [DataMember]
            public string Author { get; set; }
        }
        [DataContract]
        public class Body
        {
            [DataMember]
            public string AssignedUserName { get; set; }
            [DataMember]
            public string ReplyToUserName { get; set; }
            [DataMember]
            public bool IsRead { get; set; }
            [DataMember]
            public bool IsResolved { get; set; }
            [DataMember]
            public List<BodyComment> Comments { get; set; }
            [DataMember]
            public string VerseUsfmBeforeSelectedText { get; set; }
            [DataMember]
            public string VerseUsfmAfterSelectedText { get; set; }
            [DataMember]
            public string VerseUsfmText { get; set; }
        }
        private static string SerializeNoteBody(Body body)
        {
            //from https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN for
            //.net 4.x
            // Create a stream to serialize the object to.
            var ms = new MemoryStream();

            // Serializer the User object to the stream.
            var ser = new DataContractJsonSerializer(typeof(Body));
            ser.WriteObject(ms, body);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
        private static Body GetProjectNoteBody(this IProjectNote projectNote, string verseUsfmText)
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
