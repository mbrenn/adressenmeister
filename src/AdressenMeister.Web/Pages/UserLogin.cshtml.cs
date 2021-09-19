using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using AdressenMeister.Web.Models;
using DatenMeister.Core.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace AdressenMeister.Web.Pages
{
    public class UserLogin : PageModel
    {

        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public UserLogin(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }
        
        public async Task<IActionResult> OnGet(string email, string secret)
        {
            
            var user = _adressenMeisterLogic.GetUserByEMail(email);
            if (user == null)
            {
                return Page();
            }

            if (String.Compare(
                user.getOrDefault<string>(nameof(AdressenUser.secret)),
                secret,
                StringComparison.Ordinal) != 0
                || string.IsNullOrEmpty(secret))
            {
                return Page();
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
    }
}