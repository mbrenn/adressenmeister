using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    [Authorize(Roles = "Administrator")]
    public class Admin : PageModel
    {
        public void OnGet()
        {   
        }
    }
}