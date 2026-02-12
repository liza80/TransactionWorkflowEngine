using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TransactionWorkflowEngine.Models;

namespace TransactionWorkflowEngine.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionStatus> TransactionStatuses => Set<TransactionStatus>();
    public DbSet<TransactionStatusTransition> TransactionStatusTransitions => Set<TransactionStatusTransition>();
    public DbSet<TransactionHistory> TransactionHistories => Set<TransactionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Optimistic concurrency with RowVersion
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            
            entity.HasOne(e => e.Status)
                .WithMany(s => s.Transactions)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.ReferenceNumber).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.StatusId);
        });

        // TransactionStatus configuration
        modelBuilder.Entity<TransactionStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TransactionStatusTransition configuration
        modelBuilder.Entity<TransactionStatusTransition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            
            entity.HasOne(e => e.FromStatus)
                .WithMany(s => s.TransitionsFrom)
                .HasForeignKey(e => e.FromStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ToStatus)
                .WithMany(s => s.TransitionsTo)
                .HasForeignKey(e => e.ToStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Ensure unique transition per from-to pair
            entity.HasIndex(e => new { e.FromStatusId, e.ToStatusId }).IsUnique();
        });

        // TransactionHistory configuration
        modelBuilder.Entity<TransactionHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.History)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.FromStatus)
                .WithMany()
                .HasForeignKey(e => e.FromStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ToStatus)
                .WithMany()
                .HasForeignKey(e => e.ToStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.ChangedAt);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed transaction statuses as per assignment:
        // CREATED → VALIDATED → PROCESSING → COMPLETED
        //                              ↘ FAILED → VALIDATED
        modelBuilder.Entity<TransactionStatus>().HasData(
            new TransactionStatus { Id = 1, Name = "Created", Description = "Transaction has been created", IsInitial = true, IsFinal = false, DisplayOrder = 1 },
            new TransactionStatus { Id = 2, Name = "Validated", Description = "Transaction has been validated", IsInitial = false, IsFinal = false, DisplayOrder = 2 },
            new TransactionStatus { Id = 3, Name = "Processing", Description = "Transaction is being processed", IsInitial = false, IsFinal = false, DisplayOrder = 3 },
            new TransactionStatus { Id = 4, Name = "Completed", Description = "Transaction has been completed", IsInitial = false, IsFinal = true, DisplayOrder = 4 },
            new TransactionStatus { Id = 5, Name = "Failed", Description = "Transaction processing failed", IsInitial = false, IsFinal = false, DisplayOrder = 5 }
        );

        // Seed allowed transitions as per assignment workflow:
        // CREATED → VALIDATED → PROCESSING → COMPLETED
        //                              ↘ FAILED → VALIDATED
        modelBuilder.Entity<TransactionStatusTransition>().HasData(
            // CREATED → VALIDATED
            new TransactionStatusTransition 
            { 
                Id = 1, 
                FromStatusId = 1, 
                ToStatusId = 2, 
                Name = "Validate", 
                Description = "Validate the created transaction" 
            },
            
            // VALIDATED → PROCESSING
            new TransactionStatusTransition 
            { 
                Id = 2, 
                FromStatusId = 2, 
                ToStatusId = 3, 
                Name = "Start Processing", 
                Description = "Begin processing the validated transaction" 
            },
            
            // PROCESSING → COMPLETED
            new TransactionStatusTransition 
            { 
                Id = 3, 
                FromStatusId = 3, 
                ToStatusId = 4, 
                Name = "Complete", 
                Description = "Mark transaction as completed" 
            },
            
            // PROCESSING → FAILED
            new TransactionStatusTransition 
            { 
                Id = 4, 
                FromStatusId = 3, 
                ToStatusId = 5, 
                Name = "Fail", 
                Description = "Mark transaction as failed",
                RequiresComment = true
            },
            
            // FAILED → VALIDATED (retry/rollback)
            new TransactionStatusTransition 
            { 
                Id = 5, 
                FromStatusId = 5, 
                ToStatusId = 2, 
                Name = "Retry", 
                Description = "Retry the failed transaction",
                IsRollback = true
            }
        );
    }
}
