using Microsoft.AspNetCore.Mvc;
using pokearcanumbe.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace pokearcanumbe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<User> user, IConfiguration config) : ControllerBase
    {
        private readonly UserManager<User> _user = user;
        private readonly IConfiguration _config = config;

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegDto dto)
        {
            if (await _user.FindByEmailAsync(dto.Email) != null) return BadRequest("Email already registered!");
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.UserName
            };

            var result = await _user.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("User Registration successful!");

        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _user.FindByEmailAsync(dto.Email);
            if (user == null || !await _user.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials!");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokenHash = HashToken(refreshToken.Token);
            user.RefreshTokenExpiryTime = refreshToken.Expires;

            var result = await _user.UpdateAsync(user);

            if (!result.Succeeded) return Unauthorized("Unexpected error occured!");

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshDto dto)
        {
            var hash = HashToken(dto.RefreshToken);

            var user = await _user.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow) return Unauthorized(new { error = "Invalid refresh token!" });

            var accessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshTokenHash = HashToken(newRefreshToken.Token);
            user.RefreshTokenExpiryTime = newRefreshToken.Expires;

            var result = await _user.UpdateAsync(user);

            if (!result.Succeeded) return Unauthorized("Unexpected error occured!");
            
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not found.");

            var user = await _user.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var passwordValid = await _user.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordValid) return Unauthorized("Invalid current password.");

            if (!string.IsNullOrEmpty(dto.UserName) && dto.UserName != user.UserName)
                user.UserName = dto.UserName;

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                var token = await _user.GeneratePasswordResetTokenAsync(user);
                var result = await _user.ResetPasswordAsync(user, token, dto.NewPassword);
                if (!result.Succeeded) return BadRequest(result.Errors);
            }

            await _user.UpdateAsync(user);

            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUserCount()
        {
            var count = await _user.Users.CountAsync();
            return Ok(new { count });
        }

        private string GenerateJwtToken(User user)
        {
            var JwtConfig = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfig["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            var token = new JwtSecurityToken(
                issuer: JwtConfig["Issuer"],
                audience: JwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(JwtConfig["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private (string Token, DateTime Expires) GenerateRefreshToken()
        {
            var JwtConfig = _config.GetSection("Jwt");
            var rand = new byte[64];
            using var randGenerator = RandomNumberGenerator.Create();
            randGenerator.GetBytes(rand);
            return (
                Convert.ToBase64String(rand),
                DateTime.UtcNow.AddDays(int.Parse(JwtConfig["RefreshTokenExpireDays"]!))
            );
        }

        private string HashToken(string Token)
        {
            var bytes = Encoding.UTF8.GetBytes(Token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}