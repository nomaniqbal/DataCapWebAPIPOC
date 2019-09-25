using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using RestSharp;

using WebApi.Shared.Controllers;
using WebApi.Client.ISO8583;
using WebApi.Shared.Entities;
using System.Linq;
using System.Diagnostics;

namespace WebApi.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleLoop();
        }

        /// <summary>
        /// Console Loop
        /// </summary>
        static void ConsoleLoop()
        {
            const int MOD_PAYLOAD_OPTION = 99;

            // load the list of available users
            var users = UserController.LoadUsers();

            // Build a menu of Scenarions that can used to test
            StringBuilder sb = new StringBuilder();
            int userIdx = 1;
            sb.AppendLine();
            sb.AppendLine("DataCap POC for Auth via JWT Headers");
            sb.AppendLine();
            sb.AppendLine(" List of Test Users");
            sb.AppendLine("====================");
            sb.AppendLine($" [0] Anonymous (Do not send an Auth Header)");
            foreach (var user in users)
            {
                sb.AppendLine($" [{userIdx}] {user.User} " + (user.IsEnabled ? string.Empty : "(Disabled)"));

                userIdx++;
            }

            // pick a user to use for the Modified Payload
            var goodTestUser = users.Where(u => u.IsEnabled == true).FirstOrDefault();

            sb.AppendLine($" [{MOD_PAYLOAD_OPTION}] {goodTestUser.User} (Modified Body)");
            sb.AppendLine(" <enter> to exit");
            sb.AppendLine();
            
            // define variables outside the while loop to reduce GC pressure 
            string choiceText = string.Empty;
            int choiceIndex = 0;
            UserIdentityBE selectedUser;
            bool shouldContinue = true;
            AuthCRequestBE authcRequest = null;
            string httpBody = string.Empty;
            Stopwatch stw = null;
            string jwtToken = string.Empty;
            long create1stJWTElapsed = 0;

            // =========================
            // start the message loop
            // =========================
            while (shouldContinue)
            {
                // reset variables
                choiceIndex = -1;
                selectedUser = null;

                // clear the screen
                Console.Clear();

                // display the menu
                Console.WriteLine(sb.ToString());
                Console.Write("Choice: > ");
                choiceText = Console.ReadLine();

                if (string.IsNullOrEmpty(choiceText))
                {
                    // blank == exit
                    shouldContinue = false;
                }
                else
                {
                    // if the entry is a number and within the list, find the associated user 
                    Int32.TryParse(choiceText, out choiceIndex);
                    if (choiceIndex == 0)
                    {
                        selectedUser = new UserIdentityBE() { User = @"Anonymous" };
                        Console.WriteLine($" ==> Ready to send anonymous request");
                    }
                    else if (choiceIndex == MOD_PAYLOAD_OPTION)
                    {
                        selectedUser = goodTestUser;
                        Console.WriteLine($" ==> Ready to send request with tampered body");
                    }
                    else if (choiceIndex > 0 && choiceIndex <= users.Count)
                    {
                        selectedUser = users[choiceIndex - 1];
                        Console.WriteLine($" ==> Ready to send request for: {selectedUser.User} [{selectedUser.Company}].");
                    }

                    if (selectedUser == null)
                    {
                        Console.WriteLine($" [{choiceText}] is not at valid selection.");
                        Console.WriteLine($" ==> Press <enter> to continue");
                    }
                    else
                    {
                        // ==================================================
                        // this should be as short as possible to limit ability to fradualently reuse, 
                        //  but long enough to tolerate clock skew between client and service
                        // ==================================================
                        int ttlMinutes = 5; 

                        authcRequest = IsoMsgBuilder.GetAuthMsg();
                        httpBody = authcRequest.ToString();

                        stw = new Stopwatch();
                        stw.Start();
                        jwtToken = (choiceIndex > 0) ? JwtController.CreateJWTToken(selectedUser.User, selectedUser.Company, JwtController.AUDIENCE, ttlMinutes, httpBody) : string.Empty;
                        stw.Stop();
                        create1stJWTElapsed = stw.ElapsedMilliseconds;

                        // test ben token
                        //string benToken = JwtController.CreateBENJWTToken(selectedUser.User, selectedUser.Company, JwtController.AUDIENCE, ttlMinutes);

                        //stw.Reset();
                        //stw.Start();
                        //jwtToken = (choiceIndex > 0) ? JwtController.CreateJWTToken(selectedUser.User, selectedUser.Company, JwtController.AUDIENCE, ttlMinutes, httpBody) : string.Empty;
                        //stw.Stop();
                        //var create2ndJWTElapsed = stw.ElapsedMilliseconds;

                        //string downstreamJwtToken = (choiceIndex > 0) ? JwtController.CreateDownstreamJWTToken(selectedUser.User, selectedUser.Company, JwtController.AUDIENCE, ttlMinutes, jwtToken) : string.Empty;
                        //stw.Reset();
                        //stw.Start();
                        //var dsT = JwtController.ValidateJWTToken(downstreamJwtToken, JwtController.AUDIENCE);
                        //stw.Stop();
                        //var validate1stJWTElapsed = stw.ElapsedMilliseconds;

                        //stw.Reset();
                        //stw.Start();
                        //dsT = JwtController.ValidateJWTToken(downstreamJwtToken, JwtController.AUDIENCE);
                        //stw.Stop();
                        //var validate2ndJWTElapsed = stw.ElapsedMilliseconds;

                        if (choiceIndex == MOD_PAYLOAD_OPTION)
                        {
                            // simulate man-in-the middle tampering with the payload
                            httpBody = httpBody + "123";
                        }

                        // make the call
                        (HttpStatusCode responseCode, IList<Parameter> responseHeaders, string responseContent) = CallWebApiPost(jwtToken, httpBody);

                        // evaluate the response
                        if (responseCode == HttpStatusCode.OK)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else if (responseCode == HttpStatusCode.Unauthorized)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                        }

                        Console.WriteLine();
                        Console.WriteLine($"responseCode: [{responseCode.ToString()}]");
                        Console.WriteLine();
                        Console.WriteLine($"headers");
                        foreach(var responseHeader in responseHeaders)
                        {
                            Console.WriteLine($"   {responseHeader.Name} : {responseHeader.Value}");
                        }
                        Console.WriteLine();
                        Console.WriteLine($"content: [{responseContent}]");

                        Console.ResetColor();

                        Console.WriteLine($"Create 1st JWT: [{create1stJWTElapsed} mSec]");
                        //Console.WriteLine($"Create 2nd JWT: [{create2ndJWTElapsed} mSec]");
                        //Console.WriteLine($"Valdiate 1st JWT: [{validate1stJWTElapsed} mSec]");
                        //Console.WriteLine($"Valdiate 2nd JWT: [{validate2ndJWTElapsed} mSec]");

                        Console.WriteLine();
                        Console.WriteLine($" ==> Press <enter> to continue");
                    }

                    Console.ReadLine();
                }
            }
        }

        /// <summary>Calls the web API get endpoint.</summary>
        /// <param name="jwtToken">The JWT token.</param>
        /// <returns>System.ValueTuple&lt;HttpStatusCode, System.String&gt;.</returns>
        static (HttpStatusCode responseCode, string content) CallWebApiGet(string jwtToken)
        {
            // build the url 
            string baseWebUrl = @"localhost:44346";
            string url = string.Format($"https://{baseWebUrl}");

            var client = new RestClient(url);

            var request = new RestRequest("/api/values", Method.GET);

            // if a JWT is available, add a Bearer Auth Token
            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.AddHeader("Authorization", $"Bearer {jwtToken}");
            }

            // call the WebAPI
            IRestResponse response = client.Execute(request);
            var responseCode = response.StatusCode;
            var content = response.Content;

            // return the response
            return (responseCode, content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="bodyContent"></param>
        /// <returns></returns>
        static (HttpStatusCode responseCode, IList<Parameter> responseHeaders, string responseContent) CallWebApiPost(string jwtToken, string bodyContent)
        {
            // Datapower POC endpoint
            // https://ws-stage.infoftps.com:4443/JWT/Validate
            // build the url  
            string baseWebUrl = @"ws-stage.infoftps.com:4443"; // @"localhost:44346";
            string url = string.Format($"https://{baseWebUrl}");

            var client = new RestClient(url);

            // var request = new RestRequest("/api/values", Method.POST, DataFormat.Json);
            var request = new RestRequest("/JWT/Validate", Method.POST, DataFormat.Json);
            request.AddBody(bodyContent);

            // if a JWT is available, add a Bearer Auth Token
            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.AddHeader("Authorization", $"Bearer {jwtToken}");
            }

            // call the WebAPI
            IRestResponse response = client.Execute(request);

            // grab parts of the http response
            var responseCode = response.StatusCode;
            var responseContent = response.Content;
            var responseHeaders = response.Headers;

            // return the response
            return (responseCode, responseHeaders, responseContent);
        }
    }
}
