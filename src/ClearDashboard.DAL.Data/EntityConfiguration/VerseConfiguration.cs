using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class VerseConfiguration : IEntityTypeConfiguration<Verse>
{
    public void Configure(EntityTypeBuilder<Verse> entityBuilder)
    {
        entityBuilder.HasKey(e => e.Id);

        //CODE-REVIEW: is this correct length?
        entityBuilder.Property(e => e.SilBookNumber)
            .HasColumnType("varchar(2)");

        entityBuilder.Property(e => e.CorpusId);

        entityBuilder.Property(e => e.Modified)
            .HasColumnType("datetime");

        //CODE-REVIEW:  Is there any size limit?
        entityBuilder.Property(e => e.VerseText)
            .HasColumnType("text");

        entityBuilder.HasOne(d => d.Corpus)
            .WithMany(p => p.Verses)
            .HasForeignKey(d => d.CorpusId);

        //entityBuilder.HasOne(d => d.Corpus)
        //    .WithOne(p => p.)
        //    .HasPrincipalKey<ParallelVerse>(p => p.SourceVerseId)
        //    .HasForeignKey<Verse>(d => d.Id)
        //    .OnDelete(DeleteBehavior.ClientSetNull);

        //entityBuilder.HasOne(d => d.Verse1)
        //    .WithOne(p => p.VerseVerse1)
        //    .HasPrincipalKey<ParallelVerse>(p => p.TargetVerseId)
        //    .HasForeignKey<Verse>(d => d.Id)
        //    .OnDelete(DeleteBehavior.ClientSetNull);

        //entityBuilder.HasOne(d => d.Verse2)
        //    .WithOne(p => p.Verse)
        //    .HasPrincipalKey<Token>(p => p.VerseId)
        //    .HasForeignKey<Verse>(d => d.Id)
        //    .OnDelete(DeleteBehavior.ClientSetNull);
    }
}