using MediatR;
using MeetSpace.Application.Features.Rooms.CreateRoom;
using MeetSpace.Application.Features.Rooms.DeactivateRoom;
using MeetSpace.Application.Features.Rooms.UpdateRoom;
using MeetSpace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetSpace.API.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize(Policy = Policies.ManagerOrAbove)]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Adds a new room to an office. Manager or Admin (FR-6).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateRoomCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"/api/rooms/{id}", id);
    }

    /// <summary>Updates a room. Manager or Admin (FR-7).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { title = "ID mismatch", status = 400 });

        await _mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Deactivates a room (soft delete). Manager or Admin (FR-8).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateRoomCommand(id), ct);
        return NoContent();
    }
}
