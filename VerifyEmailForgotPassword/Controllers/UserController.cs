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
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace VerifyEmailForgotPassword.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private IConfiguration _configuration;
        private IHttpContextAccessor _acc;
        private Account account;
        private Cloudinary cloudinary;
        public UserController(DataContext context, IConfiguration configuration, IHttpContextAccessor acc)
        {
            _configuration = configuration;
            _context = context;
            _acc = acc;
            account = new Account(configuration.GetSection("Cloudinary:Cloud").Value,
                configuration.GetSection("Cloudinary:ApiKey").Value,
                configuration.GetSection("Cloudinary:ApiSecret").Value);
            cloudinary = new Cloudinary(account);
        }


        [HttpDelete("delete-image"),Authorize]

        public async Task<IActionResult> DeleteImage()
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if(user.PictureUrl != null)
                {
                user.PictureUrl = null;
                var profilePicturePublicId = $"{_configuration.GetSection("Cloudinary:ProfilePicsFolderName").Value}/user{userId}_profile-picture";
                var deletionParams = new DeletionParams(profilePicturePublicId)
                {
                    ResourceType = ResourceType.Image
                };
                cloudinary.Destroy(deletionParams);
                    _context.SaveChanges();
                    return Ok(new { message = "Picture delete succesfully" });
                }
                else
                {
                    return BadRequest(new { message = "You don't have a picture" });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add-image"),Authorize]

        public async Task<IActionResult> AddImage([FromForm] ImageVM image)
        {
            try
            {
                if (image.ProfilePicture == null)
                {
                    return BadRequest(new { message = "Prazan req" });
                }
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                var filePath = Path.GetTempFileName();

                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.ProfilePicture.CopyToAsync(stream);
                }
                var profilePicturePublicId = $"{_configuration.GetSection("Cloudinary:ProfilePicsFolderName").Value}/user{userId}_profile-picture";
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(filePath),
                    PublicId = profilePicturePublicId,
                };
                var uploadResult = cloudinary.Upload(uploadParams);
                user.PictureUrl = uploadResult.Url.ToString();
                _context.SaveChanges();
                return Ok(new { pictureUrl = uploadResult.Url });
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPatch("change-username"), Authorize]

        public async Task<IActionResult> ChangeUsernema(string newUsername)
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if(_context.Users.Any(u => u.Username == newUsername))
                {
                    return BadRequest(new { message = "Username already taken" });
                }
                else
                {
                user.Username = newUsername;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Usernmane changed succesfully" });
                }

            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("admin-delete"), Authorize]

        public async Task<IActionResult> AdminDelete(int id)
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var deletedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if(user.Role == "Admin")
                {
                    _context.Users.Remove(deletedUser);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "User delete succesfully" });
                }
                else
                {
                    return BadRequest(new { message = "You are not authorized for this aciton" });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("admin"), Authorize]

        public async Task<IActionResult> AdminTab()
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user.Role == "Admin")
                {
                    var userList = await _context.Users.Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.VerifiedAt
                    }).ToListAsync();
                    return Ok(userList);
                }
                else
                {
                    return BadRequest(new { message = "Nisi admin" });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("vote"), Authorize]

        public async Task<IActionResult> Vote([FromBody] VoteVM req)
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));

                if(_context.Votes.Any(v => v.UserId == userId && v.LinkId == req.LinkId && v.Vote == req.Vote))
                {
                    var pom = await _context.Votes.FirstOrDefaultAsync(v => v.UserId == userId && v.LinkId == req.LinkId && v.Vote == req.Vote);
                    _context.Votes.Remove(pom);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Vote deleted!" });
                }

                if (_context.Votes.Any(v => v.UserId == userId && v.LinkId == req.LinkId && v.Vote != req.Vote))
                {
                    var glas = await _context.Votes.FirstOrDefaultAsync(v => v.UserId == userId && v.LinkId == req.LinkId && v.Vote != req.Vote);
                    glas.Vote = req.Vote;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Vote changed!" });
                }

                var vote = new LinkVotes
                {
                    LinkId = req.LinkId,
                    UserId = userId,
                    Vote = req.Vote,
                    IdSadrzaja = req.idSadrzaja,
                    TipSadrzaja = req.tipsadrzaja
                };
                _context.Votes.Add(vote);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Vote added!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { ex.Message });
            }
        }


        [HttpGet("get-votes")]

        public async Task<IActionResult> GetVotes(int linkId)
        {
            try
            {
                var vote = await _context.Votes.Where(v => v.LinkId == linkId).ToListAsync();
                var positiveVotes = vote.Where(v => v.Vote == true).Count();
                var negativeVotes = vote.Where(v => v.Vote == false).Count();
                return Ok(new {votes = positiveVotes - negativeVotes});

            }
            catch (Exception ex)
            {

                return BadRequest(new { ex.Message });
            }
        }


        [HttpPost("add-link"), Authorize]

        public async Task<IActionResult> AddLink([FromBody] LinkVM links)
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));

                var link = new Links
                {
                    UserId = userId,
                    Link = links.Link,
                    IdSadrzaja = links.IdSadrzaja,
                    TipSadrzaja = links.TipSadrzaja,
                    Date = links.Date,
                };
                _context.Link.Add(link);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Link added succesfully" });
            }
            catch (Exception ex)
            {

                return BadRequest(new {message = ex.Message}); 
            }
        }

        [HttpGet("get-links")]

        public async Task<IActionResult> GetLinks(string tip, int idSadrzaja)
        {
            try
            {

                var links = await _context.Link.Where(c => c.IdSadrzaja == idSadrzaja && c.TipSadrzaja == tip).Include(l => l.Votes).Include(e => e.User).Select(u => new
                {
                    u.Id,
                    u.Link,
                    u.Date,
                    u.TipSadrzaja,
                    u.IdSadrzaja,
                    u.User.Username,
                    u.UserId,
                    u.Votes
                }).ToListAsync();
                return Ok(links);
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("edit-link"), Authorize]

        public async Task<IActionResult> EditLink([FromBody] LinkVM links)
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
                var link = await _context.Link.FirstOrDefaultAsync(c => c.UserId == userId && c.IdSadrzaja == links.IdSadrzaja && c.TipSadrzaja == links.TipSadrzaja);
                link.Link = links.Link;
                link.Date = links.Date;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Link edited succesfully!"});
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete-link"), Authorize]

        public async Task<IActionResult> DeleteLink(int linkId)
        {
            try
            {
                var link = await _context.Link.Where(c => c.Id == linkId).FirstOrDefaultAsync();
                var votes = await _context.Votes.Where(c => c.Id == link.Id).ToListAsync();
                _context.Votes.RemoveRange(votes);
                _context.Link.Remove(link);
                if (link == null)
                {
                    return BadRequest(new { message = "Link not found!" });
                }
                await _context.SaveChangesAsync();
                return Ok(new { message = "Link deleted succesfully!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add-comment"), Authorize]

        public async Task<IActionResult> AddComment([FromBody] CommentVM com)
        {
        var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));

            var comment = new Comment
            {
                UserId = userId,
                Komentar = com.Komentar,
                IdSadrzaja = com.IdSadrzaja,
                TipSadrzaja = com.TipSadrzaja,
                Date = com.Date,
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Comment added succesfully" });
        }

        [HttpDelete("delete-comment"), Authorize]

        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
            var comment = await _context.Comments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            _context.Comments.Remove(comment);
            if(comment == null)
            {
                return BadRequest(new {message="Comment not found!"});
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Comment deleted succesfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpPatch("edit-comment"), Authorize]

        public async Task<IActionResult> EditComment([FromBody]CommentVM com)
        {
            try
            {
                var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
                var comment = await _context.Comments.FirstOrDefaultAsync(c => c.UserId == userId && c.IdSadrzaja == com.IdSadrzaja && c.TipSadrzaja == com.TipSadrzaja);
                comment.Komentar = com.Komentar;
                comment.Date = com.Date;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Comment succesfully edited!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-comments")]

        public async Task<IActionResult> GetComments(string tip, int idSadrzaja)
        {
            try
            {
                var comments = await _context.Comments.Where(c => c.IdSadrzaja == idSadrzaja && c.TipSadrzaja == tip).Include(e => e.User).Select(u => new
                {
                    u.Id,
                    u.Komentar,
                    u.Date,
                    u.TipSadrzaja,
                    u.IdSadrzaja,
                    u.User.Username,
                    u.UserId
                }).ToListAsync();
                return Ok(comments);
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
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
            var sadrzaj = await _context.User_Favorites.Where(e => e.UserId == userId).Include(e => e.Favorites).ToListAsync();
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
                VerificationToken = CreateRandomToken(),
                Role = "Registered"
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

        [HttpGet("check-token"), Authorize]

        public async Task<IActionResult> CheckToken()
        {
            var userId = int.Parse(_acc.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return Ok(new { userId, user.Username, user.Role, user.Email, user.VerifiedAt, user.PictureUrl });
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
                email = user.Email,
                token = token,
                id=user.Id,
                role=user.Role,
                verifiedAt = user.VerifiedAt,
                pictureUrl = user.PictureUrl,
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


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
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
            SendEmail(user.Email, "Reset your Password", emailText);

            return Ok(new {message = "Ok"});
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
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
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateRandomToken()
        {
            //da se proveri da l ovakav string vec postoji
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
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
