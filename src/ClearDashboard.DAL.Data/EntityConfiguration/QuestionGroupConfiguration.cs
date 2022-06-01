using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class QuestionGroupConfiguration : IEntityTypeConfiguration<QuestionGroup>
{
    public void Configure(EntityTypeBuilder<QuestionGroup> entityBuilder)
    {
        // CODE-REVIEW:  Should a primary key be added?
        entityBuilder.HasNoKey();
       // entityBuilder.ToTable("QuestionGroup");
    }
}