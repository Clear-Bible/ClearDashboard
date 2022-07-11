using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class RawContentConfiguration : IEntityTypeConfiguration<RawContent>
{
    public void Configure(EntityTypeBuilder<RawContent> entityBuilder)
    {
        entityBuilder.HasDiscriminator<string>( "ContentType");
    }
}