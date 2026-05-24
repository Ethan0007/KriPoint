using Microsoft.AspNetCore.Mvc;

namespace KriPoint.TestApi.Controllers;

public record CreateUserRequest(string Email, string FullName, string Role, decimal Salary);
public record UpdateUserRequest(string FullName, string Role);
public record UserResponse(Guid Id, string Email, string FullName, string Role, DateTime CreatedAt);

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        var user = new UserResponse(
            Id:        Guid.NewGuid(),
            Email:     request.Email,
            FullName:  request.FullName,
            Role:      request.Role,
            CreatedAt: DateTime.UtcNow
        );

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        return Ok(new { id, request.FullName, request.Role, UpdatedAt = DateTime.UtcNow });
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        return Ok(new UserResponse(id, "joever@corp.com", "joever Smith", "admin", DateTime.UtcNow));
    }
}
