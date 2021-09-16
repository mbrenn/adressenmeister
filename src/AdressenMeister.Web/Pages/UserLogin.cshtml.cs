using System.Collections.Generic;
using DatenMeister.Core.EMOF.Interface.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    public class UserLogin : PageModel
    {

        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public UserLogin(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }
        
        public void OnGet(string email, string key)
        {
            var x = 1;
        }
    }
}