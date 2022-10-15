using JwtApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;


namespace JwtApp.Controllers
{ 
    [ApiController]
    public class InitController : ControllerBase
    {
        private IConfiguration _config;

        public InitController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("api/init")]
        public IActionResult Init([FromBody] UserLogin userLogin)
        {
            var token = GenerateNewUser(userLogin);
             
            if (token != null)
            {
                var data = new { token = token }; 
                return new JsonResult(new { status = "success", data = data });
            } 
            return new JsonResult(new { status = "failed"});
        }

        private string GenerateNewUser(UserLogin user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            WalletController.InsertWallet(user.customer_xid);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.customer_xid.ToString())
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token); 
        }
         
    }
}
