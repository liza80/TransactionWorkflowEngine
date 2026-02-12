namespace TransactionWorkflowEngine.Models;

/// <summary>
/// Defines allowed transitions between transaction statuses.
/// This enables configurable workflow like Jira.
/// </summary>
public class TransactionStatusTransition
{
    public int Id { get; set; }
    
    /// <summary>
    /// The status from which the transition starts
    /// </summary>
    public int FromStatusId { get; set; }
    public TransactionStatus FromStatus { get; set; } = null!;
    
    /// <summary>
    /// The status to which the transition leads
    /// </summary>
    public int ToStatusId { get; set; }
    public TransactionStatus ToStatus { get; set; } = null!;
    
    /// <summary>
    /// Display name for this transition (e.g., "Approve", "Reject", "Retry")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of when this transition should be used
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Indicates if this transition requires a comment/reason
    /// </summary>
    public bool RequiresComment { get; set; }
    
    /// <summary>
    /// Indicates if this transition is a rollback/retry operation
    /// </summary>
    public bool IsRollback { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
