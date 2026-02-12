namespace TransactionWorkflowEngine.Models;

/// <summary>
/// Represents a status that a transaction can have.
/// Statuses are data-driven, not hardcoded enums.
/// </summary>
public class TransactionStatus
{
    public int Id { get; set; }
    
    /// <summary>
    /// The name of the status (e.g., "Pending", "Processing", "Completed")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of what this status means
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Indicates if this is an initial status for new transactions
    /// </summary>
    public bool IsInitial { get; set; }
    
    /// <summary>
    /// Indicates if this is a final status (no further transitions allowed)
    /// </summary>
    public bool IsFinal { get; set; }
    
    /// <summary>
    /// Display order for UI purposes
    /// </summary>
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<TransactionStatusTransition> TransitionsFrom { get; set; } = new List<TransactionStatusTransition>();
    public ICollection<TransactionStatusTransition> TransitionsTo { get; set; } = new List<TransactionStatusTransition>();
}
