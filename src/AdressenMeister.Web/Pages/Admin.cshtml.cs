using System.Collections.Generic;
using System.Linq;
using DatenMeister.Core.EMOF.Interface.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    [Authorize(Roles = "Administrator")]
    public class Admin : PageModel
    {
        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public Admin(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }

        public IEnumerable<IElement> GetUsers()
        {
            return _adressenMeisterLogic.GetAllUsers();
        }

        public void OnGet()
        {
            
        }

        public void OnPost(string submit)
        {
            foreach(var key in Request.Form.Keys.Where(x=>x.StartsWith("email_")))
            {
                if (submit == "deleteuser")
                {
                    var email = key.Substring("email_".Length);
                    _adressenMeisterLogic.DeleteUser(email);
                }
            }
        }
    }
}