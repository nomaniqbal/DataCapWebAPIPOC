using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WebApi.Shared.Utilities
{
    public static class StringExtensionMethods
    {
        public static string SHA256Hash(this string textToHash)
        {
            //optionally compute the hash of the body
            StringBuilder hashString = new StringBuilder();

            if (!string.IsNullOrEmpty(textToHash))
            {
                var sha = new SHA256Managed();
                byte[] hashValue = sha.ComputeHash(Encoding.UTF8.GetBytes(textToHash));

                foreach (byte x in hashValue)
                {
                    hashString.AppendFormat("{0:x2}", x);
                }
            }

            return hashString.ToString();
        }
    }
}
