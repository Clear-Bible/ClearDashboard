using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entityBuilder)
    {
        entityBuilder.HasKey(e => e.Id);

        entityBuilder.Property(e => e.LastAlignmentLevelId);
        entityBuilder.Property(e => e.ParatextUsername)
            .HasColumnType("varchar(100)");

        entityBuilder.HasOne(d => d.UserNavigation)
            .WithOne(p => p.User)
            .HasPrincipalKey<InterlinearNote>(p => p.UserId)
            .HasForeignKey<User>(d => d.Id)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}