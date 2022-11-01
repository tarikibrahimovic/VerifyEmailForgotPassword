using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using VerifyEmailForgotPassword.Data.ViewModel;

namespace VerifyEmailForgotPassword.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private IConfiguration _configuration;
        public UserController(DataContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        //[HttpPost("send-mail")]
        //public void SendEmail(string user)
        //{
        //    var email = new MimeMessage();
        //    email.From.Add(MailboxAddress.Parse(_configuration.GetSection("Mail:From").Value));
        //    email.To.Add(MailboxAddress.Parse(_configuration.GetSection("Mail:From").Value));
        //    email.Subject = "Test Email Subject";
        //    var mailText = $"<a href=\"{_configuration.GetSection("ClientAppUrl").Value}/{user}\">here</a>";
        //    email.Body = new TextPart(TextFormat.Html) { Text = mailText };

        //    using var smtp = new SmtpClient();
        //    smtp.Connect("smtp.ethereal.email",
        //                 587,
        //                 SecureSocketOptions.StartTls);
        //    //smtp.gmail.com ako se na gmail salje
        //    smtp.Authenticate("rickey.kohler97@ethereal.email", "mEedpuwQb3rfbVfkSt");
        //    smtp.Send(email);
        //    smtp.Disconnect(true);
        //}



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

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if(_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new {message = "User already exists." });
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User  
            {
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
            SendEmail("tarikibrahimovic2016@gmail.com", "Confirm your account", emailText);


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
                return BadRequest(new {message = "user not found" });
            }

            if(!VerifyPasswordHash(request.Password, user.PasswordHash, user.PassswordSalt))
            {
                return BadRequest(new { message = "password incorect" });
            }

            if(user.VerifiedAt == null)
            {
                return BadRequest(new { message = "not verified" });
            }

            string token = CreateToken(user);

            return Ok(new {
                message = $"welcome user: {user.Email}",
                token = token
            });
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

            return Ok("User verified!");
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
                return BadRequest("User not found");
            }

            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            return Ok("You may reset your password");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid Token");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PassswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            await _context.SaveChangesAsync();

            return Ok("You may reset your password");
        }


        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin")

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
