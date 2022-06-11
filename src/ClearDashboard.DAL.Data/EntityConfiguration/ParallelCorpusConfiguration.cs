using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class ParallelCorpusConfiguration : IEntityTypeConfiguration<ParallelCorpus>
{
    public void Configure(EntityTypeBuilder<ParallelCorpus> entityBuilder)
    {
        entityBuilder.ToTable("ParallelCorpus");

        entityBuilder.HasKey(e => e.Id);
        entityBuilder.HasIndex(e => e.SourceCorpusId).IsUnique();
        entityBuilder.HasIndex(e => e.TargetCorpusId).IsUnique();
        entityBuilder.Property(e => e.AlignmentType);
        entityBuilder.Property(e => e.Created)
            .HasColumnType("datetime");
        entityBuilder.Property(e => e.LastGenerated)
            .HasColumnType("datetime");
        entityBuilder.Property(e => e.SourceCorpusId);
        entityBuilder.Property(e => e.TargetCorpusId);
    }
}