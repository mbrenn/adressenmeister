using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdressenMeister.Web.Models;
using BurnSystems.Logging;
using DatenMeister.Core.EMOF.Interface.Reflection;
using DatenMeister.Core.Helper;
using DatenMeister.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;

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

        public void OnGet()
        {
            
        }

        public async Task OnPost(string submit)
        {
            var templateStream = GetType().Assembly.GetManifestResourceStream(
                                     "AdressenMeister.Web.Embedded.mail.txt")
                                 ?? throw new InvalidOperationException("Mail Template not found");
            using var reader = new StreamReader(templateStream, Encoding.UTF8);
            var mailTemplate = await reader.ReadToEndAsync();

            var smtpLogic = new SmtpLogic();
            var smtpClient = smtpLogic.GetConnectedClient();
            foreach (var key in Request.Form.Keys.Where(x => x.StartsWith("email_")))
            {
                if (submit == "deleteuser")
                {
                    var email = key.Substring("email_".Length);
                    _adressenMeisterLogic.DeleteUser(email);
                }

                if (submit == "sendmail")
                {
                    var emailAddress = key.Substring("email_".Length);
                    var user = _adressenMeisterLogic.GetUserByEMail(emailAddress);
                    if ( user == null) continue;

                    var link =
                        HttpContext.Request.Scheme
                        + "://"
                        + HttpContext.Request.Host
                        + "/UserLogin/"
                        + emailAddress
                        + "/" 
                        + user.getOrDefault<string>(nameof(AdressenUser.secret));

                    var mailText = mailTemplate.Replace("{{Link}}", link);
                    
                    var bodyBuilder = new BodyBuilder
                    {
                        TextBody = mailText
                    };
                    
                    var email = new MimeMessage
                    {
                        Sender = smtpLogic.Sender,
                        Subject = "Abijahrgang 2001 - Deine Adressdaten",
                        Body = bodyBuilder.ToMessageBody()
                    };
                    
                    email.To.Add(MailboxAddress.Parse(emailAddress));
                    await smtpClient.SendAsync(email);
                    
                    logger.Info($"Sending mail to: {emailAddress}");
                }
            }
            
            await smtpClient.DisconnectAsync(true);
        }
    }
}