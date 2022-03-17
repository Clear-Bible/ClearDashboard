using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ClearDashboard.DataAccessLayer.Context
{
    public partial class AlignmentContext : DbContext
    {
        public AlignmentContext()
        {
        }

        public AlignmentContext(DbContextOptions<AlignmentContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Adornment> Adornments { get; set; }
        public virtual DbSet<Alignment> Alignments { get; set; }
        public virtual DbSet<AlignmentType> AlignmentTypes { get; set; }
        public virtual DbSet<AlignmentVersion> AlignmentVersions { get; set; }
        public virtual DbSet<Corpus> Corpa { get; set; }
        public virtual DbSet<CorpusType> CorpusTypes { get; set; }
        public virtual DbSet<InterlinearNote> InterlinearNotes { get; set; }
        public virtual DbSet<ParallelCorpus> ParallelCorpus { get; set; }
        public virtual DbSet<ParallelVerse> ParallelVerses { get; set; }
        public virtual DbSet<ProjectInfo> ProjectInfos { get; set; }
        public virtual DbSet<QuestionGroup> QuestionGroups { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Verse> Verses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("data source=alignment.sqlite");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Adornment>(entity =>
            {
                entity.ToTable("Adornment");

                entity.HasIndex(e => e.Id, "pk_Adornments")
                    .IsUnique();

                entity.HasIndex(e => e.TokenId, "unq_Adornments_TokenId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.Lemma).HasColumnType("varchar(50)");

                entity.Property(e => e.Pos)
                    .IsRequired()
                    .HasColumnType("varchar(15)")
                    .HasColumnName("POS");

                entity.Property(e => e.Strong).HasColumnType("varchar(15)");

                entity.Property(e => e.TokenId)
                    .HasColumnType("bigint")
                    .HasColumnName("TokenId");

                entity.HasOne(d => d.Token)
                    .WithOne(p => p.Adornment)
                    .HasPrincipalKey<Token>(p => p.Id)
                    .HasForeignKey<Adornment>(d => d.TokenId);
            });

            modelBuilder.Entity<Alignment>(entity =>
            {
                entity.ToTable("Alignment");

                entity.HasIndex(e => e.SourceTokenId, "IX_Alignment_SourceTokenId")
                    .IsUnique();

                entity.HasIndex(e => e.TargetTokenId, "IX_Alignment_TargetTokenId")
                    .IsUnique();

                entity.HasIndex(e => e.SourceTokenId, "Unq_Alignment_SourceTokenId")
                    .IsUnique();

                entity.HasIndex(e => e.TargetTokenId, "Unq_Alignment_TargetTokenId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.AlignmentTypeId)
                    .HasColumnType("integer")
                    .HasColumnName("AlignmentTypeId");

                entity.Property(e => e.AlignmentVersionId)
                    .HasColumnType("bigint")
                    .HasColumnName("AlignmentVersionId");

                entity.Property(e => e.Score).HasColumnType("decimal(3)");

                entity.Property(e => e.SourceTokenId)
                    .HasColumnType("integer")
                    .HasColumnName("SourceTokenId");

                entity.Property(e => e.TargetTokenId)
                    .HasColumnType("integer")
                    .HasColumnName("TargetTokenId");

                entity.HasOne(d => d.AlignmentType)
                    .WithMany(p => p.Alignments)
                    .HasForeignKey(d => d.AlignmentTypeId);

                entity.HasOne(d => d.AlignmentVersion)
                    .WithMany(p => p.Alignments)
                    .HasForeignKey(d => d.AlignmentVersionId);
            });

            modelBuilder.Entity<AlignmentType>(entity =>
            {
                entity.ToTable("AlignmentType");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.Description).HasColumnType("varchar(100)");
            });

            modelBuilder.Entity<AlignmentVersion>(entity =>
            {
                entity.ToTable("AlignmentVersion");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.IsDirty).HasColumnType("bit");

                entity.Property(e => e.UserId).HasColumnType("integer");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AlignmentVersions)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<Corpus>(entity =>
            {
                entity.ToTable("Corpus");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever();

                entity.Property(e => e.CorpusTypeId)
                    .HasColumnType("integer")
                    .HasColumnName("CorpusTypeId");

                entity.Property(e => e.IsRtl)
                    .HasColumnType("bit")
                    .HasColumnName("IsRTL");

                entity.Property(e => e.Language).HasColumnType("integer");

                entity.Property(e => e.Name).HasColumnType("varchar(100)");

                entity.Property(e => e.ParatextGuid)
                    .HasColumnType("varchar(250)")
                    .HasColumnName("ParatextGUID");

                entity.HasOne(d => d.ParallelCorpus)
                    .WithOne(p => p.CorpusCorpus)
                    .HasPrincipalKey<ParallelCorpus>(p => p.SourceCorpusId)
                    .HasForeignKey<Corpus>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.CorpusNavigation)
                    .WithOne(p => p.CorpusCorpusNavigation)
                    .HasPrincipalKey<ParallelCorpus>(p => p.TargetCorpusId)
                    .HasForeignKey<Corpus>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.CorpusType)
                    .WithMany(p => p.Corpa)
                    .HasForeignKey(d => d.CorpusTypeId);
            });

            modelBuilder.Entity<CorpusType>(entity =>
            {
                entity.ToTable("CorpusType");

                entity.HasIndex(e => e.Id, "Pk_CorpusType_CorpusTypeId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever();

                entity.Property(e => e.Description).HasColumnType("integer");
            });

            modelBuilder.Entity<InterlinearNote>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.UserId, "unq_InterlinearNotes_UserId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.Note).HasColumnType("varchar(600)");

                entity.Property(e => e.TokenId)
                    .HasColumnType("bigint")
                    .HasColumnName("TokenId");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint")
                    .HasColumnName("UserId");

                entity.HasOne(d => d.Token)
                    .WithMany(p => p.InterlinearNotes)
                    .HasPrincipalKey(p => p.Id)
                    .HasForeignKey(d => d.TokenId);
            });

            modelBuilder.Entity<ParallelCorpus>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.SourceCorpusId, "IX_ParallelCorpus_SourceCorpusId")
                    .IsUnique();

                entity.HasIndex(e => e.TargetCorpusId, "IX_ParallelCorpus_TargetCorpusId")
                    .IsUnique();

                entity.HasIndex(e => e.Id, "Pk_ParallelCorpus_ParallelCorpusId")
                    .IsUnique();

                entity.HasIndex(e => e.SourceCorpusId, "Unq_ParallelCorpus_SourceCorpusId")
                    .IsUnique();

                entity.HasIndex(e => e.TargetCorpusId, "Unq_ParallelCorpus_TargetCorpusId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.AlignmentType).HasColumnType("integer");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.LastGenerated).HasColumnType("datetime");

                entity.Property(e => e.SourceCorpusId)
                    .HasColumnType("integer")
                    .HasColumnName("SourceCorpusId");

                entity.Property(e => e.TargetCorpusId)
                    .HasColumnType("integer")
                    .HasColumnName("TargetCorpusId");
            });

            modelBuilder.Entity<ParallelVerse>(entity =>
            {
                entity.ToTable("ParallelVerse");

                entity.HasIndex(e => e.SourceVerseId, "IX_ParallelVerses_SourceVerseId")
                    .IsUnique();

                entity.HasIndex(e => e.TargetVerseId, "IX_ParallelVerses_TargetVerseId")
                    .IsUnique();

                entity.HasIndex(e => e.SourceVerseId, "Unq_ParallelVerses_SourceVerseId")
                    .IsUnique();

                entity.HasIndex(e => e.TargetVerseId, "Unq_ParallelVerses_TargetVerseId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.ParallelCorpusId)
                    .HasColumnType("integer")
                    .HasColumnName("ParallelCorpusId");

                entity.Property(e => e.SourceVerseId)
                    .HasColumnType("bigint")
                    .HasColumnName("SourceVerseId");

                entity.Property(e => e.TargetVerseId)
                    .HasColumnType("bigint")
                    .HasColumnName("TargetVerseId");

                entity.HasOne(d => d.ParallelCorpus)
                    .WithMany(p => p.ParallelVerses)
                    .HasForeignKey(d => d.ParallelCorpusId);
            });

            modelBuilder.Entity<ProjectInfo>(entity =>
            {
                entity.ToTable("ProjectInfo");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.IsRtl)
                    .HasColumnType("bit")
                    .HasColumnName("IsRTL");

                entity.Property(e => e.LastContentWordLevel).HasColumnType("integer");

                entity.Property(e => e.ProjectName).HasColumnType("text");
            });

            modelBuilder.Entity<QuestionGroup>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("QuestionGroup");
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.WordId, e.PartId, e.VerseId });

                entity.ToTable("Token");

                entity.HasIndex(e => e.VerseId, "IX_Token_VerseId")
                    .IsUnique();

                entity.HasIndex(e => e.Id, "Unq_Token_TokenId")
                    .IsUnique();

                entity.HasIndex(e => e.VerseId, "Unq_Token_VerseId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint");

                entity.Property(e => e.WordId)
                    .HasColumnType("integer")
                    .HasColumnName("WordId");

                entity.Property(e => e.PartId)
                    .HasColumnType("integer")
                    .HasColumnName("PartId");

                entity.Property(e => e.VerseId)
                    .HasColumnType("bigint")
                    .HasColumnName("VerseId");

                entity.Property(e => e.FirstLetter).HasColumnType("varchar(2)");

                entity.Property(e => e.Text).HasColumnType("varchar(250)");

                entity.HasOne(d => d.TokenNavigation)
                    .WithOne(p => p.TokenTokenNavigation)
                    .HasPrincipalKey<Alignment>(p => p.SourceTokenId)
                    .HasForeignKey<Token>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Token1)
                    .WithOne(p => p.TokenToken1)
                    .HasPrincipalKey<Alignment>(p => p.TargetTokenId)
                    .HasForeignKey<Token>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.LastAlignmentLevelId)
                    .HasColumnType("bigint")
                    .HasColumnName("LastAlignmentLevelId");

                entity.Property(e => e.ParatextUsername).HasColumnType("varchar(100)");

                entity.HasOne(d => d.UserNavigation)
                    .WithOne(p => p.User)
                    .HasPrincipalKey<InterlinearNote>(p => p.UserId)
                    .HasForeignKey<User>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Verse>(entity =>
            {
                entity.ToTable("Verse");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever();

                entity.Property(e => e.BookId).HasColumnType("varchar(2)");

                entity.Property(e => e.CorpusId).HasColumnType("integer");

                entity.Property(e => e.LastChanged).HasColumnType("datetime");

                entity.Property(e => e.VerseText).HasColumnType("text");

                entity.HasOne(d => d.Corpus)
                    .WithMany(p => p.Verses)
                    .HasForeignKey(d => d.CorpusId);

                entity.HasOne(d => d.VerseNavigation)
                    .WithOne(p => p.VerseVerseNavigation)
                    .HasPrincipalKey<ParallelVerse>(p => p.SourceVerseId)
                    .HasForeignKey<Verse>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Verse1)
                    .WithOne(p => p.VerseVerse1)
                    .HasPrincipalKey<ParallelVerse>(p => p.TargetVerseId)
                    .HasForeignKey<Verse>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Verse2)
                    .WithOne(p => p.Verse)
                    .HasPrincipalKey<Token>(p => p.VerseId)
                    .HasForeignKey<Verse>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
