using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CityGuide.API.Data;
using CityGuide.API.DTOs;
using CityGuide.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CityGuide.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthRepository _authRepository;
        private IConfiguration _configuration;
        public AuthController(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDTO userForRegisterDTO)
        {
            if(await _authRepository.UserExists(userForRegisterDTO.UserName))
            {
                ModelState.AddModelError("UserName", "Username already exists");
            }
            if(!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var userToCreate = new User
            {
                UserName = userForRegisterDTO.UserName
            };
            
            var createdUser = await _authRepository.Register(userToCreate, userForRegisterDTO.Password);
            return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login ([FromBody]UserForLoginDTO userForLoginDTO)
        {
            var user = await _authRepository.Login(userForLoginDTO.UserName, userForLoginDTO.Password);
            if(user==null) 
            {
                return Unauthorized();
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSetting:Token").Value); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                    new Claim(ClaimTypes.Name,user.UserName)
                }),
                Expires = DateTime.Now.AddDays(1), 
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor); 
            var tokenString = tokenHandler.WriteToken(token); 

            return Ok(tokenString);
        }
    }
}
