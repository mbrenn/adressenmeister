using System;
using System.IO;
using System.Reflection;
using AdressenMeister.Web;
using DatenMeister.Core;
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
    }
}