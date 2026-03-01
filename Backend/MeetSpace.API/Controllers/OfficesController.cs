using MediatR;
using MeetSpace.API.Authorization;
using MeetSpace.Application.Features.Offices;
using MeetSpace.Application.Features.Offices.AssignManager;
using MeetSpace.Application.Features.Offices.CreateOffice;
using MeetSpace.Application.Features.Offices.DeactivateOffice;
using MeetSpace.Application.Features.Offices.GetOffices;
using MeetSpace.Application.Features.Offices.RemoveManager;
using MeetSpace.Application.Features.Offices.UpdateOffice;
using MeetSpace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MeetSpace.API.Controllers;

[ApiController]
[Route("api/offices")]
[Authorize]   // base: authenticated; each action narrows further
public class OfficesController : ControllerBase
{
    private readonly IMediator _mediator;

    public OfficesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns offices with their rooms and assigned managers.
    /// Admin → all offices. OfficeManager → only their assigned offices (FR-9).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    [ProducesResponseType(typeof(IReadOnlyList<OfficeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        string? filterUserId = User.IsInRole(UserRoles.OfficeManager)
            ? User.FindFirstValue(Claims.Subject)
            : null;

        var result = await _mediator.Send(new GetOfficesQuery(filterUserId), ct);
        return Ok(result);
    }

    /// <summary>Creates a new office, optionally with initial rooms. Admin only (FR-3).</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOfficeCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }

    /// <summary>Updates an office's name, address and status. Admin only (FR-4).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOfficeCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { title = "ID mismatch", status = 400 });

        await _mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Deactivates an office (soft delete). Admin only (FR-5).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateOfficeCommand(id), ct);
        return NoContent();
    }

    // ─── Manager Assignment Endpoints ─────────────────────────────────────────

    /// <summary>Assigns an OfficeManager user to an office. Admin only.</summary>
    [HttpPost("{id:guid}/managers")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignManager(Guid id, [FromBody] AssignManagerRequest body, CancellationToken ct)
    {
        await _mediator.Send(new AssignManagerCommand(id, body.UserId), ct);
        return NoContent();
    }

    /// <summary>Removes an OfficeManager assignment from an office. Admin only.</summary>
    [HttpDelete("{id:guid}/managers/{userId}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveManager(Guid id, string userId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveManagerCommand(id, userId), ct);
        return NoContent();
    }
}

/// <summary>Request body for the assign-manager endpoint.</summary>
public record AssignManagerRequest(string UserId);
