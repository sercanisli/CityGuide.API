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
        private IConfiguration _configuration; //key değerini okuyabilmek için ekliyoruz.
        public AuthController(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }
        [HttpPost("register")]
        //oluşturduğumuz dto üzerinden işlemlerimizi yapacağız.
        public async Task<IActionResult> Register([FromBody] UserForRegisterDTO userForRegisterDTO)
        {
            if(await _authRepository.UserExists(userForRegisterDTO.UserName))//register olmaya çalışan kullanıcı daha önce sistemimizde var mı?
            {
                ModelState.AddModelError("UserName", "Username already exists"); //kullanıcı zaten varsa modelerror
            }
            if(!ModelState.IsValid) //
            {
                return BadRequest(ModelState);
            }

            var userToCreate = new User
            { //şifre hash tarafından oluşacağı için sadce usernami aldı
                UserName = userForRegisterDTO.UserName
            };
            //username i ve oluşturulan passwordu Register isimli methodumuza yolladık.
            var createdUser = await _authRepository.Register(userToCreate, userForRegisterDTO.Password);
            return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login ([FromBody]UserForLoginDTO userForLoginDTO)
        {
            var user = await _authRepository.Login(userForLoginDTO.UserName, userForLoginDTO.Password);
            if(user==null) //kullanıcı bulunamadyısa
            {
                return Unauthorized();
            }
            //kullanıcı bulunduysa bir token yollamamız gerekiyor.
            //o token ile oturu açık kalabilsin.
            var tokenHandler = new JwtSecurityTokenHandler(); // JwtSecurityTokenHandler sınıfı hazır bir kütüphaned,rkullanılabilir.
            //biz appSetting içerisindeki key ile token üretiyorduk. Ona ulaşmamız gerekiyor.
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSetting:Token").Value); //token keyine Congigurationdan ulaşacağız.
                                                                                                     //AppStting dosyasında token a karşılık gelen nesnenin değerini al
                                                                                                     //Token üreteceğiz ama bu token içerisinde neleri tutacağız.
                                                                                                     //bunu da securtiy token discriptor (güvenlik jeton tanımı) kullanarak gerçekleştireceğiz.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] //bu nesne token da tutmak istediğimiz temel bilgileri tutuyor.
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), //userId yi NameIdentifier ile tutarız
                    new Claim(ClaimTypes.Name,user.UserName) //usernami tuttuk.
                }),
                Expires = DateTime.Now.AddDays(1),      //bu token 1 gün geçerliişlemi.
                //bu tokeni kullanmakiçin keyimizi ve hangi hash algoritmasını kullandığımızı belirtmemiz gerekiyor.
                //SymmetricSecurityKey kullanıyorum. Security algoritması olarak da HMASHA512 kullanıyorum;
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            //artık her şey tamam tokeni üretmemiz gerekiyor;
            var token = tokenHandler.CreateToken(tokenDescriptor); //tokenhandler kullanarak token oluştur. Token descritoragöre
            var tokenString = tokenHandler.WriteToken(token); //write token ile tokenımızın stringini aldık.

            return Ok(tokenString);
        }
    }
}