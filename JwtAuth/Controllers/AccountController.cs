using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using JwtAuth.Models;
using Azure;
using Microsoft.IdentityModel.JsonWebTokens;

namespace JwtAuth.Controllers
{
   
    public class AccountController : Controller
    {

        private readonly SqlConnection sqlConnection;
        private readonly IConfiguration _configuration;

       
        public static AuthUserModel UserModel { get; set; } = new AuthUserModel();

        public AccountController(IConfiguration configuration)
        {
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

                    Console.WriteLine("Token "+ tokenValue+"\n Expire "+expiration);

                    Response.Headers.Append("Bearer",tokenValue);
                    UserModel.token = tokenValue;

                    Console.WriteLine("Login "+UserModel.token!);

                    reader.Close();
                    sqlConnection.Close();

                    // Redirect to Home/Index action
                    Console.WriteLine("User " + UserName);
                   
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
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(2),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Console.WriteLine("Logout "+UserModel.token!);


            Request.Headers.Clear();

            return RedirectToAction();
        }
    }
}
