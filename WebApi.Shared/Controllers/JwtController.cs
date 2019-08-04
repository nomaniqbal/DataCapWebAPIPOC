using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using WebApi.Shared.Entities;
using WebApi.Shared.Utilites;

namespace WebApi.Shared.Controllers
{
    /// <summary>
    /// This class contains logic to support JWT Tokens.  
    /// </summary>
    /// <remarks>
    /// The party being called (in this case WP) would create and supply the key to the caller.
    /// The caller uses the Private part of an asymentric key to sign the JWT header.
    /// If the party being called (who has the public key) can sucessfully validate the JWT token
    /// we an use that as evidence of who the caller is (ie authentication)
    /// 
    /// There are two (2) choices for signing...
    ///     Both choices refer to what algorithm the identity provider uses to sign the JWT. Signing is a cryptographic 
    ///     operation that generates a "signature" (part of the JWT) that the recipient of the token can validate 
    ///     to ensure that the token has not been tampered with.
    /// 
    /// RS256 (RSA Signature with SHA-256) is an asymmetric algorithm, and it uses a public/private key pair: 
    ///     the identity provider has a private (secret) key used to generate the signature, and the consumer 
    ///     of the JWT gets a public key to validate the signature. Since the public key, as opposed to the 
    ///     private key, doesn't need to be kept secured, most identity providers make it easily available 
    ///     for consumers to obtain and use (usually through a metadata URL).
    ///
    /// HS256(HMAC with SHA-256), on the other hand, involves a combination of a hashing function and one(secret) 
    ///     key that is shared between the two parties used to generate the hash that will serve as the signature.
    ///     Since the same key is used both to generate the signature and to validate it, care must be taken to 
    ///     ensure that the key is not compromised.
    /// </remarks>
    public static class JwtController
    {
        // uniquely identifies the party this JWT carries information about (used in JWT Token)
        public static readonly string AUDIENCE = @"https://www.worldpay.com/";

        /// <summary>Creates the JWT token.</summary>
        /// <param name="userName"></param>
        /// <param name="subject">The subject.</param>
        /// <param name="audience">The audience.</param>
        /// <returns>System.String.</returns>
        /// <remarks>This would be used on the client side</remarks>
        public static string CreateJWTToken(string userName, string subject, string audience)
        {
            var userInfo = UserController.GetUserInfo(userName);

            // Load the Certificate
            var certificate = CertificateController.GetCertificateWithThumbprint(userInfo.KeyThumbprint);

            // Represents the cryptographic key and security algorithms that are used to generate a digital signature.
            var credentials = new SigningCredentials(new X509SecurityKey(certificate), "RS256");

            // Create a header class that will use the supplied credentials
            var header = new JwtHeader(credentials);

            // Define the JWT Payload
            //  Note: the specifc content required for this use case (Purchase Auth) needs to be determined
            var payload = new JwtPayload
            {
               { "sub", subject },
               { "aud", audience },
               { "exp", DateTime.UtcNow.AddMinutes(15).ToEpochSeconds() },
            };

            // create the JWT
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            // convert the Token to a String
            var tokenString = handler.WriteToken(secToken);

            /*
                Sample JWT generated             
                eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NDk0MjEwN30.mwwaFczSPP1_EMDcgAdXIbf3hwHw26nTv-kG4b1_EH9q8TFrNMmPMjayyWzHDizbwF-As-6AppaNlMbEQFp-ilXLCx_MAgvff1vNA_qA_wh_t0rcsUO_Evbn5lapoDOCom97cddSIywUnb4zA14TRlrttfuOnpkj08WaR2WM38unpKjBpIHYZJYrrG5Gzyyjs2uzPfCydOCcXVuv3xcVTbmgDGVraDswDMF0xVKHwrFNG9HLfCsJhgA14_puVELPRceuXa_o-u9o05U8-BRrzvyEOxobpXc_z6c0FlnA5OcTGbVDChCASal-8kXjaZYzk1dF-FBQxK3Sj75wCi3IYg

                using https://jwt.io/
                Header
                {
                  "alg": "RS256",
                  "kid": "1DE2DF466D2188D32F7487B29BC769115DC53472",
                  "typ": "JWT"
                }

                Payload
                {
                  "sub": "https://www.datacapsystems.com/",
                  "aud": "https://www.worldpay.com/",
                  "exp": 1564942107
                }
            */

            return tokenString;
        }

        /// <summary>Validates the JWT token.</summary>
        /// <param name="jwtTokenString"></param>
        /// <param name="subject"></param>
        /// <returns>
        ///   <c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <remarks>This would be used server side</remarks>
        public static (bool isTokenValid, string validatedUsername) ValidateJWTToken(string jwtTokenString, string audience)
        {
            // determine who the caller claims to be (as identified by the private key they used)
            // the private key they used is identified by the kid parameter in the JWT header
            JwtSecurityToken jwtToken = new JwtSecurityToken(jwtTokenString);
            string signingKeyThumbprint = jwtToken.Header.Kid;

            // find the user that should be associated with the supplied thumbprint
            var userInfo = UserController.GetUserWithKeyThumbprint(signingKeyThumbprint);

            if (userInfo == null)
            {
                // fail if thumbprint is not associated with a user in our user repo
                return (false, string.Empty);
            }

            if (!userInfo.IsEnabled)
            {
                // fail if user is marked disabled in the User Repo
                return (false, string.Empty);
            }

            if (jwtToken.Payload.Sub.ToLower() != userInfo.Company.ToLower())
            {
                // fail if sub(ject) value in token payload does not match the value on the user record
                return (false, string.Empty);
            }

            // load the cert we want to use to validate the JWT
            var certificate = CertificateController.GetCertificateWithThumbprint(signingKeyThumbprint);

            // create a security key from the certificate
            SecurityKey key = new X509SecurityKey(certificate);

            // setup the token validation parameters
            TokenValidationParameters validationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudiences = new[] { audience },
                    IssuerSigningKeys = new List<SecurityKey>() { key }
                };

            // try and validate the token
            // if we can validate the JWT using the cert... we use that as a surrogate method for authentication
            SecurityToken validatedToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            try
            {
                handler.ValidateToken(jwtTokenString, validationParameters, out validatedToken);

                // validation succeeded
                return (true, userInfo.User);
            }
            catch
            {
                // validation fails
                return (false, string.Empty);
            }
        }

    }
}
