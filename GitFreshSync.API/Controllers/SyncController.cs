using GitFreshSync.Application.Dtos.Sync;
using GitFreshSync.Application.Sync.Commands.SyncGitHubToFreshdeskCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitFreshSync.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SyncController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("github-to-freshdesk")]
        public async Task<IActionResult> SyncGitHubToFreshdesk([FromBody] SyncRequestDto request)
        {
            var result = await _mediator.Send(new SyncGitHubToFreshdeskCommand(request.GitHubUsername, request.FreshdeskSubdomain));
            return result ? Ok(result) : BadRequest(result);
        }
    }
}
