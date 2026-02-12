namespace TransactionWorkflowEngine.Models;

/// <summary>
/// Represents a customer transaction that progresses through workflow statuses.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// External reference number for the transaction
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the transaction
    /// </summary>
    public int StatusId { get; set; }
    public TransactionStatus Status { get; set; } = null!;
    
    /// <summary>
    /// Transaction amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Currency code (e.g., "USD", "EUR")
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Customer identifier
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description or notes
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    public byte[]? RowVersion { get; set; }
    
    // Navigation property for audit trail
    public ICollection<TransactionHistory> History { get; set; } = new List<TransactionHistory>();
}
