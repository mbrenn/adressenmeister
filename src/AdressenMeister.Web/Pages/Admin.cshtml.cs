using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BurnSystems.Logging;
using DatenMeister.Core.EMOF.Interface.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    [Authorize(Roles = "Administrator")]
    public class Admin : PageModel
    {
        private ILogger logger = new ClassLogger(typeof(Admin));
        
        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public Admin(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }

        public IEnumerable<IElement> GetUsers()
        {
            return _adressenMeisterLogic.GetAllUsers();
        }

        public IActionResult OnGet()
        {
            // Just some security checks to be sure that the user is logged in
            if (!User.IsInRole("Administrator"))
            {
                return Redirect("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(string submit)
        {
            // Just some security checks to be sure that the user is logged in
            if (!User.IsInRole("Administrator"))
            {
                return Redirect("/Index");
            }

            var emails = new List<string>();
            foreach (var key in Request.Form.Keys.Where(x => x.StartsWith("email_")))
            {
                var emailAddress = key.Substring("email_".Length);
                emails.Add(emailAddress);   
            }

            // Delete User
            if (submit == "deleteuser")
            {
                foreach (var email in emails)
                {
                    _adressenMeisterLogic.DeleteUser(email);
                }
            }
            
            // Send Mail
            if (submit == "sendmail")
            {
                await _adressenMeisterLogic.SendEMails(emails, AdressenMeisterLogic.SecretValidityAdminSending);
            }

            return Page();
        }
    }
}