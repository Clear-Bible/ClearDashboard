using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration;

public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> entityBuilder)
    {
        entityBuilder.HasKey(e => e.Id);
        entityBuilder.HasIndex(e => e.VerseId).IsUnique();

        entityBuilder.Property(e => e.WordNumber);
        entityBuilder.Property(e => e.SubwordNumber);
        entityBuilder.Property(e => e.VerseId);


        // CODE-REVIEW:  Should this be varchar(1)?
        entityBuilder.Property(e => e.FirstLetter)
            .HasColumnType("varchar(2)");
        entityBuilder.Property(e => e.Text)
            .HasColumnType("varchar(250)");

        //entityBuilder.HasOne(d => d.Alignment)
        //    .WithOne(p => p.SourceToken)
        //    .HasPrincipalKey<Alignment>(p => p.SourceTokenId)
        //    .HasForeignKey<Token>(d => d.Id)
        //    .OnDelete(DeleteBehavior.ClientSetNull);

        //entityBuilder.HasOne(d => d.Alignment)
        //    .WithOne(p => p.TargetToken)
        //    .HasPrincipalKey<Alignment>(p => p.TargetTokenId)
        //    .HasForeignKey<Token>(d => d.Id)
        //    .OnDelete(DeleteBehavior.ClientSetNull);
    }
}