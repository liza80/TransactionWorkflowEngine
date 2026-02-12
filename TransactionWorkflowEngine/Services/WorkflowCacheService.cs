using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TransactionWorkflowEngine.Data;
using TransactionWorkflowEngine.Models;

namespace TransactionWorkflowEngine.Services;

/// <summary>
/// In-memory cache for workflow transitions to avoid DB lookups
/// </summary>
public class WorkflowCacheService : IWorkflowCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WorkflowCacheService> _logger;
    
    private const string StatusesCacheKey = "Workflow_Statuses";
    private const string TransitionsCacheKey = "Workflow_Transitions";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30); // TTL

    public WorkflowCacheService(
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        ILogger<WorkflowCacheService> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionStatusTransition>> GetAllowedTransitionsAsync(int fromStatusId)
    {
        var transitions = await GetTransitionsFromCacheAsync();
        return transitions.Where(t => t.FromStatusId == fromStatusId);
    }

    public async Task<TransactionStatusTransition?> GetTransitionAsync(int fromStatusId, int toStatusId)
    {
        var transitions = await GetTransitionsFromCacheAsync();
        return transitions.FirstOrDefault(t => t.FromStatusId == fromStatusId && t.ToStatusId == toStatusId);
    }

    public async Task<IEnumerable<TransactionStatus>> GetAllStatusesAsync()
    {
        return await GetStatusesFromCacheAsync();
    }

    public async Task<TransactionStatus?> GetInitialStatusAsync()
    {
        var statuses = await GetStatusesFromCacheAsync();
        return statuses.FirstOrDefault(s => s.IsInitial);
    }

    public async Task RefreshCacheAsync()
    {
        _logger.LogInformation("Refreshing workflow cache");
        InvalidateCache();
        await GetStatusesFromCacheAsync();
        await GetTransitionsFromCacheAsync();
        _logger.LogInformation("Workflow cache refreshed successfully");
    }

    public void InvalidateCache()
    {
        _cache.Remove(StatusesCacheKey);
        _cache.Remove(TransitionsCacheKey);
        _logger.LogInformation("Workflow cache invalidated");
    }

    private async Task<IList<TransactionStatus>> GetStatusesFromCacheAsync()
    {
        if (_cache.TryGetValue(StatusesCacheKey, out IList<TransactionStatus>? cachedStatuses) && cachedStatuses != null)
        {
            return cachedStatuses;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var statuses = await context.TransactionStatuses
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));

        _cache.Set(StatusesCacheKey, statuses, cacheOptions);
        _logger.LogDebug("Loaded {Count} statuses into cache", statuses.Count);

        return statuses;
    }

    private async Task<IList<TransactionStatusTransition>> GetTransitionsFromCacheAsync()
    {
        if (_cache.TryGetValue(TransitionsCacheKey, out IList<TransactionStatusTransition>? cachedTransitions) && cachedTransitions != null)
        {
            return cachedTransitions;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var transitions = await context.TransactionStatusTransitions
            .AsNoTracking()
            .Include(t => t.FromStatus)
            .Include(t => t.ToStatus)
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));

        _cache.Set(TransitionsCacheKey, transitions, cacheOptions);
        _logger.LogDebug("Loaded {Count} transitions into cache", transitions.Count);

        return transitions;
    }
}
