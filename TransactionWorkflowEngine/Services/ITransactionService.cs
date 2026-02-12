using TransactionWorkflowEngine.DTOs;

namespace TransactionWorkflowEngine.Services;

public interface ITransactionService
{
    /// <summary>
    /// Creates a new transaction with initial status
    /// </summary>
    Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
    
    /// <summary>
    /// Gets a transaction by its ID
    /// </summary>
    Task<TransactionDetailResponse?> GetTransactionAsync(Guid id);
    
    /// <summary>
    /// Transitions a transaction to a new status
    /// </summary>
    Task<TransactionResponse> TransitionAsync(Guid id, TransitionRequest request);
    
    /// <summary>
    /// Gets available transitions for a transaction
    /// </summary>
    Task<IEnumerable<AvailableTransitionResponse>> GetAvailableTransitionsAsync(Guid id);
    
    /// <summary>
    /// Gets the history of a transaction
    /// </summary>
    Task<IEnumerable<TransactionHistoryResponse>> GetTransactionHistoryAsync(Guid id);
}
