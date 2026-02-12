namespace TransactionWorkflowEngine.Exceptions;

/// <summary>
/// Exception thrown when a transaction is not found
/// </summary>
public class TransactionNotFoundException : Exception
{
    public Guid TransactionId { get; }

    public TransactionNotFoundException(Guid transactionId)
        : base($"Transaction with ID {transactionId} not found")
    {
        TransactionId = transactionId;
    }
}

/// <summary>
/// Exception thrown when an invalid transition is attempted
/// </summary>
public class InvalidTransitionException : Exception
{
    public InvalidTransitionException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict occurs
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a status is not found
/// </summary>
public class StatusNotFoundException : Exception
{
    public int StatusId { get; }

    public StatusNotFoundException(int statusId)
        : base($"Status with ID {statusId} not found")
    {
        StatusId = statusId;
    }
}

/// <summary>
/// Exception thrown when a transition already exists
/// </summary>
public class TransitionAlreadyExistsException : Exception
{
    public TransitionAlreadyExistsException(int fromStatusId, int toStatusId)
        : base($"Transition from status {fromStatusId} to status {toStatusId} already exists")
    {
    }
}

/// <summary>
/// Exception thrown when a status already exists
/// </summary>
public class StatusAlreadyExistsException : Exception
{
    public StatusAlreadyExistsException(string name)
        : base($"Status with name '{name}' already exists")
    {
    }
}
