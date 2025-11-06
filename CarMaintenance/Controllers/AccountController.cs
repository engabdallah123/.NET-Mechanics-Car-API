using Domain.DTO.Account;
using Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;

        private readonly JWTOption jwtOption;

        public AccountController( UserManager<AppUser> userManager,JWTOption jwtOption)
        {
            this.userManager = userManager;
            this.jwtOption = jwtOption;
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] RegisterDTO registerDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AppUser user = new AppUser
                    {
                        UserName = registerDto.FullName,
                        Email = registerDto.Email,

                    };
                    var result = await userManager.CreateAsync(user, registerDto.Password);
                    if (result.Succeeded)
                    {
                        // generate token 
                        var secretKey = jwtOption.SecretKey;
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                        var crids = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var roles = await userManager.GetRolesAsync(user);
                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim("userID",user.Id),
                        };

                        // add roles as claims
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        var tokenDiscription = new SecurityTokenDescriptor()
                        {
                            Subject = new ClaimsIdentity(claims),
                            SigningCredentials = crids,
                            Expires = DateTime.UtcNow.AddDays(1)

                        };
                        var token = new JwtSecurityTokenHandler().CreateToken(tokenDiscription);
                        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                        return Ok(new
                        {
                            token = tokenString,
                            Expire = token.ValidTo
                        });
                    }
                    else
                    {
                        return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
                    }
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { Error = ex.Message });
            }
        }

        [HttpPost("LogIn")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await userManager.FindByNameAsync(loginDTO.UserName);
                    if (user != null)
                    {
                        var result = await userManager.CheckPasswordAsync(user, loginDTO.Password);
                        if (result == true)
                        {
                            // generate token

                            var secretKey = jwtOption.SecretKey;
                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                            var crids = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                            var roles = await userManager.GetRolesAsync(user);
                            var claims = new List<Claim>
                            {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim("userID",user.Id),
                            };

                            // add roles as claims
                            foreach (var role in roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                            var tokenDiscription = new SecurityTokenDescriptor()
                            {
                                Subject = new ClaimsIdentity(claims),
                                SigningCredentials = crids,
                                Expires = DateTime.UtcNow.AddDays(1)

                            };
                            var token = new JwtSecurityTokenHandler().CreateToken(tokenDiscription);
                            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                            return Ok(new
                            {
                                token = tokenString,
                                Expire = DateTime.UtcNow.AddDays(1)
                            });

                        }
                        return BadRequest("UserName Or Password Wrong");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "UserName Or Password Wrong");
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        [HttpGet("Labors")]
        public async Task<IActionResult> GetAllLabors()
        {
            var users = await userManager.Users.ToListAsync();
            var labors = new List<LaborsDTO>();
            foreach (var user in users)
            {
                var labor = new LaborsDTO
                {
                    LaborId = user.Id,
                    LaborName = user.UserName
                };
                labors.Add(labor);
            }
            return Ok(labors);
        }
    }
}
