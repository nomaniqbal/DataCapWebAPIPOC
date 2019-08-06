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

namespace WebApi.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleLoop();
        }

        /// <summary>
        /// Consoles the loop.
        /// </summary>
        static void ConsoleLoop()
        {
            const int MOD_PAYLOAD_OPTION = 99;

            // load the list of available users
            var users = UserController.LoadUsers();

            // Build a menu of User choices that can used to test
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

            var goodTestUser = users.Where(u => u.IsEnabled == true).FirstOrDefault();

            sb.AppendLine($" [{MOD_PAYLOAD_OPTION}] {goodTestUser.User} (Modified Body)");
            sb.AppendLine(" <enter> to exit");
            sb.AppendLine();
            
            // define variables outside the while loop to reduce GC pressure 
            string choiceText = string.Empty;
            int choiceIndex = 0;
            UserIdentityBE selectedUser;
            bool shouldContinue = true;

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
                        // this should be as short as possible to limit ability to fradualently reuse, 
                        //  but long enough to tolerate clock skew between client and service
                        int ttlMinutes = 2; 
                        AuthCRequestBE authcRequest = IsoMsgBuilder.GetAuthMsg();
                        string httpBody = authcRequest.ToString();
                        string jwtToken = (choiceIndex > 0) ? JwtController.CreateJWTToken(selectedUser.User, selectedUser.Company, JwtController.AUDIENCE, ttlMinutes, httpBody) : string.Empty;
                        
                        if(choiceIndex == MOD_PAYLOAD_OPTION)
                        {
                            httpBody = httpBody + "123";
                        }

                        (HttpStatusCode responseCode, string content) = CallWebApiPost(jwtToken, httpBody);

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
                        Console.WriteLine($"content: [{content}]");

                        Console.ResetColor();

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

        /// <summary>Calls the web API post endpoint.</summary>
        /// <param name="jwtToken">The JWT token.</param>
        /// <param name="bodyContent">The Content to post in the request body</param>
        /// <returns>System.ValueTuple&lt;HttpStatusCode, System.String&gt;.</returns>
        static (HttpStatusCode responseCode, string content) CallWebApiPost(string jwtToken, string bodyContent)
        {
            // build the url 
            string baseWebUrl = @"localhost:44346";
            string url = string.Format($"https://{baseWebUrl}");

            var client = new RestClient(url);

            var request = new RestRequest("/api/values", Method.POST, DataFormat.Json);
            request.AddBody(bodyContent);

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
    }
}
