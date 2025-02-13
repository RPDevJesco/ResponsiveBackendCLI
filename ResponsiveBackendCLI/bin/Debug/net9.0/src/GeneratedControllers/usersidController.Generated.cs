
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GeneratedControllers
{
    [Authorize] // Enforce JWT authentication and optional role-based authorization
    [ApiController]
    [Route("/users/{id}")]
    public partial class usersidController : ControllerBase
    {
        [HttpGet("/users/{id}")]
        public IActionResult Get()
        {
            return GetImplementation();
        }

        partial IActionResult GetImplementation();
    }
}