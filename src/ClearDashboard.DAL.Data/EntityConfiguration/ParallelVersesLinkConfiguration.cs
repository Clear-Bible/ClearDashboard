using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class ParallelVersesLinkConfiguration : IEntityTypeConfiguration<ParallelVersesLink>
{
    public void Configure(EntityTypeBuilder<ParallelVersesLink> entityBuilder)
    {

        //entityBuilder.HasKey(e => e.Id);

        //entityBuilder.HasOne(d => d.ParallelCorpus)
        //    .WithMany(p => p.ParallelVersesLinks)
        //    .HasForeignKey(d => d.ParallelCorpusId);
    }
}