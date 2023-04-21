using CheckAuthentication.Data;
using CheckAuthentication.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CheckAuthentication.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration configuration;
        private readonly SqlConnection sqlConnection;
        private readonly IServiceCollection services;
        public static AuthUserModel UserModel { get; set; } = new AuthUserModel();

        public AccountController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            this.configuration = configuration;
            sqlConnection = new SqlConnection(configuration.GetConnectionString("Db"));

        }

        [HttpGet]
        public IActionResult Login(AuthUserModel model)
        {
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Login()
        {


            if (!User.Identity.IsAuthenticated)
            {
                // If the user is already authenticated, redirect them to the Home/Index action
                ViewData["Message"] = "You must be authenticated to access this page.";
            }



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

                
                    var identity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


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
            Console.WriteLine("12");
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
