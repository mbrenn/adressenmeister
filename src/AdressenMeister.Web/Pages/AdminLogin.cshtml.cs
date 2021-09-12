using System.Collections.Generic;
using System.Security.Claims;
using DatenMeister.AdminUserSettings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    public class AdminLogin : PageModel
    {
        public void OnGet()
        {
        }

        public bool LoginFailed { get; set; } 

        public class PostModel
        {
            [BindProperty]
            public string User { get; set; }

            [BindProperty]
            public string Password { get; set; }
        }

        public async void OnPost(PostModel postModel)
        {
            var localAdmin = new LocalUserAdminSettings();
            if (localAdmin.IsPasswordCorrect(postModel.User, postModel.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,postModel.User),
                    new Claim("FullName", postModel.User),
                    new Claim(ClaimTypes.Role, "Administrator"),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    RedirectUri = "/Admin",
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }
            else
            {
                LoginFailed = true;
            }
        }
    }
}