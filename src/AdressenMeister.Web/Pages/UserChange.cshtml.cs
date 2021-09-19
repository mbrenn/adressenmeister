using System.Security.Claims;
using AdressenMeister.Web.Models;
using DatenMeister.Core.EMOF.Implementation.DotNet;
using DatenMeister.Core.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    [Authorize(Roles = "User")]
    public class UserChange : PageModel
    {

        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public UserChange(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }
        
        public bool Success = false;

        [BindProperty] public AdressenUser UserData { get; set; } = new();
        
        public IActionResult OnGet()
        {
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


            UserData = DotNetConverter.ConvertToDotNetObject<AdressenUser>(userData);

            return Page();
        }

        public IActionResult OnPost()
        {
            var userName = User.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Redirect("/Index");
            }
            
            _adressenMeisterLogic.SetUserData(userName, UserData);
            Success = true;

            return Page();
        }
    }
}