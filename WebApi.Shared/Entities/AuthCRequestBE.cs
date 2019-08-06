using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace WebApi.Shared.Entities
{
    public class AuthCRequestBE
    {
        public AuthCRequestBE(string iso8583Base64String)
        {
            this.Iso8583Base64String = iso8583Base64String;
        }

        [JsonProperty(@"ISO8583_BASE64")]
        public string Iso8583Base64String { get; set; }

        /// <summary>
        /// Return the serialized form of the request
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
