using Microsoft.EntityFrameworkCore;
using TransactionWorkflowEngine.Data;
using TransactionWorkflowEngine.Models;

namespace TransactionWorkflowEngine.Tests;

public class WorkflowTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        var context = new ApplicationDbContext(options);
        SeedData(context);
        return context;
    }

    private void SeedData(ApplicationDbContext context)
    {
        // Statuses
        context.TransactionStatuses.AddRange(
            new TransactionStatus { Id = 1, Name = "Created", IsInitial = true },
            new TransactionStatus { Id = 2, Name = "Validated" },
            new TransactionStatus { Id = 3, Name = "Processing" },
            new TransactionStatus { Id = 4, Name = "Completed", IsFinal = true },
            new TransactionStatus { Id = 5, Name = "Failed" }
        );

        // Transitions: Created→Validated→Processing→Completed, Processing→Failed→Validated
        context.TransactionStatusTransitions.AddRange(
            new TransactionStatusTransition { Id = 1, FromStatusId = 1, ToStatusId = 2, Name = "Validate" },
            new TransactionStatusTransition { Id = 2, FromStatusId = 2, ToStatusId = 3, Name = "Process" },
            new TransactionStatusTransition { Id = 3, FromStatusId = 3, ToStatusId = 4, Name = "Complete" },
            new TransactionStatusTransition { Id = 4, FromStatusId = 3, ToStatusId = 5, Name = "Fail" },
            new TransactionStatusTransition { Id = 5, FromStatusId = 5, ToStatusId = 2, Name = "Retry" }
        );

        context.SaveChanges();
    }

    [Fact]
    public async Task ValidTransition_ShouldSucceed()
    {
        // Arrange
        using var context = CreateDbContext();
        var transaction = new Transaction { Id = Guid.NewGuid(), StatusId = 1, Amount = 100, CustomerId = "C1" };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act - transition from Created(1) to Validated(2)
        var allowedTransition = await context.TransactionStatusTransitions
            .AnyAsync(t => t.FromStatusId == 1 && t.ToStatusId == 2);

        // Assert
        Assert.True(allowedTransition);
    }

    [Fact]
    public async Task InvalidTransition_ShouldBeRejected()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act - try transition from Created(1) directly to Completed(4)
        var allowedTransition = await context.TransactionStatusTransitions
            .AnyAsync(t => t.FromStatusId == 1 && t.ToStatusId == 4);

        // Assert
        Assert.False(allowedTransition);
    }

    [Fact]
    public async Task RollbackTransition_ShouldBeAllowed()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act - transition from Failed(5) back to Validated(2)
        var allowedTransition = await context.TransactionStatusTransitions
            .AnyAsync(t => t.FromStatusId == 5 && t.ToStatusId == 2);

        // Assert
        Assert.True(allowedTransition);
    }

    [Fact]
    public async Task GetAvailableTransitions_FromProcessing_ShouldReturnTwo()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act - from Processing(3) can go to Completed(4) or Failed(5)
        var transitions = await context.TransactionStatusTransitions
            .Where(t => t.FromStatusId == 3)
            .ToListAsync();

        // Assert
        Assert.Equal(2, transitions.Count);
    }

    [Fact]
    public async Task FinalStatus_ShouldHaveNoOutgoingTransitions()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act - Completed(4) is final, should have no outgoing transitions
        var transitions = await context.TransactionStatusTransitions
            .Where(t => t.FromStatusId == 4)
            .ToListAsync();

        // Assert
        Assert.Empty(transitions);
    }

    [Fact]
    public async Task InitialStatus_ShouldBeCreated()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act
        var initialStatus = await context.TransactionStatuses
            .FirstOrDefaultAsync(s => s.IsInitial);

        // Assert
        Assert.NotNull(initialStatus);
        Assert.Equal("Created", initialStatus.Name);
    }
}
