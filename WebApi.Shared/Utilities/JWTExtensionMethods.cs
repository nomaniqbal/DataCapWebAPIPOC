using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace WebApi.Shared.Utilities
{
    public static class JWTExtensionMethods
    {
        public static string GetPayloadValue(this JwtSecurityToken jwtToken, string claimName)
        {
            object value = string.Empty;

            if (jwtToken.Payload.ContainsKey(claimName))
            {

                jwtToken.Payload.TryGetValue(claimName, out value);
            }

            return value.ToString();
        }
    }
}
