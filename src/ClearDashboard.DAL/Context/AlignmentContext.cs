using System;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Context
{
    public partial class AlignmentContext : DbContext
    {
        private readonly ILogger<AlignmentContext> _logger;
        public AlignmentContext(ILogger<AlignmentContext> logger)
        {
            _logger = logger;
        }

        public AlignmentContext(DbContextOptions<AlignmentContext> options)
            : base(options)
        {
        }

        protected AlignmentContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public virtual DbSet<Adornment> Adornments { get; set; }
        public virtual DbSet<Alignment> Alignments { get; set; }
        //public virtual DbSet<AlignmentType> AlignmentTypes { get; set; }
        public virtual DbSet<AlignmentVersion> AlignmentVersions { get; set; }
        public virtual DbSet<Corpus> Corpa { get; set; }
        //public virtual DbSet<CorpusType> CorpusTypes { get; set; }
        public virtual DbSet<InterlinearNote> InterlinearNotes { get; set; }
        public virtual DbSet<ParallelCorpus> ParallelCorpus { get; set; }
        public virtual DbSet<ParallelVerse> ParallelVerses { get; set; }
        public virtual DbSet<ProjectInfo> ProjectInfos { get; set; }
        public virtual DbSet<QuestionGroup> QuestionGroups { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Verse> Verses { get; set; }


        public string DatabasePath { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Filename={DatabasePath}");
            }
        }

        public async Task Migrate()
        {
            try
            {
                // Ensure that the database is created.  Note that if we want to be able to apply migrations later,
                // we want to call Database.Migrate(), not Database.EnsureCreated().
                // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
                _logger.LogInformation("Ensure that the database is created, migrating if necessary.");
                await Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not apply database migrations");

                // This is useful when applying migrations via the EF core plumbing, please leave in place.
                Console.WriteLine($"Could not apply database migrations: {ex.Message}");
            }
        }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Adornment>(entity =>
            {
                entity.ToTable("Adornment");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.TokenId).IsUnique();

                entity.Property(e => e.TokenId);

                entity.Property(e => e.Lemma)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.PartsOfSpeech)
                    .IsRequired()
                    .HasColumnType("varchar(15)")
                    .HasMaxLength(15);
                   

                entity.Property(e => e.Strong)
                    .HasColumnType("varchar(15)");
               
                   
                entity.HasOne(d => d.Token)
                    .WithOne(p => p.Adornment)
                    .HasPrincipalKey<Token>(p => p.Id)
                    .HasForeignKey<Adornment>(d => d.TokenId);
            });

            modelBuilder.Entity<Alignment>(entity =>
            {
                entity.ToTable("Alignment");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.SourceTokenId).IsUnique();
                entity.HasIndex(e => e.TargetTokenId).IsUnique();

                entity.Property(e => e.AlignmentTypeId);
                entity.Property(e => e.AlignmentVersionId);
                entity.Property(e => e.Score)
                    .HasColumnType("decimal(3)");
                entity.Property(e => e.SourceTokenId);
                entity.Property(e => e.TargetTokenId);

                //entity.HasOne(d => d.AlignmentType)
                //    .WithMany(p => p.Alignments)
                //    .HasForeignKey(d => d.AlignmentTypeId);

                entity.HasOne(d => d.AlignmentVersion)
                    .WithMany(p => p.Alignments)
                    .HasForeignKey(d => d.AlignmentVersionId);
            });

            //modelBuilder.Entity<AlignmentType>(entity =>
            //{
            //    entity.ToTable("AlignmentType");

            //    entity.HasKey(e => e.Id);
               
            //    entity.Property(e => e.Description)
            //        .HasMaxLength(100)
            //        .HasColumnType("varchar(100)");
            //});

            modelBuilder.Entity<AlignmentVersion>(entity =>
            {
                entity.ToTable("AlignmentVersion");

                entity.HasKey(e => e.Id);
               
                entity.Property(e => e.Created)
                    .HasColumnType("datetime");
                entity.Property(e => e.IsDirty)
                    .HasColumnType("bit");
                entity.Property(e => e.UserId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AlignmentVersions)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<Corpus>(entity =>
            {
                entity.ToTable("Corpus");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CorpusTypeId);
                   entity.Property(e => e.IsRtl)
                    .HasColumnType("bit")
                    .HasColumnName("IsRTL");

                entity.Property(e => e.Language); 

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(100)");


                // CODEREVIEW:  Is this really GUID? Should we adjust the length of the varchar?  Should we just make it a GUID?
                entity.Property(e => e.ParatextGuid)
                    .HasColumnType("varchar(250)")
                    .HasColumnName("ParatextGUID");

                //entity.HasOne(d => d.ParallelCorpus)
                //    .WithOne(p => p.CorpusCorpus)
                //    .HasPrincipalKey<ParallelCorpus>(p => p.SourceCorpusId)
                //    .HasForeignKey<Corpus>(d => d.Id)
                //    .OnDelete(DeleteBehavior.ClientSetNull);

                //entity.HasOne(d => d.CorpusNavigation)
                //    .WithOne(p => p.CorpusCorpusNavigation)
                //    .HasPrincipalKey<ParallelCorpus>(p => p.TargetCorpusId)
                //    .HasForeignKey<Corpus>(d => d.Id)
                //    .OnDelete(DeleteBehavior.ClientSetNull);

                //entity.HasOne(d => d.CorpusType)
                //    .WithMany(p => p.Corpa)
                //    .HasForeignKey(d => d.CorpusTypeId);
            });

            //modelBuilder.Entity<CorpusType>(entity =>
            //{
            //    entity.ToTable("CorpusType");

            //    entity.HasKey(e => e.Id);

            //    // CODE-REVIEW: Should this be a string?
            //    entity.Property(e => e.Description);
            //});

            modelBuilder.Entity<InterlinearNote>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.UserId).IsUnique();

                entity.Property(e => e.Created)
                    .HasColumnType("datetime");

                entity.Property(e => e.Note).HasColumnType("varchar(600)");

                entity.Property(e => e.TokenId);
           
                entity.Property(e => e.UserId);
                  

                entity.HasOne(d => d.Token)
                    .WithMany(p => p.InterlinearNotes)
                    .HasPrincipalKey(p => p.Id)
                    .HasForeignKey(d => d.TokenId);
            });

            modelBuilder.Entity<ParallelCorpus>(entity =>
            {
                entity.ToTable("ParallelCorpus");

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SourceCorpusId).IsUnique();
                entity.HasIndex(e => e.TargetCorpusId).IsUnique();
                entity.Property(e => e.AlignmentTypeId);
                entity.Property(e => e.Created)
                    .HasColumnType("datetime");
                entity.Property(e => e.LastGenerated)
                    .HasColumnType("datetime");
                entity.Property(e => e.SourceCorpusId);
                entity.Property(e => e.TargetCorpusId);
            });

            modelBuilder.Entity<ParallelVerse>(entity =>
            {
                entity.ToTable("ParallelVerse");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.SourceVerseId).IsUnique();
                entity.HasIndex(e => e.TargetVerseId).IsUnique();

                entity.Property(e => e.ParallelCorpusId);
                entity.Property(e => e.SourceVerseId);
                entity.Property(e => e.TargetVerseId);

                entity.HasOne(d => d.ParallelCorpus)
                    .WithMany(p => p.ParallelVerses)
                    .HasForeignKey(d => d.ParallelCorpusId);
            });

            modelBuilder.Entity<ProjectInfo>(entity =>
            {
                entity.ToTable("ProjectInfo");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Created)
                    .HasColumnType("datetime");

                entity.Property(e => e.IsRtl)
                    .HasColumnType("bit")
                    .HasColumnName("IsRTL");

                entity.Property(e => e.LastContentWordLevel);

                entity.Property(e => e.ProjectName);
            });

            modelBuilder.Entity<QuestionGroup>(entity =>
            {
                // CODE-REVIEW:  Should a primary key be added?
                entity.HasNoKey();
                entity.ToTable("QuestionGroup");
            });

            modelBuilder.Entity<Token>(entity =>
            {

                entity.ToTable("Token");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VerseId).IsUnique();

                entity.Property(e => e.WordNumber);
                entity.Property(e => e.SubwordNumber);
                entity.Property(e => e.VerseId);


                // CODE-REVIEW:  Should this be varchar(1)?
                entity.Property(e => e.FirstLetter)
                    .HasColumnType("varchar(2)");
                entity.Property(e => e.Text)
                    .HasColumnType("varchar(250)");

                //entity.HasOne(d => d.Alignment)
                //    .WithOne(p => p.SourceToken)
                //    .HasPrincipalKey<Alignment>(p => p.SourceTokenId)
                //    .HasForeignKey<Token>(d => d.Id)
                //    .OnDelete(DeleteBehavior.ClientSetNull);

                //entity.HasOne(d => d.Alignment)
                //    .WithOne(p => p.TargetToken)
                //    .HasPrincipalKey<Alignment>(p => p.TargetTokenId)
                //    .HasForeignKey<Token>(d => d.Id)
                //    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.LastAlignmentLevelId);
                entity.Property(e => e.ParatextUsername)
                    .HasColumnType("varchar(100)");

                entity.HasOne(d => d.UserNavigation)
                    .WithOne(p => p.User)
                    .HasPrincipalKey<InterlinearNote>(p => p.UserId)
                    .HasForeignKey<User>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Verse>(entity =>
            {
                entity.ToTable("Verse");

                entity.HasKey(e => e.Id);

                //CODE-REVIEW: is this correct length?
                entity.Property(e => e.SilBookNumber)
                    .HasColumnType("varchar(2)");

             

                entity.Property(e => e.CorpusId);

                entity.Property(e => e.LastChanged)
                    .HasColumnType("datetime");

                //CODE-REVIEW:  Is there any size limit?
                entity.Property(e => e.VerseText)
                    .HasColumnType("text");

                entity.HasOne(d => d.Corpus)
                    .WithMany(p => p.Verses)
                    .HasForeignKey(d => d.CorpusId);

                //entity.HasOne(d => d.Corpus)
                //    .WithOne(p => p.)
                //    .HasPrincipalKey<ParallelVerse>(p => p.SourceVerseId)
                //    .HasForeignKey<Verse>(d => d.Id)
                //    .OnDelete(DeleteBehavior.ClientSetNull);

                //entity.HasOne(d => d.Verse1)
                //    .WithOne(p => p.VerseVerse1)
                //    .HasPrincipalKey<ParallelVerse>(p => p.TargetVerseId)
                //    .HasForeignKey<Verse>(d => d.Id)
                //    .OnDelete(DeleteBehavior.ClientSetNull);

                //entity.HasOne(d => d.Verse2)
                //    .WithOne(p => p.Verse)
                //    .HasPrincipalKey<Token>(p => p.VerseId)
                //    .HasForeignKey<Verse>(d => d.Id)
                //    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
