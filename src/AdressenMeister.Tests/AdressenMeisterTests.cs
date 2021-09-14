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

        [Test]
        public void TestUsersByEmail()
        {
            var dm = GiveMe.DatenMeister(GetIntegrationSettings());
            var adressenMeisterLogic = new AdressenMeisterLogic(dm.WorkspaceLogic, dm.ScopeStorage);

            var foundBefore = adressenMeisterLogic.GetUserByEMail("brenn@depon.net");
            Assert.That(foundBefore, Is.Null);
            Assert.That(adressenMeisterLogic.AdressenExtent.elements().Count(), Is.EqualTo(0));

            var created = adressenMeisterLogic.AddUsersByEMails("brenn@depon.net;brenner@depon.net").ToList();
            Assert.That(created.Count, Is.EqualTo(2));
            Assert.That(adressenMeisterLogic.AdressenExtent.elements().Count(), Is.EqualTo(2));

            var found = adressenMeisterLogic.GetUserByEMail("brenn@depon.net");
            Assert.That(found, Is.Not.Null);

            var created2 = adressenMeisterLogic.AddUsersByEMails("brenn@depon.net;brenner@depon.net").ToList();
            Assert.That(created2.Count, Is.EqualTo(2));

            Assert.That(created2[0].getOrDefault<string>(nameof(AdressenUser.secret)), Is.Not.Null);
            Assert.That(created2[0].getOrDefault<string>(nameof(AdressenUser.secret)), Is.Not.EqualTo(string.Empty));

            found = adressenMeisterLogic.GetUserByEMail("brenn@depon.net");
            Assert.That(found, Is.Not.Null);

            Assert.That(adressenMeisterLogic.AdressenExtent.elements().Count(), Is.EqualTo(2));
        }

        [Test]
        public void TestGetPublicUsers()
        {
            var dm = GiveMe.DatenMeister(GetIntegrationSettings());
            var adressenMeisterLogic = new AdressenMeisterLogic(dm.WorkspaceLogic, dm.ScopeStorage);

            var created = adressenMeisterLogic.AddUsersByEMails("brenn@depon.net;noname@depon.net;noaddress@depon.net;nophone@depon.net").ToList();
            
            Assert.That(created.Count, Is.EqualTo(4));
            
            created[0].set(nameof(AdressenUser.name), "Brenn");
            created[0].set(nameof(AdressenUser.prename), "Martin");
            created[0].set(nameof(AdressenUser.street), "Straße 1");
            created[0].set(nameof(AdressenUser.zipcode), "55130");
            created[0].set(nameof(AdressenUser.city), "Mainz");
            created[0].set(nameof(AdressenUser.country), "Germany");
            created[0].set(nameof(AdressenUser.phone), "0123");
            created[0].set(nameof(AdressenUser.isNameVisible), true);
            created[0].set(nameof(AdressenUser.isPhoneVisible), true);
            created[0].set(nameof(AdressenUser.isAddressVisible), true);
            
            created[1].set(nameof(AdressenUser.name), "Brenner");
            created[1].set(nameof(AdressenUser.prename), "Martiner");
            created[1].set(nameof(AdressenUser.street), "Straße 2");
            created[1].set(nameof(AdressenUser.zipcode), "55131");
            created[1].set(nameof(AdressenUser.city), "Mainzer");
            created[1].set(nameof(AdressenUser.country), "Germany");
            created[1].set(nameof(AdressenUser.phone), "0234");
            created[1].set(nameof(AdressenUser.isNameVisible), false);
            created[1].set(nameof(AdressenUser.isPhoneVisible), true);
            created[1].set(nameof(AdressenUser.isAddressVisible), true);
            
            created[2].set(nameof(AdressenUser.name), "Brenni");
            created[2].set(nameof(AdressenUser.prename), "Martini");
            created[2].set(nameof(AdressenUser.street), "Straße 3");
            created[2].set(nameof(AdressenUser.zipcode), "55132");
            created[2].set(nameof(AdressenUser.city), "Nicht Wiesbaden");
            created[2].set(nameof(AdressenUser.country), "Germany");
            created[2].set(nameof(AdressenUser.phone), "03456");
            created[2].set(nameof(AdressenUser.isNameVisible), true);
            created[2].set(nameof(AdressenUser.isPhoneVisible), true);
            created[2].set(nameof(AdressenUser.isAddressVisible), false);
            
            created[3].set(nameof(AdressenUser.name), "Brenna");
            created[3].set(nameof(AdressenUser.prename), "Martina");
            created[3].set(nameof(AdressenUser.street), "Straße 4");
            created[3].set(nameof(AdressenUser.zipcode), "55133");
            created[3].set(nameof(AdressenUser.city), "Mainza");
            created[3].set(nameof(AdressenUser.country), "Germany");
            created[3].set(nameof(AdressenUser.phone), "04567");
            created[3].set(nameof(AdressenUser.isNameVisible), true);
            created[3].set(nameof(AdressenUser.isPhoneVisible), false);
            created[3].set(nameof(AdressenUser.isAddressVisible), true);

            var users = adressenMeisterLogic.GetPublicDataOfAllUsers().ToList();
            var brenn = users.FirstOrDefault(x=>x.getOrDefault<string>(nameof(AdressenUser.email))=="brenn@depon.net");
            Assert.That(brenn, Is.Not.Null);
            Assert.That(brenn.getOrDefault<string>(nameof(AdressenUser.name)), Is.EqualTo("Brenn"));
            Assert.That(brenn.getOrDefault<string>(nameof(AdressenUser.prename)), Is.EqualTo("Martin"));
            Assert.That(brenn.getOrDefault<string>(nameof(AdressenUser.street)), Is.EqualTo("Straße 1"));
            Assert.That(brenn.getOrDefault<string>(nameof(AdressenUser.zipcode)), Is.EqualTo("55130"));
            Assert.That(brenn.getOrDefault<string>(nameof(AdressenUser.city)), Is.EqualTo("Mainz"));
            Assert.That(brenn.getOrDefault<string>(nameof(AdressenUser.country)), Is.EqualTo("Germany"));
            Assert.That(brenn.getOrDefault<string>(nameof(AdressenUser.phone)), Is.EqualTo("0123"));
            Assert.That(brenn.getOrDefault<bool>(nameof(AdressenUser.isNameVisible)), Is.EqualTo(true));
            Assert.That(brenn.getOrDefault<bool>(nameof(AdressenUser.isPhoneVisible)), Is.EqualTo(true));
            Assert.That(brenn.getOrDefault<bool>(nameof(AdressenUser.isAddressVisible)), Is.EqualTo(true));
            
            var noName = users.FirstOrDefault(x=>x.getOrDefault<string>(nameof(AdressenUser.email))=="noname@depon.net");
            Assert.That(noName, Is.Null);
            
            var brenni = users.FirstOrDefault(x=>x.getOrDefault<string>(nameof(AdressenUser.email))=="noaddress@depon.net");
            Assert.That(brenni, Is.Not.Null);
            Assert.That(brenni.getOrDefault<string>(nameof(AdressenUser.name)), Is.EqualTo("Brenni"));
            Assert.That(brenni.getOrDefault<string>(nameof(AdressenUser.prename)), Is.EqualTo("Martini"));
            Assert.That(brenni.getOrDefault<string>(nameof(AdressenUser.street)), Is.Null.Or.Empty);
            Assert.That(brenni.getOrDefault<string>(nameof(AdressenUser.zipcode)), Is.Null.Or.Empty);
            Assert.That(brenni.getOrDefault<string>(nameof(AdressenUser.city)), Is.Null.Or.Empty);
            Assert.That(brenni.getOrDefault<string>(nameof(AdressenUser.country)), Is.Null.Or.Empty);
            Assert.That(brenni.getOrDefault<string>(nameof(AdressenUser.phone)), Is.EqualTo("03456"));
            Assert.That(brenni.getOrDefault<bool>(nameof(AdressenUser.isNameVisible)), Is.EqualTo(true));
            Assert.That(brenni.getOrDefault<bool>(nameof(AdressenUser.isPhoneVisible)), Is.EqualTo(true));
            Assert.That(brenni.getOrDefault<bool>(nameof(AdressenUser.isAddressVisible)), Is.EqualTo(false));
            
            var brenna = users.FirstOrDefault(x=>x.getOrDefault<string>(nameof(AdressenUser.email))=="nophone@depon.net");
            Assert.That(brenna, Is.Not.Null);
            Assert.That(brenna.getOrDefault<string>(nameof(AdressenUser.name)), Is.EqualTo("Brenna"));
            Assert.That(brenna.getOrDefault<string>(nameof(AdressenUser.prename)), Is.EqualTo("Martina"));
            Assert.That(brenna.getOrDefault<string>(nameof(AdressenUser.street)), Is.EqualTo("Straße 4"));
            Assert.That(brenna.getOrDefault<string>(nameof(AdressenUser.zipcode)), Is.EqualTo("55133"));
            Assert.That(brenna.getOrDefault<string>(nameof(AdressenUser.city)), Is.EqualTo("Mainza"));
            Assert.That(brenna.getOrDefault<string>(nameof(AdressenUser.country)), Is.EqualTo("Germany"));
            Assert.That(brenna.getOrDefault<string>(nameof(AdressenUser.phone)), Is.Null.Or.Empty);
            Assert.That(brenna.getOrDefault<bool>(nameof(AdressenUser.isNameVisible)), Is.EqualTo(true));
            Assert.That(brenna.getOrDefault<bool>(nameof(AdressenUser.isPhoneVisible)), Is.EqualTo(false));
            Assert.That(brenna.getOrDefault<bool>(nameof(AdressenUser.isAddressVisible)), Is.EqualTo(true));
        }
    }
}