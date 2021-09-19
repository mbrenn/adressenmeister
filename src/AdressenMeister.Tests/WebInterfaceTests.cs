using System.Threading.Tasks;
using AdressenMeister.Web;
using AdressenMeister.Web.Models;
using AdressenMeister.Web.Pages;
using DatenMeister.Core.Helper;
using DatenMeister.Integration.DotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NUnit.Framework;

namespace AdressenMeister.Tests
{
    [TestFixture]
    public class WebInterfaceTests
    {
        [Test]
        public async Task TestUserLogin()
        {
            var dm = GiveMe.DatenMeister(AdressenMeisterTests.GetIntegrationSettings());
            var adressenMeisterLogic = new AdressenMeisterLogic(dm.WorkspaceLogic, dm.ScopeStorage);
            var userLogin = new UserLogin(adressenMeisterLogic);


            var user = adressenMeisterLogic.CreateUser();
            user.set(nameof(AdressenUser.email), "brenn@depon.net");

            var secret = user.getOrDefault<string>(nameof(AdressenUser.secret));
            Assert.That(secret, Is.Not.Null.Or.Empty);

            var result = await userLogin.OnGet("none@depon.net", "abc");
            Assert.That(result as PageResult, Is.Not.Null);

            result = await userLogin.OnGet("brenn@depon.net", "abc");
            Assert.That(result as PageResult, Is.Not.Null);

            result = await userLogin.OnGet("brenn@depon.net", secret + "a");
            Assert.That(result as PageResult, Is.Not.Null);

            result = await userLogin.OnGet("brenn@depon.net", "a" + secret);
            Assert.That(result as PageResult, Is.Not.Null);

            result = await userLogin.OnGet("brenn@depon.net", string.Empty);
            Assert.That(result as PageResult, Is.Not.Null);

            result = await userLogin.OnGet("brenn@depon.net", secret);
            Assert.That(result as RedirectResult, Is.Not.Null);

            // Check, if login with empty secret is rejected
            user.set(nameof(AdressenUser.secret), "");
            result = await userLogin.OnGet("brenn@depon.net", string.Empty);
            Assert.That(result as PageResult, Is.Not.Null);
        }
    }
}