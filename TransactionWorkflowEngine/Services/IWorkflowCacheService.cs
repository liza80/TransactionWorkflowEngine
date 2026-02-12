using TransactionWorkflowEngine.Models;

namespace TransactionWorkflowEngine.Services;

/// <summary>
/// Service for caching workflow transitions in memory
/// to avoid DB lookup on every transition
/// </summary>
public interface IWorkflowCacheService
{
    /// <summary>
    /// Gets all allowed transitions from a specific status
    /// </summary>
    Task<IEnumerable<TransactionStatusTransition>> GetAllowedTransitionsAsync(int fromStatusId);
    
    /// <summary>
    /// Gets a specific transition if it's allowed
    /// </summary>
    Task<TransactionStatusTransition?> GetTransitionAsync(int fromStatusId, int toStatusId);
    
    /// <summary>
    /// Gets all statuses
    /// </summary>
    Task<IEnumerable<TransactionStatus>> GetAllStatusesAsync();
    
    /// <summary>
    /// Gets the initial status for new transactions
    /// </summary>
    Task<TransactionStatus?> GetInitialStatusAsync();
    
    /// <summary>
    /// Refreshes the cache from database
    /// </summary>
    Task RefreshCacheAsync();
    
    /// <summary>
    /// Invalidates the cache
    /// </summary>
    void InvalidateCache();
}
