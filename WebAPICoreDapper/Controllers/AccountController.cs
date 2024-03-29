using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPICoreDapper.Extensions;
using WebAPICoreDapper.Filters;
using WebAPICoreDapper.Data.ViewModels;
using WebAPICoreDapper.Data.Models;
using WebAPICoreDapper.Utilities.Constants;
using System.Text.Json;

namespace WebAPICoreDapper.Controllers;

[Route("api/{culture}/[controller]")]
[ApiController]
[MiddlewareFilter(typeof(LocalizationPipeline))]
[Authorize]
public class AccountController(
    UserManager<AppUser> userManager,
    IConfiguration configuration,
    SignInManager<AppUser> signInManager)
    : ControllerBase
{
    private readonly string _connectionString = configuration.GetConnectionString("DbConnectionString");

    [HttpPost]
    [AllowAnonymous]
    [Route("login")]
    [ValidateModel]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        var user = await userManager.FindByNameAsync(model.UserName);
        if (user != null)
        {
            var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, true);
            if (!result.Succeeded)
                return BadRequest("Mật khẩu không đúng");
            var roles = await userManager.GetRolesAsync(user);
            var permissions = await GetPermissionByUserId(user.Id.ToString());
            var claims = new[]
            {
                new Claim("Email", user.Email),
                new Claim(SystemConstants.UserClaim.Id, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(SystemConstants.UserClaim.FullName, user.FullName??string.Empty),
                new Claim(SystemConstants.UserClaim.Roles, string.Join(";", roles)),
                new Claim(SystemConstants.UserClaim.Permissions, JsonSerializer.Serialize(permissions)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(configuration["Tokens:Issuer"],
                configuration["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        return NotFound($"Không tìm thấy tài khoản {model.UserName}");
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("register")]
    [ValidateModel]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var user = new AppUser { FullName = model.FullName, UserName = model.Email, Email = model.Email };

        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // User claim for write customers data
            //await _userManager.AddClaimAsync(user, new Claim("Customers", "Write"));

            //await _signInManager.SignInAsync(user, false);

            return Ok(model);
        }

        return BadRequest();
    }
    private async Task<List<string>> GetPermissionByUserId(string userId)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();

        var parameters = new DynamicParameters();
        parameters.Add("@userId", userId);

        var result = await conn.QueryAsync<string>("Get_Permission_ByUserId", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return result.ToList();
    }
}
