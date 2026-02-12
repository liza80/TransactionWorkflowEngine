using Microsoft.EntityFrameworkCore;
using TransactionWorkflowEngine.Data;
using TransactionWorkflowEngine.DTOs;
using TransactionWorkflowEngine.Exceptions;
using TransactionWorkflowEngine.Models;

namespace TransactionWorkflowEngine.Services;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly IWorkflowCacheService _workflowCache;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ApplicationDbContext context, 
        IWorkflowCacheService workflowCache,
        ILogger<TransactionService> logger)
    {
        _context = context;
        _workflowCache = workflowCache;
        _logger = logger;
    }

    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
    {
        // Get the initial status from cache
        var initialStatus = await _workflowCache.GetInitialStatusAsync()
            ?? throw new InvalidOperationException("No initial status configured in the system");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = GenerateReferenceNumber(),
            StatusId = initialStatus.Id,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            CustomerId = request.CustomerId,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created transaction {TransactionId} with reference {ReferenceNumber}", 
            transaction.Id, transaction.ReferenceNumber);

        return new TransactionResponse(
            transaction.Id,
            transaction.ReferenceNumber,
            MapToStatusInfo(initialStatus),
            transaction.Amount,
            transaction.Currency,
            transaction.CustomerId,
            transaction.Description,
            transaction.CreatedAt,
            transaction.UpdatedAt
        );
    }

    public async Task<TransactionDetailResponse?> GetTransactionAsync(Guid id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Status)
            .Include(t => t.History)
                .ThenInclude(h => h.FromStatus)
            .Include(t => t.History)
                .ThenInclude(h => h.ToStatus)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return null;

        return MapToDetailResponse(transaction);
    }

    public async Task<TransactionResponse> TransitionAsync(Guid id, TransitionRequest request)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new TransactionNotFoundException(id);

        // Check if current status is final
        if (transaction.Status.IsFinal)
        {
            throw new InvalidTransitionException(
                $"Cannot transition from final status '{transaction.Status.Name}'");
        }

        // Check if the transition is allowed using cached transitions
        var allowedTransition = await _workflowCache.GetTransitionAsync(transaction.StatusId, request.ToStatusId);

        if (allowedTransition == null)
        {
            throw new InvalidTransitionException(
                $"Transition from '{transaction.Status.Name}' to status ID {request.ToStatusId} is not allowed");
        }

        // Check if comment/reason is required
        if (allowedTransition.RequiresComment && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidTransitionException(
                $"Transition '{allowedTransition.Name}' requires a reason");
        }

        // Create history record
        var historyRecord = new TransactionHistory
        {
            TransactionId = transaction.Id,
            FromStatusId = transaction.StatusId,
            ToStatusId = request.ToStatusId,
            Comment = request.Reason,
            ChangedAt = DateTime.UtcNow
        };

        _context.TransactionHistories.Add(historyRecord);

        // Update transaction status
        var oldStatusName = transaction.Status.Name;
        transaction.StatusId = request.ToStatusId;
        transaction.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(
                "The transaction was modified by another process. Please refresh and try again.");
        }

        _logger.LogInformation(
            "Transaction {TransactionId} transitioned from '{OldStatus}' to '{NewStatus}' via '{TransitionName}'",
            transaction.Id, oldStatusName, allowedTransition.ToStatus.Name, allowedTransition.Name);

        // Get the new status from cache for response
        var statuses = await _workflowCache.GetAllStatusesAsync();
        var newStatus = statuses.First(s => s.Id == request.ToStatusId);

        return new TransactionResponse(
            transaction.Id,
            transaction.ReferenceNumber,
            MapToStatusInfo(newStatus),
            transaction.Amount,
            transaction.Currency,
            transaction.CustomerId,
            transaction.Description,
            transaction.CreatedAt,
            transaction.UpdatedAt
        );
    }

    public async Task<IEnumerable<AvailableTransitionResponse>> GetAvailableTransitionsAsync(Guid id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new TransactionNotFoundException(id);

        // If current status is final, no transitions available
        if (transaction.Status.IsFinal)
        {
            return Enumerable.Empty<AvailableTransitionResponse>();
        }

        // Get available transitions from cache
        var availableTransitions = await _workflowCache.GetAllowedTransitionsAsync(transaction.StatusId);

        return availableTransitions.Select(t => new AvailableTransitionResponse(
            t.Id,
            t.ToStatusId,
            t.ToStatus.Name,
            t.Name,
            t.Description,
            t.RequiresComment,
            t.IsRollback
        ));
    }

    public async Task<IEnumerable<TransactionHistoryResponse>> GetTransactionHistoryAsync(Guid id)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new TransactionNotFoundException(id);

        var history = await _context.TransactionHistories
            .Include(h => h.FromStatus)
            .Include(h => h.ToStatus)
            .Where(h => h.TransactionId == id)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        return history.Select(h => new TransactionHistoryResponse(
            h.Id,
            MapToStatusInfo(h.FromStatus),
            MapToStatusInfo(h.ToStatus),
            h.Comment,
            h.ChangedAt
        ));
    }

    private static string GenerateReferenceNumber()
    {
        // Generate a unique reference number: TXN-YYYYMMDD-XXXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"TXN-{datePart}-{randomPart}";
    }

    private static StatusInfo MapToStatusInfo(TransactionStatus status)
    {
        return new StatusInfo(
            status.Id,
            status.Name,
            status.Description,
            status.IsInitial,
            status.IsFinal
        );
    }

    private static TransactionDetailResponse MapToDetailResponse(Transaction transaction)
    {
        var historyResponses = transaction.History
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new TransactionHistoryResponse(
                h.Id,
                MapToStatusInfo(h.FromStatus),
                MapToStatusInfo(h.ToStatus),
                h.Comment,
                h.ChangedAt
            ));

        return new TransactionDetailResponse(
            transaction.Id,
            transaction.ReferenceNumber,
            MapToStatusInfo(transaction.Status),
            transaction.Amount,
            transaction.Currency,
            transaction.CustomerId,
            transaction.Description,
            transaction.CreatedAt,
            transaction.UpdatedAt,
            historyResponses
        );
    }
}
