using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class InterlinearNoteConfiguration : IEntityTypeConfiguration<InterlinearNote>
{
    public void Configure(EntityTypeBuilder<InterlinearNote> entityBuilder)
    {
        entityBuilder.HasKey(e => e.Id);

        entityBuilder.HasIndex(e => e.UserId).IsUnique();

        entityBuilder.Property(e => e.Created)
            .HasColumnType("datetime");

        entityBuilder.Property(e => e.Note).HasColumnType("varchar(600)");

        entityBuilder.Property(e => e.TokenId);

        entityBuilder.Property(e => e.UserId);


        entityBuilder.HasOne(d => d.Token)
            .WithMany(p => p.InterlinearNotes)
            .HasPrincipalKey(p => p.Id)
            .HasForeignKey(d => d.TokenId);
    }
}