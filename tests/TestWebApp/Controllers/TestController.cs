using Microsoft.AspNetCore.Mvc;

namespace TestWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello from test API!" });
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        return Ok(new { id = id, message = $"Hello from test API with id {id}!" });
    }

    [HttpPost]
    public IActionResult Post([FromBody] object data)
    {
        return CreatedAtAction(nameof(Get), new { id = 1 }, data);
    }
} 