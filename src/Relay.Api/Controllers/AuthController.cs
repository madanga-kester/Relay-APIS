using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Contracts;
using Relay.Application.Services;
using ApplicationAuthenticationService = Relay.Application.Services.IAuthenticationService;

namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(ApplicationAuthenticationService authentication) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken cancellationToken) => Ok(await authentication.RegisterAsync(request, cancellationToken));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await authentication.ValidateCredentialsAsync(request, cancellationToken);
        if (user is null) return Unauthorized(new { title = "Invalid credentials", status = 401 });
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Name, user.DisplayName), new Claim(ClaimTypes.Role, user.Role.ToString()) };
        await HttpContext.SignInAsync("RelayCookie", new ClaimsPrincipal(new ClaimsIdentity(claims, "RelayCookie")));
        var csrf = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        Response.Cookies.Append("relay.csrf", csrf, CsrfCookieOptions());
        return Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("RelayCookie");
        Response.Cookies.Delete("relay.csrf");
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new { id = User.FindFirstValue(ClaimTypes.NameIdentifier), email = User.FindFirstValue(ClaimTypes.Email), displayName = User.FindFirstValue(ClaimTypes.Name), role = User.FindFirstValue(ClaimTypes.Role) });

    [HttpGet("csrf")]
    [AllowAnonymous]
    public IActionResult Csrf()
    {
        var csrf = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        Response.Cookies.Append("relay.csrf", csrf, CsrfCookieOptions());
        return Ok(new { token = csrf });
    }

    private CookieOptions CsrfCookieOptions() => new()
    {
        HttpOnly = false,
        Secure = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue("Auth:RequireHttps", !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()),
        SameSite = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue("Auth:CrossSiteCookies", false) ? SameSiteMode.None : SameSiteMode.Lax,
        IsEssential = true
    };

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken) { await authentication.RequestPasswordResetAsync(request, cancellationToken); return Accepted(); }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken) => await authentication.ResetPasswordAsync(request, cancellationToken) ? NoContent() : BadRequest(new { title = "Reset link is invalid or expired", status = 400 });








    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken) => await authentication.VerifyEmailAsync(request, cancellationToken) ? NoContent() : BadRequest(new { title = "Verification code is invalid or expired", status = 400 });

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification(ResendVerificationRequest request, CancellationToken cancellationToken) { await authentication.ResendVerificationAsync(request, cancellationToken); return NoContent(); }
}
