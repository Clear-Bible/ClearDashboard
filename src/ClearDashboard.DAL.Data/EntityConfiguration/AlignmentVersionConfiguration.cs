using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;


public class AlignmentVersionConfiguration : IEntityTypeConfiguration<AlignmentVersion>
{
    public void Configure(EntityTypeBuilder<AlignmentVersion> entityBuilder)
    {

        entityBuilder.HasKey(e => e.Id);

        // TODO:  delete this?
        //entityBuilder.Property(e => e.Created)
        //    .HasColumnType("datetime");

        entityBuilder.Property(e => e.IsDirty)
            .HasColumnType("bit");
        entityBuilder.Property(e => e.UserId);

        entityBuilder.HasOne(d => d.User)
            .WithMany(p => p.AlignmentVersions)
            .HasForeignKey(d => d.UserId);
    }
}