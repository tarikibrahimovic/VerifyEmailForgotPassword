using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Text;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using VerifyEmailForgotPassword.Data.Model;
using VerifyEmailForgotPassword.Data.ViewModel;

namespace VerifyEmailForgotPassword.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private IConfiguration _configuration;
        private IHttpContextAccessor _acc;
        public UserController(DataContext context, IConfiguration configuration, IHttpContextAccessor acc)
        {
            _configuration = configuration;
            _context = context;
            _acc = acc;
        }



        private void SendEmail(string recipientEmail, string emailSubject, string emailText)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("Mail:From").Value));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = emailSubject;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = emailText
            };
            using var smtp = new SmtpClient();
            smtp.Connect(
                _configuration.GetSection("Mail:Smtp").Value,
                int.Parse(_configuration.GetSection("Mail:Port").Value),
                SecureSocketOptions.StartTls
                );
            smtp.Authenticate(
                _configuration.GetSection("Mail:Username").Value,
                _configuration.GetSection("Mail:Password").Value
                );
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        [HttpPost("add-favorite"), Authorize]

        public async Task<IActionResult> AddToFavorite([FromBody]FavoritesVM favorite)
        {

            var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var favSadrzaj = new Favorites();
            if (!_context.Favorites.Any(f => f.IdSadrzaja == favorite.IdSadrzaja && f.Tip == favorite.Tip))
            {
                favSadrzaj = new Favorites
                {
                    IdSadrzaja = favorite.IdSadrzaja,
                    Tip = favorite.Tip
                };
                _context.Favorites.Add(favSadrzaj);
                await _context.SaveChangesAsync();
            }
            else
            {
                favSadrzaj = await _context.Favorites.Where(f => f.IdSadrzaja == favorite.IdSadrzaja && f.Tip == favorite.Tip).FirstOrDefaultAsync();
            }

            var favUser = new User_Favorites
            {
                UserId = userId,
                FavoritesId = favSadrzaj.Id,
            };
            _context.User_Favorites.Add(favUser);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Uspesno dodano" });

        }

        [HttpGet("get-favorites"), Authorize]

        public async Task<IActionResult> GetFavorites()
        {
            var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var sadrzaj = await _context.User_Favorites.Include(e => e.Favorites).ToListAsync();
            return Ok(sadrzaj);
        }


        [HttpDelete("delete-favorite"),Authorize]

        public async Task<IActionResult> DeleteFavorites(string Tip, int idSadrzaja)
        {
            var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var favSadrzaj = await _context.Favorites.Where(f => f.IdSadrzaja == idSadrzaja && f.Tip == Tip).FirstOrDefaultAsync();
            var rel = await _context.User_Favorites.Where(f => f.UserId == userId && f.FavoritesId == favSadrzaj.Id).FirstOrDefaultAsync();
            _context.User_Favorites.Remove(rel);
            _context.SaveChanges();


            return Ok(new {message = "Relation removed"});
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if(_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new {message = "User already exists." });
            }

            if(_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Username is taken" });
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User  
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PassswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken()
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var emailText = $"<h1>Welcome to Fuji</h1>" +
            $"<h3>Please click " +
                $"<a href=\"{_configuration.GetSection("ClientAppUrl").Value}/{user.VerificationToken}\">here</a>" +
                $" to confirm your account</h3>";
            SendEmail(user.Email, "Confirm your account", emailText);


            return Ok( new {message = "User seccesfully created!" });
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private string CreateRandomToken()
        {
            //da se proveri da l ovakav string vec postoji
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if(user == null)
            {
                return BadRequest(new {message = "User not found" });
            }

            if(!VerifyPasswordHash(request.Password, user.PasswordHash, user.PassswordSalt))
            {
                return BadRequest(new { message = "Something not right, try again" });
            }

            if(user.VerifiedAt == null)
            {
                return BadRequest(new { message = "Not verified" });
            }

            string token = CreateToken(user);

            return Ok(new
            {
                username = user.Username,
                message = $"welcome user: {user.Email}",
                token = token
            });
        }

        [HttpPost("change-password"), Authorize]

        public async Task<IActionResult> ChangePassword([FromBody]ChangePassword change)
        {
            var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            CreatePasswordHash(change.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PassswordSalt = passwordSalt;
            _context.SaveChanges();

            return Ok(new { message = "Password changed" });
        }


        [HttpDelete("delete-acc"), Authorize]

        public async Task<IActionResult> DeleteUser(string password)
        {
            var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PassswordSalt))
            {
                return BadRequest(new { message = "Something not right, try again" });
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok(new { message = "User removed" });
        }


        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyVM token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token.Token);
            if (user == null)
            {
                return BadRequest("Invalid token"); 
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new {message = "User Created"});
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest(new {message = "Email not found"});
            }

            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
             _context.SaveChanges();

            var emailText = $"<h1>Welcome to Fuji</h1>" +
            $"<h3>Please click " +
                $"<a href=\"{_configuration.GetSection("ClientAppUrl1").Value}/{user.PasswordResetToken}\">here</a>" +
                $" to reset your password</h3>";
            SendEmail(user.Email, "Confirm your account", emailText);

            return Ok(new {message = "Ok"});
        }

        [HttpGet("get-message"), Authorize]

        public async Task<IActionResult> GetMessage()
        {
            return Ok(new { message = "Bravo autorizovan si" });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest(new {message = "Invalid Token" });
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PassswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            await _context.SaveChangesAsync();

            return Ok(new {message = "Ok"});
        }


        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "Registered")
                //new Claim(ClaimTypes.Role, "Admin")

                //ovo drugo je za role i dodaje se isto
                //u weatherForecastController kod authorise da se stavlja koji role moze da pristupi
                /*new Claim(ClaimTypes.Role, "Admin")*///ovako se daje nekome admin
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
