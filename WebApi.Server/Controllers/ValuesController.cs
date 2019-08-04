using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.Shared;
using WebApi.Shared.Controllers;
using WebApi.Shared.Entities;

namespace WebApi.Server.Controllers
{
    /// <summary>
    /// This is a sample controller for a WebAPI
    /// 
    /// In this scenario this endpoint would be exposed by DataPower
    /// If the caller is successfully authenticated, they would add a new HTTP Header that identifies the caller and forward
    /// the HTTP Call to the new Mercury IP Receiver Service
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<string> lst = new List<string>();

            // look for an Auth Header
            // we expect it to look like:
            // 
            string authHeader = Request.Headers["Authorization"];

            if(string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized();
            }
            else
            {
                // parse the header
                var parts = authHeader.Split(" ");
                string jwtToken = parts[1];

                (bool isJwtTokenValid, string username) = JwtController.ValidateJWTToken(jwtToken, JwtController.AUDIENCE);

                if(!isJwtTokenValid)
                {
                    return Unauthorized();
                }
                else
                {
                    // at this point this controller would add the identity of the authenticated caller as a new HTTP header and call a downstream webapi

                    // in this POC just return the identity that we derived from the JWT
                    var authCResult = new AuthCResultBE() { IsValid = isJwtTokenValid, User = username, JwtToken = jwtToken };
                    //return Ok(JsonConvert.SerializeObject(authCResult, Formatting.Indented));
                    return Ok(authCResult);
                }

            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

    }
}
