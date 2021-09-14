using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{

    [Authorize(Roles = "Administrator")]
    public class AdminNew : PageModel
    {
        private readonly AdressenMeisterLogic _adressenMeisterLogic;

        public AdminNew(AdressenMeisterLogic adressenMeisterLogic)
        {
            _adressenMeisterLogic = adressenMeisterLogic;
        }
        
        public void OnGet()
        {

        }

        public class PostModel
        {
            [BindProperty] public string? emails { get; set; }
        }

        /// <summary>
        /// Stores the number of recently added emailaddresses
        /// </summary>
        public int EMailsAdded { get; set; } = -1;

        public void OnPost(PostModel postModel)
        {
            if (!string.IsNullOrEmpty(postModel.emails))
            {
                var result = _adressenMeisterLogic.AddUsersByEMails(postModel.emails);
                EMailsAdded = result.Count();
            }
        }
    }
}