using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class RawContentConfiguration : IEntityTypeConfiguration<RawContent>
{
    public void Configure(EntityTypeBuilder<RawContent> entityBuilder)
    {
        // CODE-REVIEW:  Should a primary key be added?
        
        // entityBuilder.ToTable("QuestionGroup");
        entityBuilder.HasDiscriminator(entity => entity.ContentType);

    }
}