using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecureApiWithAuth.Services;
using System.Security.Claims;

namespace SecureApiWithAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IClientService service;

        public UserController(IClientService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> GetRoleAsync()
        {
            return Ok(GetCurrentRole());
        }

        [HttpGet("onlyUser")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<string>> GetRoleUserAsync()
        {
            return Ok(GetCurrentRole());
        }

        [HttpGet("adminUser")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<string>> GetRoleUserAsync1()
        {
            return Ok("Hello we are both Admin / user");
        }


        [HttpGet("allowall")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> AllowAnonymous()
        {
            return Ok("Open to all");
        }

        //Get current role from the incoming token
        private string? GetCurrentRole()
        {
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var userClaims = identity.Claims;
                return userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            }
            return null;
        }
    }
}
