using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecureApiWithAuth.Data.Entity;
using SecureApiWithAuth.Services;

namespace SecureApiWithAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IClientService service;
        public AccountController(IClientService service)
        {
            this.service = service;
        }

        [HttpPost("Registration")]
        [AllowAnonymous]
        public async Task<ActionResult<(bool flag, string message)>> RegisterUserAsync(Registration model)
        {
            if (model is null) return BadRequest();
            var (flag, Message) = await service.RegisterUserAsync(model);
            if (flag)
                return Ok(flag);
            else
                return BadRequest(Message);
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<(bool flag, string message)>> LoginUserAsync(Login model)
        {
            if (model is null) return BadRequest();
            var (flag, Token) = await service.LoginUserAsync(model);
            if (flag)
                return Ok(Token);
            else
                return BadRequest(flag);
        }
    }
}
