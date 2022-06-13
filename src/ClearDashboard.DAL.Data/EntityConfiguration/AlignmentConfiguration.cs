using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class AlignmentConfiguration : IEntityTypeConfiguration<Alignment>
{
    public void Configure(EntityTypeBuilder<Alignment> entityBuilder)
    {
        entityBuilder.HasKey(e => e.Id);

        entityBuilder.HasIndex(e => e.SourceTokenId).IsUnique();
        entityBuilder.HasIndex(e => e.TargetTokenId).IsUnique();

        entityBuilder.Property(e => e.AlignmentType);
        entityBuilder.Property(e => e.AlignmentVersionId);
        entityBuilder.Property(e => e.Score)
            .HasColumnType("decimal(3)");
        entityBuilder.Property(e => e.SourceTokenId);
        entityBuilder.Property(e => e.TargetTokenId);

        //entity.HasOne(d => d.AlignmentType)
        //    .WithMany(p => p.Alignments)
        //    .HasForeignKey(d => d.AlignmentTypeId);

        entityBuilder.HasOne(d => d.AlignmentVersion)
            .WithMany(p => p.Alignments)
            .HasForeignKey(d => d.AlignmentVersionId);
    }
}