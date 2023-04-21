using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using JwtAuth.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using System.Text;
using JwtAuth.Models;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly SqlConnection sqlConnection;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

       
        public static AuthUserModel UserModel { get; set; } = new AuthUserModel();

        public AccountController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            sqlConnection = new SqlConnection(configuration.GetConnectionString("Db"));

        }

        [HttpGet]
        public IActionResult Login(AuthUserModel model)
        {
            return View(model);
        }


        [HttpPost]
        public IActionResult Login()
        {


            string? UserName = Request.Form["UserName"];
            string? Password = Request.Form["Password"];

            Console.WriteLine("User " + UserName);
            //check if the employee exists in the database

            sqlConnection.Open();

            SqlCommand cmd = new SqlCommand("select * from Users where username=@Username", sqlConnection);
            cmd.Parameters.AddWithValue("@Username", UserName);

            SqlDataReader reader = cmd.ExecuteReader();

            //Generate jwt token for authentication if passwod matches and also import the necessary package
            if (reader.Read())
            {
                UserModel.UserName = UserName!;
                UserModel.Password = Password!;

                try
                {

                    List<Claim> claim = new List<Claim>() {
                    new Claim(ClaimTypes.Name, UserModel.UserName!),
                    new Claim(ClaimTypes.Sid,UserModel.UserId!.ToString()),
                    };

                    var token = GetToken(claim);

                    var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                    var expiration = token.ValidTo;

                    
                    reader.Close();
                    sqlConnection.Close();

                    // Redirect to Home/Index action
                    return RedirectToAction("Index", "Home");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error " + ex.Message);
                }
            }
             return Unauthorized(); 
        }

       
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
