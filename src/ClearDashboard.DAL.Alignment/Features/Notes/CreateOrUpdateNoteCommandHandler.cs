﻿using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class CreateOrUpdateNoteCommandHandler : ProjectDbContextCommandHandler<CreateOrUpdateNoteCommand,
        RequestResult<NoteId>, NoteId>
    {
        private readonly IMediator _mediator;
        private readonly IUserProvider _userProvider;

        public CreateOrUpdateNoteCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider,
            IUserProvider userProvider,
            ILogger<CreateOrUpdateNoteCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
            _userProvider = userProvider;
        }

        protected override async Task<RequestResult<NoteId>> SaveDataAsync(CreateOrUpdateNoteCommand request,
            CancellationToken cancellationToken)
        {
            Models.Note? note = null;
            if (request.NoteId != null)
            {
                note = ProjectDbContext!.Notes
                    .Include(n => n.User)
                    .Include(n => n.NoteUserSeenAssociations)
                    .FirstOrDefault(c => c.Id == request.NoteId.Id);
                if (note == null)
                {
                    return new RequestResult<NoteId>
                    (
                        success: false,
                        message: $"Invalid NoteId '{request.NoteId.Id}' found in request"
                    );
                }

                note.Text = request.Text;
                note.AbbreviatedText = request.AbbreviatedText;
                note.Modified = DateTimeOffset.UtcNow;
                note.NoteStatus = request.NoteStatus;

                var seenByUserIdsToAdd = request.SeenByUserIds.Except(note.NoteUserSeenAssociations.Select(e => e.UserId)).Distinct();
                foreach (var userId in seenByUserIdsToAdd)
                {
                    ProjectDbContext.NoteUserSeenAssociations.Add(new Models.NoteUserSeenAssociation
                    {
                        UserId = userId,
                        NoteId = request.NoteId.Id
                    });
                }

                var noteSeenUserAssociationsToRemove = note.NoteUserSeenAssociations.ExceptBy(request.SeenByUserIds, n => n.UserId);
                ProjectDbContext.RemoveRange(noteSeenUserAssociationsToRemove);

                // DO NOT MODIFY note.ThreadId once it is set during note creation
            }
            else
            {
                note = new Models.Note
                {
                    Id = Guid.NewGuid(),
                    Text = request.Text,
                    AbbreviatedText = request.AbbreviatedText,
                    NoteStatus = request.NoteStatus
                };

                // Validate ThreadId:
                if (request.ThreadId is not null)
                {
                    var leadNoteInThread = ProjectDbContext!.Notes
                        .FirstOrDefault(n => n.Id == request.ThreadId.Id && n.ThreadId == null || n.ThreadId == request.ThreadId.Id);
                    if (leadNoteInThread is not null)
                    {
                        if (leadNoteInThread.ThreadId is not null && leadNoteInThread.ThreadId != request.ThreadId.Id)
                        {
                            return new RequestResult<NoteId>
                            (
                                success: false,
                                message: $"Note referred by ThreadId '{request.ThreadId.Id}' found in request is already itself a reply note"
                            );
                        }

                        leadNoteInThread.ThreadId = request.ThreadId.Id;
                    }

                    // A note's ThreadId does not have to refer to the Id of a Note
                    // (the lead note of a thread might have been deleted):
                    note.ThreadId = request.ThreadId.Id;
                }

                ProjectDbContext.Notes.Add(note);

                if (_userProvider.CurrentUser is not null && !request.SeenByUserIds.Contains(_userProvider.CurrentUser.Id))
                {
                    request.SeenByUserIds.Add(_userProvider.CurrentUser.Id);
                }

                foreach (var userId in request.SeenByUserIds.Distinct())
                {
                    ProjectDbContext.NoteUserSeenAssociations.Add(new Models.NoteUserSeenAssociation
                    {
                        UserId = userId,
                        NoteId = note.Id
                    });
                }
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            note = ProjectDbContext.Notes.Include(n => n.User).First(n => n.Id == note.Id);

            return new RequestResult<NoteId>(ModelHelper.BuildNoteId(note));
        }
    }
}