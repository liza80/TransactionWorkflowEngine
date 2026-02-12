namespace TransactionWorkflowEngine.DTOs;

// Request DTOs
public record CreateTransactionRequest(
    decimal Amount,
    string Currency,
    string CustomerId,
    string? Description
);

public record TransitionRequest(
    int ToStatusId,
    string? Reason
);

// Response DTOs
public record TransactionResponse(
    Guid Id,
    string ReferenceNumber,
    StatusInfo Status,
    decimal Amount,
    string Currency,
    string CustomerId,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record StatusInfo(
    int Id,
    string Name,
    string? Description,
    bool IsInitial,
    bool IsFinal
);

public record AvailableTransitionResponse(
    int Id,
    int ToStatusId,
    string ToStatusName,
    string TransitionName,
    string? Description,
    bool RequiresComment,
    bool IsRollback
);

public record TransactionHistoryResponse(
    int Id,
    StatusInfo FromStatus,
    StatusInfo ToStatus,
    string? Reason,
    DateTime Timestamp
);

public record TransactionDetailResponse(
    Guid Id,
    string ReferenceNumber,
    StatusInfo Status,
    decimal Amount,
    string Currency,
    string CustomerId,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<TransactionHistoryResponse> History
);

// Admin DTOs
public record CreateStatusRequest(
    string Name,
    string? Description,
    bool IsInitial = false,
    bool IsFinal = false,
    int DisplayOrder = 0
);

public record CreateTransitionRequest(
    int FromStatusId,
    int ToStatusId,
    string Name,
    string? Description,
    bool RequiresComment = false,
    bool IsRollback = false
);

public record StatusResponse(
    int Id,
    string Name,
    string? Description,
    bool IsInitial,
    bool IsFinal,
    int DisplayOrder
);

public record TransitionResponse(
    int Id,
    int FromStatusId,
    string FromStatusName,
    int ToStatusId,
    string ToStatusName,
    string Name,
    string? Description,
    bool RequiresComment,
    bool IsRollback
);

// Error response
public record ErrorResponse(
    string Error,
    string? Details = null
);
