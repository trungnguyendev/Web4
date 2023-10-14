using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Web4.Models.Dto;
using Microsoft.AspNetCore.Authorization;

namespace Web4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if ((regex.IsMatch(registerDto.Email) == false) || (registerDto.Email == null))
            {
                var result = new ModelState
                {
                    statusCode = 401,
                    message = "email không đúng định dạng hoặc không được để trống",
                    error = "Bad Request"
                };
                return Ok(result);
            }
            var identityUser = new IdentityUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber
            };
            var identityResult = await userManager.CreateAsync(identityUser, registerDto.Password);
            if (identityResult.Succeeded)
            {
                if (registerDto.Role != null )
                {
                    identityResult = await userManager.AddToRoleAsync(identityUser, registerDto.Role);
                    if (identityResult.Succeeded)
                    {
                        var result = new ModelState
                        {
                            statusCode = 201,
                            message = "Đăng ký thành công"
                        };
                        var resultDto = new ModelStateDto
                        {
                            statusCode = result.statusCode,
                            message = result.message,
                        };
                        return Ok(resultDto);
                    }
                }
            }
            return Ok("Something went wrong!!");
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginRequestDto)
        {
            var user = await userManager.FindByNameAsync(loginRequestDto.Username);
            var roleUser = await userManager.GetRolesAsync(user);
            if (user != null)
            {
                var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);
                if (checkPasswordResult)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    if (roles != null)
                    {
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Email, user.Email));
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            configuration["Jwt:Issuer"],
                            configuration["Jwt:Audience"],
                            claims,
                            expires: DateTime.Now.AddMinutes(15),
                            signingCredentials: credentials);
                        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
                        HttpContext.Response.Cookies.Append("refresh_token", jwtToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTime.UtcNow.AddMinutes(30),
                            Secure = true,
                            SameSite = SameSiteMode.None
                        });
                        var result = new TokenDto
                        {
                            message = "Đăng nhập thành công",
                            statusCode = 201,
                            access_token = jwtToken,
                            role = (List<string>)roleUser
                        };
                       return Ok(result);
                    }
                }
            }
            var data = new ModelState
            {
                statusCode = 401,
                message = "Tài khoản hoặc mật khẩu không chính xác",
                error = "Bad requests"
            };
            return Ok(data);
        }
        [HttpGet]
        [Route("account")]
        [Authorize(Roles = ("Writer"))]
        public async Task<IActionResult> getMyAccount()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = new TokenDto
            {
                statusCode = 201,
                message = "đăng nhập thành công",
                access_token = token,
                role = User.FindAll(ClaimTypes.Role)
                           .Select(claim => claim.Value)
                           .ToList()
            };
            return Ok(result);
        }
    }
}
