using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWT_token.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;

        // IConfigiration injected t oconstructor so can use env. variables
        public AuthController(IConfiguration config)
        {
            _configuration = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            // generates the Hash n salt from password
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            // check if user exist - in production, will check some DB
            if(user.Username != request.Username)
            {
                return BadRequest("User not found");
            }

            if(!VerifyPasswordHash(request.Password,user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password");
            }

            // after checking credentials - create token
            string token = CreateToken(user);

            return Ok(token);
        }


        // for POC and learning purposes this to be in controller
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // HAMCSHA512 already creates salt automatically so we just access the Key property
            // Key == Salt
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(user.PasswordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                       // compare computeHash == passwordHash
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        // claim in JWT is some property that is saved in the token, username, or ID.. etc..
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                // we can add role to be part of the token
                new Claim(ClaimTypes.Role, "Admin")   //authorized user
            };

            // secret key that will be saved in environment cars
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            // create signing credentials with the key created above
            var signingCreds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // define payload of the JWT
            var token = new JwtSecurityToken(
                claims: claims,
                expires:DateTime.Now.AddDays(1),
                signingCredentials:signingCreds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
            
}
