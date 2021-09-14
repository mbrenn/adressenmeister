using System;
using System.Collections.Generic;
using System.Linq;
using AdressenMeister.Web.Models;
using BurnSystems;
using DatenMeister.Core;
using DatenMeister.Core.EMOF.Implementation;
using DatenMeister.Core.EMOF.Interface.Identifiers;
using DatenMeister.Core.EMOF.Interface.Reflection;
using DatenMeister.Core.Functions.Queries;
using DatenMeister.Core.Helper;
using DatenMeister.Core.Runtime.Copier;
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

        public IElement CreateUser(AdressenUser? user = null)
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

            AdressenExtent.elements().add(createdUser);

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

        /// <summary>
        /// Adds a series of users by their email addresses. If there is already a user with that e-mailaddress,
        /// the existing user is returned
        /// </summary>
        /// <param name="emailConcat">Semicolon-Separated list of email-Addresses</param>
        /// <returns>Enumeration of elements of new or existing users</returns>
        public IEnumerable<IElement> AddUsersByEMails(string emailConcat)
        {
            var emails = emailConcat.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var email in emails)
            {
                var created = AddUsersByEMail(email);
                if (created != null)
                {
                    yield return created;
                }
            }
        }

        /// <summary>
        /// Adds a new user by the email
        /// </summary>
        /// <param name="email">E-Mail to be added</param>
        /// <returns>The already existing or newly created email-address</returns>
        private IElement? AddUsersByEMail(string email)
        {
            if (AdressenExtent == null) throw new InvalidOperationException();
            if (!email.Contains('@'))
            {
                return null;
            }

            var found = AdressenExtent.elements()
                .GetAllDescendantsIncludingThemselves()
                .WhenMetaClassIs(TypeUser)
                .WhenPropertyHasValue(nameof(AdressenUser.email), email)
                .OfType<IElement>()
                .FirstOrDefault();

            if (found != null)
            {
                return found;
            }

            found = CreateUser();
            found.set(nameof(AdressenUser.email), email);
            found.set(nameof(AdressenUser.secret), StringManipulation.SecureRandomString(32));

            return found;
        }

        /// <summary>
        /// Gets the complete data of all users and honors the visibility setting of each user.
        /// </summary>
        /// <returns>Enumeration of all visible data</returns>
        public IEnumerable<IElement> GetPublicDataOfAllUsers()
        {
            if (AdressenExtent == null) throw new InvalidOperationException();
            
            var users = AdressenExtent.elements()
                .GetAllDescendantsIncludingThemselves()
                .WhenMetaClassIs(TypeUser)
                .OfType<IElement>();

            foreach (var user in users)
            {
                var copy = ObjectCopier.CopyForTemporary(user) as IElement
                    ?? throw new InvalidOperationException("The copying has failed");

                if (!copy.getOrDefault<bool>(nameof(AdressenUser.isNameVisible)))
                {
                    continue;
                }

                if (!copy.getOrDefault<bool>(nameof(AdressenUser.isAddressVisible)))
                {
                    copy.set(nameof(AdressenUser.street), string.Empty);
                    copy.set(nameof(AdressenUser.zipcode), string.Empty);
                    copy.set(nameof(AdressenUser.city), string.Empty);
                    copy.set(nameof(AdressenUser.country), string.Empty);
                }

                if (!copy.getOrDefault<bool>(nameof(AdressenUser.isPhoneVisible)))
                {
                    copy.set(nameof(AdressenUser.phone), string.Empty);
                }

                yield return copy;
            }
        }
        
        /// <summary>
        /// Deletes a user by email
        /// </summary>
        /// <param name="email">E-Mail to be used</param>
        /// <returns>true, if the user is deleted</returns>
        public bool DeleteUser(string email)
        {
            if (AdressenExtent == null) throw new InvalidOperationException();
            
            var found = AdressenExtent.elements()
                .GetAllDescendantsIncludingThemselves()
                .WhenMetaClassIs(TypeUser)
                .WhenPropertyHasValue(nameof(AdressenUser.email), email)
                .OfType<IElement>()
                .FirstOrDefault();

            if (found != null)
            {
                return AdressenExtent.elements().remove(found);
            }

            return false;
        }
    }
}