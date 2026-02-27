using MediatR;
using MeetSpace.Application.Features.Users.CreateUser;
using MeetSpace.Application.Features.Users.GetUsers;
using MeetSpace.Application.Features.Users.UpdateUser;
using MeetSpace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetSpace.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.AdminOnly)]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns the list of all users. Admin only (FR-14).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetUsersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Creates a new user. Admin only (FR-14).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing user's details. Admin only (FR-15).
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { title = "ID mismatch", status = 400 });

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
