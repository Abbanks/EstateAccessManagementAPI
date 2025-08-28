using EstateAccessManagement.Application.Commands.Users;
using EstateAccessManagement.Application.DTOs;
using EstateAccessManagement.Application.Queries.Users;
using EstateAccessManagement.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateAccessManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            // Check if any users exist to handle the bootstrapping scenario
            var userCountQuery = new GetUserCountQuery();
            var userCount = await mediator.Send(userCountQuery);

            if (userCount == 0)
            {
                // First user registration - automatically make them Admin
                var firstUserCommand = new RegisterUserCommand
                {
                    Email = request.Email,
                    Password = request.Password,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserType = UserType.Admin // Force first user to be Admin
                };

                var result = await mediator.Send(firstUserCommand);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            else
            {
                // Subsequent user registrations require Admin authentication
                if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
                {
                    return Forbid("Only administrators can register new users.");
                }

                var command = new RegisterUserCommand
                {
                    Email = request.Email,
                    Password = request.Password,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserType = request.UserType
                };

                var result = await mediator.Send(command);
                return StatusCode(StatusCodes.Status201Created, result);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var query = new LoginUserCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await mediator.Send(query);
            return Ok(result);
        }
    }
}
