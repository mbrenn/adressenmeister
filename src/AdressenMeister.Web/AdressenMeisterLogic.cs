using System;
using System.Collections.Generic;
using System.Linq;
using AdressenMeister.Web.Models;
using DatenMeister.Core;
using DatenMeister.Core.EMOF.Implementation;
using DatenMeister.Core.EMOF.Interface.Identifiers;
using DatenMeister.Core.EMOF.Interface.Reflection;
using DatenMeister.Core.Functions.Queries;
using DatenMeister.Core.Runtime.Workspaces;
using DatenMeister.Extent.Manager.ExtentStorage;
using DatenMeister.Types;

namespace AdressenMeister.Web
{
    public class AdressenMeisterLogic
    {
        private readonly IWorkspaceLogic _workspaceLogic;
        private readonly IScopeStorage _scopeStorage;

        public const string contextUrl = "dm:///adressen/";
        
        public IUriExtent? AdressenExtent { get; private set; }
        
        public IElement? TypeUser { get; private set; }

        public AdressenMeisterLogic(IWorkspaceLogic workspaceLogic, IScopeStorage scopeStorage)
        {
            _workspaceLogic = workspaceLogic;
            _scopeStorage = scopeStorage;
            
            Initialize();
        }

        public void Initialize()
        {
            AdressenExtent = _workspaceLogic.FindExtent(contextUrl); 
            
            if (AdressenExtent == null)
            {
                var extentManager = new ExtentManager(_workspaceLogic, _scopeStorage);
                var loaded = extentManager.CreateAndAddXmiExtent(
                    contextUrl,
                    "adressen.xmi");
                if (loaded.LoadingState == ExtentLoadingState.Loaded)
                {
                    AdressenExtent = loaded.Extent;
                }
                
                extentManager.StoreAllExtents();
            }

            var localTypeManager = new LocalTypeSupport(_workspaceLogic, _scopeStorage);
            TypeUser = localTypeManager.AddInternalType("AdressenMeister", typeof(AdressenUser));
        }

        public IElement CreateUser(AdressenUser? user)
        {
            if (AdressenExtent == null) throw new InvalidOperationException();
            
            var factory = new MofFactory(AdressenExtent);
            var createdUser = factory.create(TypeUser);

            if (user != null)
            {
                createdUser.set(nameof(AdressenUser.name), user.name);
                createdUser.set(nameof(AdressenUser.prename), user.prename);
                createdUser.set(nameof(AdressenUser.street), user.street);
                createdUser.set(nameof(AdressenUser.zipcode), user.zipcode);
                createdUser.set(nameof(AdressenUser.city), user.city);
                createdUser.set(nameof(AdressenUser.country), user.country);
                createdUser.set(nameof(AdressenUser.phone), user.phone);
                createdUser.set(nameof(AdressenUser.email), user.email);
                createdUser.set(nameof(AdressenUser.isNameVisible), user.isNameVisible);
                createdUser.set(nameof(AdressenUser.isAddressVisible), user.isAddressVisible);
                createdUser.set(nameof(AdressenUser.isPhoneVisible), user.isPhoneVisible);
            }

            return createdUser;
        }

        public IEnumerable<IElement> GetAllUsers()
        {
            if (AdressenExtent == null) throw new InvalidOperationException();
            if (TypeUser == null) throw new InvalidOperationException();

            return AdressenExtent.elements()
                .GetAllDescendantsIncludingThemselves()
                .WhenMetaClassIs(TypeUser)
                .OfType<IElement>();
        }

        /// <summary>
        /// Gets a certain user by the email address or null, if the user is not found
        /// </summary>
        /// <param name="email">Email to be checked</param>
        /// <returns>The found user or null, if not found</returns>
        public IElement? GetUserByEMail(string email)
        {
            if (AdressenExtent == null) throw new InvalidOperationException();
            if (TypeUser == null) throw new InvalidOperationException();

            return AdressenExtent.elements()
                .GetAllDescendantsIncludingThemselves()
                .WhenMetaClassIs(TypeUser)
                .WhenPropertyHasValue(nameof(AdressenUser.email), email)
                .OfType<IElement>()
                .FirstOrDefault();
        }
    }
}