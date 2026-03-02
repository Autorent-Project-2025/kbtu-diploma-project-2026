using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request, [FromQuery] bool returnid = false, [FromQuery] bool returnrole = false)
        {
            try
            {
                var result = await _authService.Register(request.Name, request.Email, request.Password, request.Role);
                if (returnid && returnrole)
                {
                    return Ok(new { userid = result.UserId, role = result.RoleName });
                }

                if (returnid)
                {
                    return Ok(new { userid = result.UserId });
                }

                if (returnrole)
                {
                    return Ok(new { role = result.RoleName });
                }

                return Ok(new { message = result.Message });
            }
            catch (UserAlreadyExistsException userEx)
            {
                return Conflict(new { message = userEx.Message });
            }
            catch (InvalidOperationException roleEx)
            {
                return BadRequest(new { message = roleEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var token = await _authService.Login(request.Email, request.Password);
                return Ok(new { token });
            }
            catch (InvalidCredentialsException credEx)
            {
                return Unauthorized(new { message = "Invalid credentials", error = credEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
