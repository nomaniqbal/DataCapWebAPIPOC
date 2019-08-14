using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using WebApi.Shared.Entities;
using WebApi.Shared.Utilities;

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
        public static readonly string BODYHASH_CLAIM_NAME = @"bodyhash";
        public static readonly string HASHTYPE_CLAIM_NAME = @"hashtype";
        public static readonly string CLIENT_ID_CLAIM_NAME = @"client_id";
        public static readonly string CLIENT_PUBK_CLAIM_NAME = @"client_pubk";

        private static Dictionary<string, JwtHeader> _cache = new Dictionary<string, JwtHeader>();

        /// <summary>Creates the JWT token.</summary>
        /// <param name="userName"></param>
        /// <param name="subject">The subject.</param>
        /// <param name="audience">The audience.</param>
        /// <param name="ttlMinutes">The ttlMinutes.</param>
        /// <param name="httpBody">The httpBody.</param>
        /// <returns>System.String.</returns>
        /// <remarks>This would be used on the client side</remarks>
        public static string CreateJWTToken(string userName, string subject, string audience, int ttlMinutes, string httpBody)
        {
            JwtHeader header = null;
            var userInfo = UserController.GetUserInfo(userName);

            if (!_cache.TryGetValue(userName, out header))
            {
                if (ttlMinutes < 1 || ttlMinutes > 10)
                {
                    throw new ArgumentException($"ttlMinutes paramter value of: [{ttlMinutes}] must be from 1 to 10");
                }

                // Load the Certificate
                var certificate = CertificateController.GetCertificateWithThumbprint(userInfo.KeyThumbprint);

                // Represents the cryptographic key and security algorithms that are used to generate a digital signature.
                var credentials = new SigningCredentials(new X509SecurityKey(certificate), "RS256");

                // Create a header class that will use the supplied credentials
                header = new JwtHeader(credentials);

                // add to the cache
                _cache.Add(userName, header);
            }

            // Define the JWT Payload
            //  Note: the specifc content required for this use case (Purchase Auth) needs to be determined
            JwtPayload payload = null;
            if (!string.IsNullOrEmpty(httpBody))
            {
                payload = new JwtPayload
                            {
                               { "sub", subject },
                               { "aud", audience },
                               { "exp", DateTime.UtcNow.AddMinutes(ttlMinutes).ToEpochSeconds() },
                               { CLIENT_ID_CLAIM_NAME, userInfo.ClientID },
                               { BODYHASH_CLAIM_NAME, httpBody.SHA256Hash() },
                               { HASHTYPE_CLAIM_NAME, @"sha256" }
                            };
            }
            else
            {
                payload = new JwtPayload
                            {
                               { "sub", subject },
                               { "aud", audience },
                               { "exp", DateTime.UtcNow.AddMinutes(15).ToEpochSeconds() },
                            };
            }

            // create the JWT
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            // convert the Token to a String
            var tokenString = handler.WriteToken(secToken);

            /*
                Sample JWT generated             
                eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NTAzNjcyMCwiYm9keWhhc2giOiI4NTFkOTNkMjMwYmE0ZThiMzVkNWE1ZjUwYWY3N2Q2MDgxNmJiY2ZjZGM4NWE4ODUwODZkMzhiZDVjMzM5NjViIiwiaGFzaHR5cGUiOiJzaGEyNTYifQ.bD_Aw72U9niNcy_SWdmmSyb_bcpWe6itbni5D0TunC3lf3_SkGWwnWKRlWViwp59VtC6Vj0B3M5ouXvQ1aI4ehsB6R03uAMkh4zW5HQKhccbInhtpNSeOmGh2IsjZfBK2w8_gs7z3Wsqhvio9N2PTWq9wtAuVUdNCeaBBhtsr_WP8QQjLpm0GswqGLARZc07Rw6bxOIHASsF-4Doy-huDLmydBggjO5YS2y2Wp2_MFmCIawCYvnnrSnhxzvZ1iz7bIwHX614VPlDHc2PlspLp7Lal5oJTOBoh284njuOgVlxwKaW2YjZS0W3dmUgNrro3_JDEvWeJvvgiRXk58bXdw

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
                  "exp": 1565036720,
                  "bodyhash": "851d93d230ba4e8b35d5a5f50af77d60816bbcfcdc85a885086d38bd5c33965b",
                  "hashtype": "sha256"
                }
            */

            return tokenString;
        }

        public static string CreateDownstreamJWTToken(string userName, string subject, string audience, int ttlMinutes, string upstreamjwt)
        {
            var userInfo = UserController.GetUserInfo(userName);

            if (ttlMinutes < 1 || ttlMinutes > 10)
            {
                throw new ArgumentException($"ttlMinutes paramter value of: [{ttlMinutes}] must be from 1 to 10");
            }

            // Load the Certificate
            var certificate = CertificateController.GetCertificateForDP();

            #region Experimenting with Pubic Key
            string publicKey = certificate.GetPublicKeyString();
            byte[] bytes = new byte[publicKey.Length * sizeof(char)];
            System.Buffer.BlockCopy(publicKey.ToCharArray(), 0, bytes, 0, bytes.Length);

            // ===================
            //Create a new instance of RSACryptoServiceProvider.
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            //Get an instance of RSAParameters from ExportParameters function.
            RSAParameters RSAKeyInfo = RSA.ExportParameters(false);

            //Set RSAKeyInfo to the public key values.
            RSAKeyInfo.Modulus = bytes;
            //Import key parameters into RSA.
            RSA.ImportParameters(RSAKeyInfo);
            // ===================
            #endregion

            // Represents the cryptographic key and security algorithms that are used to generate a digital signature.
            var credentials = new SigningCredentials(new X509SecurityKey(certificate), "RS256");

            // Create a header class that will use the supplied credentials
            var header = new JwtHeader(credentials);

            // Define the JWT Payload
            //  Note: the specifc content required for this use case (Purchase Auth) needs to be determined
            JwtPayload payload = new JwtPayload
                            {
                               { "sub", subject },
                               { "aud", audience },
                               { "exp", DateTime.UtcNow.AddMinutes(15).ToEpochSeconds() },
                               { "ujwt", upstreamjwt },
                            };

            // create the JWT
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            // convert the Token to a String
            var tokenString = handler.WriteToken(secToken);

            /*
                Sample JWT generated             
                eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NTAzNjcyMCwiYm9keWhhc2giOiI4NTFkOTNkMjMwYmE0ZThiMzVkNWE1ZjUwYWY3N2Q2MDgxNmJiY2ZjZGM4NWE4ODUwODZkMzhiZDVjMzM5NjViIiwiaGFzaHR5cGUiOiJzaGEyNTYifQ.bD_Aw72U9niNcy_SWdmmSyb_bcpWe6itbni5D0TunC3lf3_SkGWwnWKRlWViwp59VtC6Vj0B3M5ouXvQ1aI4ehsB6R03uAMkh4zW5HQKhccbInhtpNSeOmGh2IsjZfBK2w8_gs7z3Wsqhvio9N2PTWq9wtAuVUdNCeaBBhtsr_WP8QQjLpm0GswqGLARZc07Rw6bxOIHASsF-4Doy-huDLmydBggjO5YS2y2Wp2_MFmCIawCYvnnrSnhxzvZ1iz7bIwHX614VPlDHc2PlspLp7Lal5oJTOBoh284njuOgVlxwKaW2YjZS0W3dmUgNrro3_JDEvWeJvvgiRXk58bXdw

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
                  "exp": 1565036720,
                  "bodyhash": "851d93d230ba4e8b35d5a5f50af77d60816bbcfcdc85a885086d38bd5c33965b",
                  "hashtype": "sha256"
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
        public static (bool isTokenValid, string validatedUsername, JwtSecurityToken jwtToken) ValidateJWTToken(string jwtTokenString, string audience)
        {
            // determine who the caller claims to be (as identified by the private key they used)
            // the private key they used is identified by the kid parameter in the JWT header
            JwtSecurityToken jwtToken = new JwtSecurityToken(jwtTokenString);
            //string signingKeyThumbprint = jwtToken.Header.Kid;

            // find the user that should be associated with the supplied thumbprint
            //var userInfo = UserController.GetUserWithKeyThumbprint(signingKeyThumbprint);
            string clientID = jwtToken.GetPayloadValue(CLIENT_ID_CLAIM_NAME);
            var userInfo = UserController.GetUserWithClientID(clientID);

            if (userInfo == null)
            {
                // fail if thumbprint is not associated with a user in our user repo
                return (false, string.Empty, null);
            }

            if (!userInfo.IsEnabled)
            {
                // fail if user is marked disabled in the User Repo
                return (false, string.Empty, null);
            }

            if (jwtToken.Payload.Sub.ToLower() != userInfo.Company.ToLower())
            {
                // fail if sub(ject) value in token payload does not match the value on the user record
                return (false, string.Empty, null);
            }

            if (!jwtToken.Payload.ContainsKey(CLIENT_ID_CLAIM_NAME) || ((string)jwtToken.Payload[CLIENT_ID_CLAIM_NAME] != userInfo.ClientID.ToLower()))
            {
                // fail if Client id value in token payload does not match the value on the user record
                return (false, string.Empty, null);
            }

            // load the cert we want to use to validate the JWT
            var certificate = CertificateController.GetCertificateWithThumbprint(userInfo.KeyThumbprint);

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
                return (true, userInfo.User, jwtToken);
            }
            catch
            {
                // validation fails
                return (false, string.Empty, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jwtPayload"></param>
        /// <returns></returns>
        public static (string hashType, string bodyHash) GetBodyHashInfo(JwtPayload jwtPayload)
        {
            string hashType = string.Empty;
            string bodyHash = string.Empty;

            if (jwtPayload.ContainsKey(HASHTYPE_CLAIM_NAME))
            {
                hashType = jwtPayload[HASHTYPE_CLAIM_NAME].ToString();
            }

            if (jwtPayload.ContainsKey(BODYHASH_CLAIM_NAME))
            {
                bodyHash = jwtPayload[BODYHASH_CLAIM_NAME].ToString();
            }


            return (hashType, bodyHash);
        }
    }
}
