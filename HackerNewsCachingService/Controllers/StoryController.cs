using Microsoft.AspNetCore.Mvc;

namespace HackerNewsCachingService.Controllers;

[Route("api/[Controller]")]
[ApiController]
public class StoriesController(BestStoriesReadModel readModel) : ControllerBase
{
    [HttpGet("best/{n}")]
    public IActionResult GetBestStories(int n)
    {
        return Ok(readModel.GetBestStories(n));
    }
}