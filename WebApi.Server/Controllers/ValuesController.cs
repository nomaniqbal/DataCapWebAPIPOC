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

using WebApi.Shared.Controllers;
using WebApi.Shared.Entities;
using WebApi.Shared.Utilities;
using WebApi.Server.Utilities;
using System.IdentityModel.Tokens.Jwt;

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
        public ActionResult<string> Get()
        {
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
                string jwtTokenString = parts[1];

                (bool isJwtTokenValid, string username, JwtSecurityToken jwtToken) = JwtController.ValidateJWTToken(jwtTokenString, JwtController.AUDIENCE);

                if(!isJwtTokenValid)
                {
                    return Unauthorized();
                }
                else
                {
                    // at this point this controller would add the identity of the authenticated caller as a new HTTP header and call a downstream webapi

                    // in this POC just return the identity that we derived from the JWT
                    var authCResult = new AuthCResultBE() { IsValid = isJwtTokenValid, User = username, JwtToken = jwtTokenString };
                    //return Ok(JsonConvert.SerializeObject(authCResult, Formatting.Indented));
                    return Ok(authCResult);
                }

            }
        }

        // POST api/values
        [HttpPost]
        //public ActionResult<string> Post([FromBody] string rawRequest)
        public async Task<ActionResult<string>> Post()
        {
            // note: I specficially choose NOT to use the [FromBody] approach to make debugging easier
            //          if we use the [FromBody] approach this method never get control if the body cannot be correctly deserialized
            //          this approach lets us capture the raw body content passed
            string httpBody = Request.GetRawBodyString();

            AuthCRequestBE authRequest = null;
            try
            {
                authRequest = JsonConvert.DeserializeObject<AuthCRequestBE>(httpBody);
            }
            catch(Exception ex)
            {
                // you could do addl logging here to support debugging
                return BadRequest();
            }

            // look for an Auth Header
            // we expect it to look like:
            // Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NDk0MjEwN30.mwwaFczSPP1_EMDcgAdXIbf3hwHw26nTv-kG4b1_EH9q8TFrNMmPMjayyWzHDizbwF-As-6AppaNlMbEQFp-ilXLCx_MAgvff1vNA_qA_wh_t0rcsUO_Evbn5lapoDOCom97cddSIywUnb4zA14TRlrttfuOnpkj08WaR2WM38unpKjBpIHYZJYrrG5Gzyyjs2uzPfCydOCcXVuv3xcVTbmgDGVraDswDMF0xVKHwrFNG9HLfCsJhgA14_puVELPRceuXa_o-u9o05U8-BRrzvyEOxobpXc_z6c0FlnA5OcTGbVDChCASal-8kXjaZYzk1dF-FBQxK3Sj75wCi3IYg
            string authHeader = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized();
            }
            else
            {
                // parse the header
                var parts = authHeader.Split(" ");
                string jwtTokenString = parts[1];

                (bool isJwtTokenValid, string username, JwtSecurityToken jwtToken) = JwtController.ValidateJWTToken(jwtTokenString, JwtController.AUDIENCE);

                if (!isJwtTokenValid)
                {
                    return Unauthorized();
                }
                else
                {
                    // at this point this controller would add the identity of the authenticated caller as a new HTTP header and call a downstream webapi
                    // this part would typically be in the downstream client and called using await
                    
                    // validate that the body has not body modified
                    string hash = httpBody.SHA256Hash();
                    (string hashType, string bodyHash) = JwtController.GetBodyHashInfo(jwtToken.Payload);
                    if(!string.IsNullOrEmpty(hashType))
                    {
                        if (hashType.ToLower() != @"sha256")
                        {
                            return BadRequest($"hash type: [{hashType}] is NOT supported, use sha256 instead!");
                        }

                        if(httpBody.SHA256Hash() != bodyHash)
                        {
                            return BadRequest($"Post Body has been modified");
                        }
                    }


                    // in this POC just return the identity that we derived from the JWT
                    var authCResult = new AuthCResultBE() { IsValid = isJwtTokenValid, User = username, JwtToken = jwtTokenString };
                    //return Ok(JsonConvert.SerializeObject(authCResult, Formatting.Indented));
                    return Ok(authCResult);
                }

            }
        }

    }
}
