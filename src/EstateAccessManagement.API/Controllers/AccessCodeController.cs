using EstateAccessManagement.Application.Features.AccessCodes.Commands;
using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
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
        [HttpPost("generate")]
        [Authorize(Roles = "Resident")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateAccessCode([FromBody] GenerateAccessCodeCommand command)
        {
            var result = await mediator.Send(command);
            return CreatedAtAction(nameof(GenerateAccessCode), new { id = result.Id }, result);
        }

        [HttpPost("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAccessCode(Guid id)
        {
            var result = await mediator.Send(new GetAccessCodeQuery { Id = id });
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("validate")]
        [Authorize(Roles = "Security")]
        [ProducesResponseType(typeof(AccessCodeValidationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateAccessCode([FromQuery] ValidateAccessCodeQuery command)
        {
            var result = await mediator.Send(command);
            if (!result.IsValid)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
