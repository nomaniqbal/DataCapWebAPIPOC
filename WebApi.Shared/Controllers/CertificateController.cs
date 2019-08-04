using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WebApi.Shared.Controllers
{
    /// <summary>
    /// This class represents the interface to a certificate repository
    /// </summary>
    /// <remarks>
    ///   load certificate from a file (useful for testing, not suitable for production use)
    ///   load certificate from a cert store (prefered for production)
    /// </remarks>
    public static class CertificateController
    {
        const string CERT_FOLDER_PATHNAME = @"C:\Users\xtobr\Source\Repos\Worldpay\DataCapWebAPIPOC\ClientCertificates";
        const string CERT_PWD_FILENAME = @"Password.txt";

        /// <summary>Gets the certificate with thumbprint.</summary>
        /// <param name="signingKeyThumbprint">The signing key thumbprint.</param>
        /// <returns>X509Certificate2.</returns>
        /// <exception cref="T:System.ApplicationException">SigningKeyThumbprint [{signingKeyThumbprint}</exception>
        internal static X509Certificate2 GetCertificateWithThumbprint(string signingKeyThumbprint)
        {
            var userInfo = UserController.GetUserWithKeyThumbprint(signingKeyThumbprint);

            var certificate = LoadCertFromFile(userInfo.KeyFileName, CERT_PWD_FILENAME, signingKeyThumbprint);

            return certificate;
        }

        #region ========================= Private Helper methods =========================
        /// <summary>Gets the cert by thumbprint from a cert store.</summary>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>X509Certificate2.</returns>
        private static X509Certificate2 LoadCertByThumbprint(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            X509Store store = null;
            X509Certificate2 cert = null;

            try
            {
                // define which store holds the certificate
                store = new X509Store(storeName, storeLocation);

                // open the cert store
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

                // retrieve the cert
                cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)[0];

            }
            finally
            {
                if (store != null) store.Close();
            }

            return cert;
        }

        /// <summary>Gets the cert from file and valdiaet it against an expected thumbprint.</summary>
        /// <param name="certFilePathName">Name of the cert file path.</param>
        /// <param name="certPwdFilePathName">Name of the cert password file path.</param>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>X509Certificate2.</returns>
        /// <exception cref="ArgumentException">You must supply a value for the thumbprint - thumbprint</exception>
        /// <exception cref="ApplicationException">The actual thumbprint [{certificate.Thumbprint}] does not match the expected thumbprint [{thumbprint}</exception>
        private static X509Certificate2 LoadCertFromFile(string certFileName, string certPwdFileName, string thumbprint)
        {
            // if we are supposed to validate the cert, make sure a thumbprint was passed in
            if (string.IsNullOrEmpty(thumbprint))
            {
                throw new ArgumentException($"You must supply a value for the thumbprint", @"thumbprint");
            }

            // load the cert pwd from the file
            var certificate = LoadCertFromFile(certFileName, certPwdFileName); ;

            // validate the thumbprint
            if (certificate.Thumbprint.ToLower() != thumbprint.ToLower())
            {
                throw new ApplicationException($"The actual thumbprint [{certificate.Thumbprint}] does not match the expected thumbprint [{thumbprint}]");
            }

            return certificate;
        }

        /// <summary>Gets the cert from file.</summary>
        /// <param name="certFilePathName">Name of the cert file path.</param>
        /// <param name="certPwdFilePathName">Name of the cert password file path.</param>
        /// <returns>X509Certificate2.</returns>
        private static X509Certificate2 LoadCertFromFile(string certFileName, string certPwdFileName)
        {
            // build the paths
            string certFilePathName = System.IO.Path.Combine(CERT_FOLDER_PATHNAME, certFileName);
            string certPwdFilePathName = System.IO.Path.Combine(CERT_FOLDER_PATHNAME, certPwdFileName);

            // load the cert's pwd from the file
            string certPassword = System.IO.File.ReadAllText(certPwdFilePathName);

            // load the cert from the file
            X509Certificate2 certificate = new X509Certificate2(certFilePathName, certPassword);

            return certificate;
        }

        #endregion
    }
}
