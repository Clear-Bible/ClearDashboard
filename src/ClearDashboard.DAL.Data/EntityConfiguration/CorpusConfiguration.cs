using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class CorpusConfiguration : IEntityTypeConfiguration<Corpus>
{
    public void Configure(EntityTypeBuilder<Corpus> entityBuilder)
    {
        //entityBuilder.ToTable("Corpus");

        //entityBuilder.HasKey(e => e.Id);

        //entityBuilder.Property(e => e.CorpusType);
        //entityBuilder.Property(e => e.IsRtl)
        //    .HasColumnType("bit")
        //    .HasColumnName("IsRTL");

        //entityBuilder.Property(e => e.Language);

        //entityBuilder.Property(e => e.Name)
        //    .HasColumnType("varchar(100)");


        //// CODEREVIEW:  Is this really GUID? Should we adjust the length of the varchar?  Should we just make it a GUID?
        //entityBuilder.Property(e => e.ParatextGuid)
        //    .HasColumnType("varchar(250)")
        //    .HasColumnName("ParatextGUID");

        ////entityBuilder.HasOne(d => d.ParallelCorpus)
        ////    .WithOne(p => p.CorpusCorpus)
        ////    .HasPrincipalKey<ParallelCorpus>(p => p.SourceCorpusId)
        ////    .HasForeignKey<Corpus>(d => d.Id)
        ////    .OnDelete(DeleteBehavior.ClientSetNull);

        ////entityBuilder.HasOne(d => d.CorpusNavigation)
        ////    .WithOne(p => p.CorpusCorpusNavigation)
        ////    .HasPrincipalKey<ParallelCorpus>(p => p.TargetCorpusId)
        ////    .HasForeignKey<Corpus>(d => d.Id)
        ////    .OnDelete(DeleteBehavior.ClientSetNull);

        ////entityBuilder.HasOne(d => d.CorpusType)
        ////    .WithMany(p => p.Corpa)
        ////    .HasForeignKey(d => d.CorpusTypeId);
    }

}