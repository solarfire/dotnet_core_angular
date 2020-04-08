using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /** 
    Extend Controller allows us to validate, http responses etc. 
    Controller - with View Support
    ControllerBase - withOUT view support .  So client (e.g. angular will be view support)
    */
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuartion;
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository, IConfiguration configuartion)
        {
            this._configuartion = configuartion;
            this._authRepository = authRepository;
        }
        /* From Body not needed as it will work it out */
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegistrationDto userForRegistrationDto)
        {

            /* 
            only needed if this Controller is annotated with [Controller] and not [ApiController] to invoke the bean validation.
            if(!ModelState.IsValid)
                return BadRequest(ModelState);*/

            userForRegistrationDto.Username = userForRegistrationDto.Username.ToLower(); // allow user to use username case-insenstive.

            if (await _authRepository.UserExists(userForRegistrationDto.Username))
            {
                return BadRequest("Username already exists");
            }

            var newUser = new User()
            {
                Username = userForRegistrationDto.Username
            };

            var createdUser = await _authRepository.Register(newUser, userForRegistrationDto.Password);

            return StatusCode(201); //TODO Send back location of new resource
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserForLoginDto userForLoginDto)
        {
            var userInRepo = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (null == userInRepo)
                return Unauthorized();

            /* 1. Create Claims */
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userInRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userInRepo.Username)
            };
            /* 2. Key to sign the token.  
            The key will be held securley.  But now in appsettings.json */
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuartion.GetSection("AppSettings:Token").Value));

            /* 3. Signing Credentials 
            Hash the key with an algorithim.*/
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); /*in jwt header*/

            /* 4. Create a Security Token Descriptor */
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddDays(1), //expire in 1 day (24 hours)
                SigningCredentials = credentials
            };

            /* 5. Token Handler */
            var tokenHandler = new JwtSecurityTokenHandler();

            /* 6. Token.
            With the token handler we can create a token and pass in the token descriptor.
            This contains the Jwt token. So return it.*/
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                _token = tokenHandler.WriteToken(token)
            });
        }
    }


}