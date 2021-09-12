﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdressenMeister.Web.Pages
{
    [Authorize(Roles = "User")]
    public class UserChange : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}