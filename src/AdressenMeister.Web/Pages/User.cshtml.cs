using System.Collections.Generic;
using System.Security.Claims;
using DatenMeister.Core.EMOF.Interface.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    [Authorize(Roles = "User")]
    public class User : PageModel
    {
        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public User(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }
        
        public IEnumerable<IElement> GetUsers()
        {
            return _adressenMeisterLogic.GetPublicDataOfAllUsers();
        }
        public IActionResult OnGet()
        {
            // Just some security checks to be sure that the user is logged in
            var userName = User.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Redirect("/Index");
            }

            var userData = _adressenMeisterLogic.GetUserByEMail(userName);
            if (userData == null)
            {
                return Redirect("/Index");
            }

            return Page();
        }
    }
}