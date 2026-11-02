using Application.Services.BackgroundJobs;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackgroundJobsController : ControllerBase
    {
        private readonly IBackgroundJobService _service;

        public BackgroundJobsController(
            IBackgroundJobService service)
        {
            _service = service;
        }

        [HttpPost("enqueue")]
        public async Task<IActionResult> Enqueue(
            [FromBody] EnqueueBackgroundJobsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.EnqueueAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
    }
}
