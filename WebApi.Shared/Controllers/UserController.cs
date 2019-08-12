using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WebApi.Shared.Entities;

namespace WebApi.Shared.Controllers
{
    /// <summary>
    /// This class represents the interface to a user repository
    /// </summary>
    public static class UserController
    {
        public static UserIdentityBE GetUserInfo(string userName)
        {
            // load user store
            var userXref = UserController.LoadUsers();

            // find the info about the expected user
            var userIdentity = userXref.Where(u => u.User.ToLower() == userName.ToLower()).FirstOrDefault();

            if (userIdentity == null)
            {
                throw new ApplicationException($"UserName [{userName}] is not valid!");
            }

            return userIdentity;
        }

        public static UserIdentityBE GetUserWithClientID(string clientID)
        {
            // load user store
            var userXref = UserController.LoadUsers();

            // find the info about the expected user
            var userIdentity = userXref.Where(u => u.ClientID.ToLower() == clientID.ToLower()).FirstOrDefault();

            return userIdentity;
        }

        [Obsolete("Migrate over to use GetUserWithClientID")]
        public static UserIdentityBE GetUserWithKeyThumbprint(string signingKeyThumbprint)
        {
            // load user store
            var userXref = UserController.LoadUsers();

            // find the info about the expected user
            var userIdentity = userXref.Where(u => u.KeyThumbprint.ToLower() == signingKeyThumbprint.ToLower()).FirstOrDefault();

            return userIdentity;
        }

        /// <summary>Loads the user store.</summary>
        /// <returns>List&lt;UserIdentityBE&gt;.</returns>
        /// <remarks>
        /// This is a list of keys we manually generated to support testing
        /// </remarks>
        public static List<UserIdentityBE> LoadUsers()
        {
            List<UserIdentityBE> users = new List<UserIdentityBE>()
            {
                new UserIdentityBE() {User = @"UserA", KeyFileName = @"UserA_SelfSignedCertificate.pfx", KeyThumbprint = @"1de2df466d2188d32f7487b29bc769115dc53472", IsEnabled = true, Company = @"https://www.datacapsystems.com/", ClientID = @"e0ba2665-7056-4210-8c71-2140150ebf77" },
                new UserIdentityBE() {User = @"UserB", KeyFileName = @"UserB_SelfSignedCertificate.pfx", KeyThumbprint = @"af929f265e746f9a28ec5e6afbec13455597bcc0", IsEnabled = true, Company = @"https://www.someothercompany.com/", ClientID = @"7a6741c0-e032-4253-84d4-732b8338fbdb" },
                // Note we will force user 3 to always fail "authentication"
                new UserIdentityBE() {User = @"UserC", KeyFileName = @"UserC_SelfSignedCertificate.pfx", KeyThumbprint = @"db72108d92b8e51bcdbadc65dd20d3376924b47f", IsEnabled = false, Company = @"https://www.somedisabledcompany.com/", ClientID = @"4d9bc2f9-3bf8-43a6-b11d-dcfab97756d6" },
            };

            return users;
        }
    }
}
