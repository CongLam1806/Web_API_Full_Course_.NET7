using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LearnAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly LearndataContext _context;
        private readonly IRefreshHandler refreshHandler;
        public AuthorizeController(LearndataContext context, IOptions<JwtSettings> options, IRefreshHandler refreshHandler) 
        { 
            this._context = context;
            this._jwtSettings = options.Value;
            this.refreshHandler = refreshHandler;
        }

        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCred userCred)
        {
            var user = await this._context.TblUsers.FirstOrDefaultAsync(item => item.Code == userCred.username && item.Password == userCred.password);
            if (user != null)
            {
                //generate token
                var tokenhandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this._jwtSettings.securitykey);
                var tokendesc = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Code),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)
                };
                var token = tokenhandler.CreateToken(tokendesc);
                var finaltoken = tokenhandler.WriteToken(token);
                return Ok(new TokenResponse() {Token = finaltoken, RefreshToken =  await this.refreshHandler.GenerateToken(userCred.username)});
            }
            else
            {
                return Unauthorized();
            }
            
        }

        [HttpPost("GenerateRefreshToken")]
        public async Task<IActionResult> GenerateRefreshToken([FromBody] TokenResponse token)
        {
            var _refreshtoken = await this._context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Refreshtoken == token.RefreshToken);
            if (_refreshtoken != null)
            {
                //generate token
                var tokenhandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this._jwtSettings.securitykey);
                SecurityToken securityToken;
                var principal = tokenhandler.ValidateToken(token.Token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenkey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                }, out securityToken);

                var _token = securityToken as JwtSecurityToken;
                if(_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    string username = principal.Identity?.Name;
                    var _existdata = await this._context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Userid == username
                    && item.Refreshtoken == token.RefreshToken);
                    if(_existdata != null)
                    {
                        var _newtoken = new JwtSecurityToken(
                                claims: principal.Claims.ToArray(),
                                expires: DateTime.Now.AddMinutes(2),
                                signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._jwtSettings.securitykey)), 
                                SecurityAlgorithms.HmacSha256)
                            );
                        var _finaltoken = tokenhandler.WriteToken(_newtoken);
                        return Ok(new TokenResponse() { Token = _finaltoken, RefreshToken = await this.refreshHandler.GenerateToken(username) });
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }
                //var tokendesc = new SecurityTokenDescriptor
                //{
                //    Subject = new ClaimsIdentity(new Claim[]
                //    {
                //        new Claim(ClaimTypes.Name, user.Code),
                //        new Claim(ClaimTypes.Role, user.Role)
                //    }),
                //    Expires = DateTime.UtcNow.AddMinutes(2),
                //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)
                //};
                //var token = tokenhandler.CreateToken(tokendesc);
                //var finaltoken = tokenhandler.WriteToken(token);
                //return Ok(new TokenResponse() { Token = finaltoken, RefreshToken = await this.refreshHandler.GenerateToken(userCred.username) });
            }
            else
            {
                return Unauthorized();
            }

        }
    }
}
