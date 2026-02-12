using Microsoft.AspNetCore.Mvc;
using TransactionWorkflowEngine.DTOs;
using TransactionWorkflowEngine.Exceptions;
using TransactionWorkflowEngine.Services;

namespace TransactionWorkflowEngine.Controllers;

[ApiController]
[Route("transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new transaction
    /// </summary>
    /// <param name="request">The transaction creation request</param>
    /// <returns>The created transaction</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var transaction = await _transactionService.CreateTransactionAsync(request);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create transaction");
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Gets a transaction by ID
    /// </summary>
    /// <param name="id">The transaction ID</param>
    /// <returns>The transaction details including history</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var transaction = await _transactionService.GetTransactionAsync(id);
        
        if (transaction == null)
        {
            return NotFound(new ErrorResponse($"Transaction with ID {id} not found"));
        }

        return Ok(transaction);
    }

    /// <summary>
    /// Transitions a transaction to a new status
    /// </summary>
    /// <param name="id">The transaction ID</param>
    /// <param name="request">The transition request containing target status and optional reason</param>
    /// <returns>The updated transaction</returns>
    [HttpPost("{id:guid}/transition")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TransitionTransaction(Guid id, [FromBody] TransitionRequest request)
    {
        try
        {
            var transaction = await _transactionService.TransitionAsync(id, request);
            return Ok(transaction);
        }
        catch (TransactionNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (InvalidTransitionException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Gets available transitions for a transaction
    /// </summary>
    /// <param name="id">The transaction ID</param>
    /// <returns>List of available transitions</returns>
    [HttpGet("{id:guid}/available-transitions")]
    [ProducesResponseType(typeof(IEnumerable<AvailableTransitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableTransitions(Guid id)
    {
        try
        {
            var transitions = await _transactionService.GetAvailableTransitionsAsync(id);
            return Ok(transitions);
        }
        catch (TransactionNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Gets the history of a transaction
    /// </summary>
    /// <param name="id">The transaction ID</param>
    /// <returns>List of status change history records</returns>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IEnumerable<TransactionHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionHistory(Guid id)
    {
        try
        {
            var history = await _transactionService.GetTransactionHistoryAsync(id);
            return Ok(history);
        }
        catch (TransactionNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
    }
}
