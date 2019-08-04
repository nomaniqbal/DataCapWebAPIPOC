using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Shared.Entities
{
    public class AuthCResultBE
    {
        public bool IsValid { get; set; }
        public string User { get; set; }
        public string JwtToken { get; set; }
    }
}
