using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AdressenMeister.Web;
using AdressenMeister.Web.Models;
using DatenMeister.Core;
using DatenMeister.Core.Helper;
using DatenMeister.Integration.DotNet;
using NUnit.Framework;

namespace AdressenMeister.Tests
{
    [TestFixture]
    public class AdressenMeisterTests
    {

        /// <summary>
        /// Gets the integration settings
        /// </summary>
        /// <param name="dropDatabase">true, if the database shall be dropped</param>
        /// <returns>The created integration settings</returns>
        public static IntegrationSettings GetIntegrationSettings(bool dropDatabase = true)
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Get Directory Name failed"),
                "testing/datenmeister/data");
            var integrationSettings = new IntegrationSettings
            {
                DatabasePath = path,
                EstablishDataEnvironment = true,
                PerformSlimIntegration = false,
                AllowNoFailOfLoading = false,
                InitializeDefaultExtents = dropDatabase
            };

            return integrationSettings;
        }
        
        [Test]
        public void TestInitialize()
        {
            var dm = GiveMe.DatenMeister(GetIntegrationSettings());
            var adressenMeisterLogic = new AdressenMeisterLogic(dm.WorkspaceLogic, dm.ScopeStorage);
            
            Assert.That(adressenMeisterLogic.AdressenExtent, Is.Not.Null);
            Assert.That(adressenMeisterLogic.TypeUser, Is.Not.Null);
        }

        [Test]
        public void TestCreateGetAndGetAll()
        {
            var dm = GiveMe.DatenMeister(GetIntegrationSettings());
            var adressenMeisterLogic = new AdressenMeisterLogic(dm.WorkspaceLogic, dm.ScopeStorage);

            var user = adressenMeisterLogic.CreateUser();
            user.set(nameof(AdressenUser.email), "brenn@depon.net");
            user.set(nameof(AdressenUser.name), "Brenn");
            user.set(nameof(AdressenUser.prename), "Martin");
            
            var user2 = adressenMeisterLogic.CreateUser();
            user2.set(nameof(AdressenUser.email), "mbrenn@depon.net");
            user2.set(nameof(AdressenUser.name), "Brenner");
            user2.set(nameof(AdressenUser.prename), "Martiner");

            var users = adressenMeisterLogic.GetAllUsers().ToList();
            Assert.That(users.Count, Is.EqualTo(2));
            
            Assert.That(users.Any (x=>x.getOrDefault<string>(nameof(AdressenUser.name)) == "Brenn"), Is.True);
            Assert.That(users.Any (x=>x.getOrDefault<string>(nameof(AdressenUser.name)) == "Brenner"), Is.True);

            var found = adressenMeisterLogic.GetUserByEMail("brenn@depon.net");
            Assert.That(found,Is.Not.Null);
            Assert.That(found.getOrDefault<string>(nameof(AdressenUser.name)), Is.EqualTo("Brenn"));
            
            var found2 = adressenMeisterLogic.GetUserByEMail("brenn@depon.nett");
            Assert.That(found2, Is.Null);
        }
    }
}