using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class ProjectInfoConfiguration : IEntityTypeConfiguration<ProjectInfo>
{
    public void Configure(EntityTypeBuilder<ProjectInfo> entityBuilder)
    {
        entityBuilder.HasKey(e => e.Id);

        entityBuilder.Property(e => e.Created)
            .HasColumnType("datetime");

        entityBuilder.Property(e => e.IsRtl)
            .HasColumnType("bit")
            .HasColumnName("IsRTL");

        entityBuilder.Property(e => e.LastContentWordLevel);

        entityBuilder.Property(e => e.ProjectName);
    }
}