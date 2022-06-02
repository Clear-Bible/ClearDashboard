using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    // TODO:  Fix this properly
    public void Configure(EntityTypeBuilder<Note> entityBuilder)
    {

        entityBuilder.HasKey(e => e.Id);

        entityBuilder.Property(e => e.Created).HasColumnType("datetime");
        entityBuilder.Property(e => e.Modified).HasColumnType("datetime");

        // TODO: unremark
        entityBuilder.Property(e => e.NoteAssociations);
        entityBuilder.Property(e => e.ContentCollection);
        entityBuilder.Property(e => e.NoteRecipients);

    }
}