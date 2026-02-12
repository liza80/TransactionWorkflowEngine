using Microsoft.EntityFrameworkCore;
using TransactionWorkflowEngine.Data;
using TransactionWorkflowEngine.DTOs;
using TransactionWorkflowEngine.Exceptions;
using TransactionWorkflowEngine.Models;

namespace TransactionWorkflowEngine.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly IWorkflowCacheService _workflowCache;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        ApplicationDbContext context,
        IWorkflowCacheService workflowCache,
        ILogger<AdminService> logger)
    {
        _context = context;
        _workflowCache = workflowCache;
        _logger = logger;
    }

    public async Task<IEnumerable<StatusResponse>> GetAllStatusesAsync()
    {
        var statuses = await _context.TransactionStatuses
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        return statuses.Select(s => new StatusResponse(
            s.Id,
            s.Name,
            s.Description,
            s.IsInitial,
            s.IsFinal,
            s.DisplayOrder
        ));
    }

    public async Task<StatusResponse> AddStatusAsync(CreateStatusRequest request)
    {
        // Check if status with same name already exists
        var existingStatus = await _context.TransactionStatuses
            .FirstOrDefaultAsync(s => s.Name == request.Name);

        if (existingStatus != null)
        {
            throw new StatusAlreadyExistsException(request.Name);
        }

        // If this is an initial status, ensure no other initial status exists
        if (request.IsInitial)
        {
            var hasInitial = await _context.TransactionStatuses.AnyAsync(s => s.IsInitial);
            if (hasInitial)
            {
                throw new InvalidOperationException("An initial status already exists. Only one initial status is allowed.");
            }
        }

        var status = new TransactionStatus
        {
            Name = request.Name,
            Description = request.Description,
            IsInitial = request.IsInitial,
            IsFinal = request.IsFinal,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.TransactionStatuses.Add(status);
        await _context.SaveChangesAsync();

        // Invalidate cache since we added a new status
        _workflowCache.InvalidateCache();

        _logger.LogInformation("Added new status: {StatusName} (ID: {StatusId})", status.Name, status.Id);

        return new StatusResponse(
            status.Id,
            status.Name,
            status.Description,
            status.IsInitial,
            status.IsFinal,
            status.DisplayOrder
        );
    }

    public async Task<IEnumerable<TransitionResponse>> GetAllTransitionsAsync()
    {
        var transitions = await _context.TransactionStatusTransitions
            .Include(t => t.FromStatus)
            .Include(t => t.ToStatus)
            .OrderBy(t => t.FromStatusId)
            .ThenBy(t => t.ToStatusId)
            .ToListAsync();

        return transitions.Select(t => new TransitionResponse(
            t.Id,
            t.FromStatusId,
            t.FromStatus.Name,
            t.ToStatusId,
            t.ToStatus.Name,
            t.Name,
            t.Description,
            t.RequiresComment,
            t.IsRollback
        ));
    }

    public async Task<TransitionResponse> AddTransitionAsync(CreateTransitionRequest request)
    {
        // Validate that both statuses exist
        var fromStatus = await _context.TransactionStatuses.FindAsync(request.FromStatusId)
            ?? throw new StatusNotFoundException(request.FromStatusId);

        var toStatus = await _context.TransactionStatuses.FindAsync(request.ToStatusId)
            ?? throw new StatusNotFoundException(request.ToStatusId);

        // Check if transition already exists
        var existingTransition = await _context.TransactionStatusTransitions
            .FirstOrDefaultAsync(t => t.FromStatusId == request.FromStatusId && t.ToStatusId == request.ToStatusId);

        if (existingTransition != null)
        {
            throw new TransitionAlreadyExistsException(request.FromStatusId, request.ToStatusId);
        }

        // Cannot create transition from a final status
        if (fromStatus.IsFinal)
        {
            throw new InvalidOperationException($"Cannot create transition from final status '{fromStatus.Name}'");
        }

        var transition = new TransactionStatusTransition
        {
            FromStatusId = request.FromStatusId,
            ToStatusId = request.ToStatusId,
            Name = request.Name,
            Description = request.Description,
            RequiresComment = request.RequiresComment,
            IsRollback = request.IsRollback,
            CreatedAt = DateTime.UtcNow
        };

        _context.TransactionStatusTransitions.Add(transition);
        await _context.SaveChangesAsync();

        // Invalidate cache since we added a new transition
        _workflowCache.InvalidateCache();

        _logger.LogInformation(
            "Added new transition: {TransitionName} ({FromStatus} → {ToStatus})",
            transition.Name, fromStatus.Name, toStatus.Name);

        return new TransitionResponse(
            transition.Id,
            transition.FromStatusId,
            fromStatus.Name,
            transition.ToStatusId,
            toStatus.Name,
            transition.Name,
            transition.Description,
            transition.RequiresComment,
            transition.IsRollback
        );
    }

    public async Task RefreshCacheAsync()
    {
        await _workflowCache.RefreshCacheAsync();
        _logger.LogInformation("Workflow cache refreshed via admin endpoint");
    }
}
