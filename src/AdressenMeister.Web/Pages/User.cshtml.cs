using System.Collections.Generic;
using DatenMeister.Core.EMOF.Interface.Reflection;
using Microsoft.AspNetCore.Authorization;
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
        public void OnGet()
        {
            
        }
    }
}