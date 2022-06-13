using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class DataAssociationConfiguration : IEntityTypeConfiguration<NoteAssociation>
{
    public void Configure(EntityTypeBuilder<NoteAssociation> entityBuilder)
    {
        entityBuilder.HasKey(entity => entity.Id);
    }
}