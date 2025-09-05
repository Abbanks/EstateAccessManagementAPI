using EstateAccessManagement.Application.Features.AccessCodes.Commands;
using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateAccessManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccessCodesController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Resident")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAccessCode([FromBody] CreateAccessCodeCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        [HttpGet("validate")]
        [Authorize(Roles = "Security")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateAccessCode([FromQuery] ValidateAccessCodeQuery query)
        {
            if (string.IsNullOrEmpty(query.Code))
            {
                return BadRequest("Access code is required.");
            }

            var result = await mediator.Send(query);

            if (result.IsValid)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
