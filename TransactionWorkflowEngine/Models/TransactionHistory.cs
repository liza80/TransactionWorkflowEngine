namespace TransactionWorkflowEngine.Models;

/// <summary>
/// Audit trail for transaction status changes.
/// </summary>
public class TransactionHistory
{
    public int Id { get; set; }
    
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;
    
    /// <summary>
    /// The status the transaction had before the change
    /// </summary>
    public int FromStatusId { get; set; }
    public TransactionStatus FromStatus { get; set; } = null!;
    
    /// <summary>
    /// The status the transaction changed to
    /// </summary>
    public int ToStatusId { get; set; }
    public TransactionStatus ToStatus { get; set; } = null!;
    
    /// <summary>
    /// Optional comment provided during the transition
    /// </summary>
    public string? Comment { get; set; }
    
    /// <summary>
    /// User who made the transition (if applicable)
    /// </summary>
    public string? ChangedBy { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
