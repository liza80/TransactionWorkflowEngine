using TransactionWorkflowEngine.DTOs;

namespace TransactionWorkflowEngine.Services;

/// <summary>
/// Service for managing workflow configuration (statuses and transitions)
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Gets all statuses
    /// </summary>
    Task<IEnumerable<StatusResponse>> GetAllStatusesAsync();
    
    /// <summary>
    /// Adds a new status
    /// </summary>
    Task<StatusResponse> AddStatusAsync(CreateStatusRequest request);
    
    /// <summary>
    /// Gets all transitions
    /// </summary>
    Task<IEnumerable<TransitionResponse>> GetAllTransitionsAsync();
    
    /// <summary>
    /// Adds a new transition
    /// </summary>
    Task<TransitionResponse> AddTransitionAsync(CreateTransitionRequest request);
    
    /// <summary>
    /// Refreshes the workflow cache
    /// </summary>
    Task RefreshCacheAsync();
}
