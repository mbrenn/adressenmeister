using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    public class UserLogin : PageModel
    {
        public class PostEmail
        {
            [BindProperty] public string? Email { get; set; } = string.Empty;
        }

        [BindProperty] public PostEmail PostModel { get; set; } = new ();
        

        public bool LoginInvalid { get; set; }
        
        public bool LoginInvalidExpired { get; set; }
        
        public bool LoginEmailInvalid { get; set; }

        public bool LoginAlreadySent { get; set; }

        public bool LoginSent { get; set; }


        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public UserLogin(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }
        
        public async Task<IActionResult> OnGet(string? email, string? secret)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(secret))
            {
                return Page();
            }

            switch(_adressenMeisterLogic.IsLoginValid(email, secret))
            {
                case AdressenMeisterLogic.LoginResult.Success:
                    break;
                case AdressenMeisterLogic.LoginResult.Wrong:
                    LoginInvalid = true;
                    return Page();
                case AdressenMeisterLogic.LoginResult.Expired:
                    LoginInvalidExpired = true;
                    return Page();
                default:
                    throw new InvalidOperationException();
            }

            // We got a successful login... at least, I hope so
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name,email),
                new("FullName", email),
                new(ClaimTypes.Role, "User"),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                RedirectUri = "/User",
            };

            if (HttpContext != null)
            {
                // Within NUnit, HttpContext might be null!
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }

            return Redirect("/User");
        }

        public async Task OnPost()
        {
            if (PostModel.Email != null && PostModel.Email.Contains("@") && PostModel.Email.Contains("."))
            {
                var result = await _adressenMeisterLogic.SendEMails(
                    new List<string> { PostModel.Email }, AdressenMeisterLogic.SecretValidityUserRequest);

                switch (result)
                {
                    case AdressenMeisterLogic.SendEMailResult.Success:
                        LoginSent = true;
                        break;
                    case AdressenMeisterLogic.SendEMailResult.AlreadySent:
                        LoginAlreadySent = true;
                        break;
                    case AdressenMeisterLogic.SendEMailResult.EMailNotKnown:
                        LoginEmailInvalid = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                LoginEmailInvalid = true;
            }
        }
    }
}