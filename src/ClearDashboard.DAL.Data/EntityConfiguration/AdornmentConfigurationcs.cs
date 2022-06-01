using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearDashboard.DataAccessLayer.Data.EntityConfiguration
{

    public class AdornmentConfiguration : IEntityTypeConfiguration<Adornment>
    {
        public void Configure(EntityTypeBuilder<Adornment> entityBuilder)
        {

            entityBuilder.HasKey(e => e.Id);

            entityBuilder.HasIndex(e => e.TokenId).IsUnique();

            entityBuilder.Property(e => e.TokenId);

            entityBuilder.Property(e => e.Lemma)
                .HasColumnType("varchar(50)");

            entityBuilder.Property(e => e.PartsOfSpeech)
                .IsRequired()
                .HasColumnType("varchar(15)")
                .HasMaxLength(15);


            entityBuilder.Property(e => e.Strong)
                .HasColumnType("varchar(15)");


            entityBuilder.HasOne(d => d.Token)
                .WithOne(p => p.Adornment)
                .HasPrincipalKey<Token>(p => p.Id)
                .HasForeignKey<Adornment>(d => d.TokenId);
        }
    }
}
