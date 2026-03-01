#nullable enable
using MediatR;
using MeetSpace.API.Authorization;
using MeetSpace.Application.Features.Bookings.CancelBooking;
using MeetSpace.Application.Features.Bookings.CreateBooking;
using MeetSpace.Application.Features.Bookings.GetUserBookings;
using MeetSpace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetSpace.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Policy = Policies.EmployeeOrAbove)]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator) => _mediator = mediator;

    public record CreateBookingRequest(
        Guid OfficeId,
        Guid RoomId,
        DateOnly Date,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Title
    );

    /// <summary>Returns all bookings (active and cancelled) for the current user (FR-12).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BookingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub")
                  ?? string.Empty;

        var result = await _mediator.Send(new GetUserBookingsQuery(userId), ct);
        return Ok(result);
    }

    /// <summary>Creates a new booking (FR-11). All authenticated users.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub")
                  ?? string.Empty;

        var cmd = new CreateBookingCommand(
            request.OfficeId,
            request.RoomId,
            request.Date,
            request.StartTime,
            request.EndTime,
            request.Title,
            userId
        );

        var id = await _mediator.Send(cmd, ct);
        return Created($"/api/bookings/{id}", id);
    }

    /// <summary>Cancels a booking (FR-13). Owner or Manager/Admin.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub")
                  ?? string.Empty;

        var isManagerOrAdmin = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.OfficeManager);

        try
        {
            await _mediator.Send(new CancelBookingCommand(id, userId, isManagerOrAdmin), ct);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        return NoContent();
    }
}
