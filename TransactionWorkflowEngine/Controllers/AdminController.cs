using Microsoft.AspNetCore.Mvc;
using TransactionWorkflowEngine.DTOs;
using TransactionWorkflowEngine.Exceptions;
using TransactionWorkflowEngine.Services;

namespace TransactionWorkflowEngine.Controllers;

[ApiController]
[Route("admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all workflow statuses
    /// </summary>
    /// <returns>List of all statuses</returns>
    [HttpGet("statuses")]
    [ProducesResponseType(typeof(IEnumerable<StatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllStatuses()
    {
        var statuses = await _adminService.GetAllStatusesAsync();
        return Ok(statuses);
    }

    /// <summary>
    /// Adds a new status to the workflow
    /// </summary>
    /// <param name="request">The status creation request</param>
    /// <returns>The created status</returns>
    [HttpPost("statuses")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddStatus([FromBody] CreateStatusRequest request)
    {
        try
        {
            var status = await _adminService.AddStatusAsync(request);
            return CreatedAtAction(nameof(GetAllStatuses), null, status);
        }
        catch (StatusAlreadyExistsException ex)
        {
            return Conflict(new ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Gets all workflow transitions
    /// </summary>
    /// <returns>List of all transitions</returns>
    [HttpGet("transitions")]
    [ProducesResponseType(typeof(IEnumerable<TransitionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTransitions()
    {
        var transitions = await _adminService.GetAllTransitionsAsync();
        return Ok(transitions);
    }

    /// <summary>
    /// Adds a new transition to the workflow
    /// </summary>
    /// <param name="request">The transition creation request</param>
    /// <returns>The created transition</returns>
    [HttpPost("transitions")]
    [ProducesResponseType(typeof(TransitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddTransition([FromBody] CreateTransitionRequest request)
    {
        try
        {
            var transition = await _adminService.AddTransitionAsync(request);
            return CreatedAtAction(nameof(GetAllTransitions), null, transition);
        }
        catch (StatusNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (TransitionAlreadyExistsException ex)
        {
            return Conflict(new ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Refreshes the workflow cache
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("cache/refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshCache()
    {
        await _adminService.RefreshCacheAsync();
        return Ok(new { message = "Cache refreshed successfully" });
    }
}
