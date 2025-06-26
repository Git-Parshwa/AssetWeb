using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AssetWeb.CustomActionFilters;
using AssetWeb.Data;
using AssetWeb.Models.Domain;
using AssetWeb.Models.DTO;
using AssetWeb.Repositories;
using AssetWeb.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AssetWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly IMapper mapper;
        private readonly EmailService emailService;

        public AuthController(UserManager<User> userManager, ITokenRepository tokenRepository, IMapper mapper, EmailService emailService)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.mapper = mapper;
            this.emailService = emailService;
        }

        [HttpPost]
        [Route("Register")]
        [AllowAnonymous]
        [ValidateModel]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email,
                Email = registerDto.Email
            };

            var checkUserExists = await tokenRepository.CheckUserExists(user);
            if (checkUserExists == true)
            {
                return Ok("User already exists! Please login.");
            }

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                if (registerDto.Roles != null && registerDto.Roles.Any())
                {
                    result = await userManager.AddToRolesAsync(user, registerDto.Roles);
                    if (result.Succeeded)
                    {
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        var encodedToken = System.Net.WebUtility.UrlEncode(token);
                        var confirmationLink = Url.Action(
                            nameof(VerifyEmail),
                            "Auth",
                            new { userId = user.Id, token = encodedToken },
                            Request.Scheme
                        );
                        await emailService.SendEmailAsync(user.Email, "Email Verification",
                        $"Verify Email <a href='{confirmationLink}'>here</a>");
                        return Ok("User Registered! Verify Email & Login.");
                    }
                }
            }
            return BadRequest("Something went wrong!");
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        [AllowAnonymous]
        [ValidateModel]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid User");
            }

            var decodedToken = System.Net.WebUtility.UrlDecode(token);
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                return Ok("Email Verified Successfully!");
            }
            return BadRequest("Email not Verified!");
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(loginDto.Email);
                if (user != null)
                {
                    var result = await userManager.IsEmailConfirmedAsync(user);
                    if (result == false)
                    {
                        return BadRequest("Email not Verified!");
                    }
                    var checkPassword = await userManager.CheckPasswordAsync(user, loginDto.Password);
                    if (checkPassword == true)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        if (roles != null)
                        {
                            await tokenRepository.RevokeExistingRefreshToken(user.Id);
                            var token = tokenRepository.GetJwtToken(user, roles.ToList());
                            var (rawToken, hashedToken) = tokenRepository.GetRefreshToken(user);

                            if (user.RefreshTokens == null)
                            {
                                user.RefreshTokens = new List<RefreshToken>();
                            }

                            user.RefreshTokens.Add(hashedToken);
                            await userManager.UpdateAsync(user);
                            var loginResponseDto = new LoginResponseDto
                            {
                                JwtToken = token,
                                RefreshToken = rawToken
                            };
                            return Ok(loginResponseDto);
                        }
                    }
                }
                return BadRequest("Wrong Username or Password!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Login failed: {ex.Message}");
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ValidateModel]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (userId == null)
            // {
            //     return Unauthorized("Invalid Token");
            // }

            // var user = await tokenRepository.GetCurrentUserAsync(userId);
            // if (user == null)
            // {
            //     return NotFound("User Not Found");
            // }

            var hashedToken = tokenRepository.HashedToken(token);
            // var user = await tokenRepository.GetUserFromTokenAsync(hashedToken);
            var user = await tokenRepository.GetUserFromTokenAsync(token);
            if (user == null)
            {
                return Unauthorized();
            }

            // var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == hashedToken);
            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == token);
            if (refreshToken == null || refreshToken.IsRevoked)
            {
                return Unauthorized();
            }
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.IsRevoked = true;

            var (newRawToken, newHashedToken) = tokenRepository.GetRefreshToken(user);
            user.RefreshTokens.Add(newHashedToken);
            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            var newJwtToken = tokenRepository.GetJwtToken(user, roles.ToList());
            var loginResponseDto = new LoginResponseDto
            {
                JwtToken = newJwtToken,
                RefreshToken = newRawToken
            };
            return Ok(loginResponseDto);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Invalid Token");
            }

            var user = await tokenRepository.GetCurrentUserAsync(userId);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsRevoked == false);
            if (refreshToken == null)
            {
                return Unauthorized();
            }
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.IsRevoked = true;
            await userManager.UpdateAsync(user);
            return Ok("User Logged Out!");
        }      

    }
}