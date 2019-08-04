using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Shared.Entities
{
    /// <summary>
    /// This class represents a User in a User Account Repo
    /// </summary>
    public class UserIdentityBE
    {
        // This is our (WP) identity for a user
        public string User { get; set; }

        // This is the filename of the KeyFile
        // In this conect this will be a RSA Public/Private Key Pair (2048 Bits)
        public string KeyFileName { get; set; }

        // This is the Thumbprint associated with the Keyfile
        // In this context we will use this as an "index" to find the correct certificate to use
        // FYI: This value is included in the Header (kid parameter) of the JWT (in this POC)
        public string KeyThumbprint { get; set; }

        // We use this to force specific Users to fail during testing
        public bool IsEnabled { get; set; }

        // This will be used as the subject in the JWT
        public string Company { get; set; }
    }
}
