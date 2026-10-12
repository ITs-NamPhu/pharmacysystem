using PharmacyManagement.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace PharmacyManagement.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [AllowAnonymous] // cho phép truy cập không cần token
        // [EnableRateLimiting("Limit_Per_IP")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _service.LoginAsync(request);

            return Success(result, "Login successfully.");
        }

        [AllowAnonymous]
        [EnableRateLimiting("Limit_Per_IP")]
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken_Receive(RefreshTokenRequest request)
        {
            var result = await _service.RefreshTokenAsync(request);

            return Success( result, "Refresh successfully.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            var result = await _service.LogoutAsync(request);
            if (!result)
            {
                return NotFound("Refresh token not found.");
            }

            return Success(new
            {
                message = "Logout successfully."
            });
        }
    }
}
