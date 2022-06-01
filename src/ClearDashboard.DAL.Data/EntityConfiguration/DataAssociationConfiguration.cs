using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class DataAssociationConfiguration : IEntityTypeConfiguration<DataAssociation>
{
    public void Configure(EntityTypeBuilder<DataAssociation> entityBuilder)
    {
        entityBuilder.HasKey(entity => entity.Id);
    }
}