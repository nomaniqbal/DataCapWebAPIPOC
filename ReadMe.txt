=======================================================================
!!! YOU WILL NEED TO UPDATE THIS ABSOLUTE PATH BEFORE THIS WILL RUN !!!
=======================================================================
Project:	WebApi.Shared
File:		CertificateController.cs

	const string CERT_FOLDER_PATHNAME = @"C:\Users\xtobr\Source\Repos\Worldpay\DataCapWebAPIPOC\ClientCertificates";

=================================================================
How the certs that are being used were created
=================================================================

Powershell command to create a new certificate (run as an admin!)

New-SelfSignedCertificate -Type Custom -Subject "CN=DO_NOT_USE_IN_PROD_USER_A" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2","2.5.29.17={text}upn=joe@contoso.com") -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation "Cert:\LocalMachine\My"
New-SelfSignedCertificate -Type Custom -Subject "CN=DO_NOT_USE_IN_PROD_USER_B" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2","2.5.29.17={text}upn=joe@contoso.com") -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation "Cert:\LocalMachine\My"
New-SelfSignedCertificate -Type Custom -Subject "CN=DO_NOT_USE_IN_PROD_USER_C" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2","2.5.29.17={text}upn=joe@contoso.com") -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation "Cert:\LocalMachine\My"

The result will be in the current machine's cert store
After running you can go into MMC "cert" (ie Manage Computer Certificates)
Find the new cert and Export it (since you wall also be exporting the private key make sure use a password!)

The password to use during the export is in the file password.txt

=================================================================
HTTP Traces
=================================================================

-----------------------------------------------------------------
Typical for http request w/o an Auth Header
-----------------------------------------------------------------
GET https://localhost:44346/api/values HTTP/1.1
Host: localhost:44346
Accept: application/json, text/json, text/x-json, text/javascript, application/xml, text/xml
User-Agent: RestSharp/106.6.10.0
Connection: Keep-Alive
Accept-Encoding: gzip, deflate

HTTP/1.1 401 Unauthorized
Transfer-Encoding: chunked
Content-Type: application/problem+json; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Sun, 04 Aug 2019 18:08:25 GMT

8f
{"type":"https://tools.ietf.org/html/rfc7235#section-3.1","title":"Unauthorized","status":401,"traceId":"8000001d-0002-fd00-b63f-84710c7967bb"}
0



-----------------------------------------------------------------
Typical for UserA & UserB which are "valid" (recognized & enabled)
-----------------------------------------------------------------
GET https://localhost:44346/api/values HTTP/1.1
Host: localhost:44346
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NDk0MjEwN30.mwwaFczSPP1_EMDcgAdXIbf3hwHw26nTv-kG4b1_EH9q8TFrNMmPMjayyWzHDizbwF-As-6AppaNlMbEQFp-ilXLCx_MAgvff1vNA_qA_wh_t0rcsUO_Evbn5lapoDOCom97cddSIywUnb4zA14TRlrttfuOnpkj08WaR2WM38unpKjBpIHYZJYrrG5Gzyyjs2uzPfCydOCcXVuv3xcVTbmgDGVraDswDMF0xVKHwrFNG9HLfCsJhgA14_puVELPRceuXa_o-u9o05U8-BRrzvyEOxobpXc_z6c0FlnA5OcTGbVDChCASal-8kXjaZYzk1dF-FBQxK3Sj75wCi3IYg
Accept: application/json, text/json, text/x-json, text/javascript, application/xml, text/xml
User-Agent: RestSharp/106.6.10.0
Connection: Keep-Alive
Accept-Encoding: gzip, deflate


Using https://jwt.io to analyze the token...

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


HTTP/1.1 200 OK
Transfer-Encoding: chunked
Content-Type: application/json; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Sun, 04 Aug 2019 18:08:28 GMT

266
{"isValid":true,"user":"UserA","jwtToken":"eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NDk0MjEwN30.mwwaFczSPP1_EMDcgAdXIbf3hwHw26nTv-kG4b1_EH9q8TFrNMmPMjayyWzHDizbwF-As-6AppaNlMbEQFp-ilXLCx_MAgvff1vNA_qA_wh_t0rcsUO_Evbn5lapoDOCom97cddSIywUnb4zA14TRlrttfuOnpkj08WaR2WM38unpKjBpIHYZJYrrG5Gzyyjs2uzPfCydOCcXVuv3xcVTbmgDGVraDswDMF0xVKHwrFNG9HLfCsJhgA14_puVELPRceuXa_o-u9o05U8-BRrzvyEOxobpXc_z6c0FlnA5OcTGbVDChCASal-8kXjaZYzk1dF-FBQxK3Sj75wCi3IYg"}
0



-----------------------------------------------------------------
Typical for UserC which is "disabled"
-----------------------------------------------------------------
GET https://localhost:44346/api/values HTTP/1.1
Host: localhost:44346
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IkRCNzIxMDhEOTJCOEU1MUJDREJBREM2NUREMjBEMzM3NjkyNEI0N0YiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5zb21lZGlzYWJsZWRjb21wYW55LmNvbS8iLCJhdWQiOiJodHRwczovL3d3dy53b3JsZHBheS5jb20vIiwiZXhwIjoxNTY0OTQyMTEzfQ.KTk0IJbiCnqcx1dNpLkYpM3-hAYP1zJvpuD_7SkT3MoamtSAzRdtfkFZblK0spZ8Tkxiu_rs51OR0Ot2Dge367Ba_e6T83stjc_1FMLvDliK_1ITa8b9bZnztwobUKJh4V-unIebcyRAz0vIGpj2YW-SoUdAHIgDH6wSe4gRCm4gOUFtN-PIf8LTTuied8XgLgI3BOkEIkZiIEtTpqT5LJId48DzPtBwixcZVZYYUsutd2VugCjpHpShWpARG9Hn43JVXcimnzNgF0h3hAL4_vAnu5iNtd6nT5hVJnhNSxkPwGl1OzjXWGVLtJCTLj8lkUOzvD9VCIOKpClii9xCaw
Accept: application/json, text/json, text/x-json, text/javascript, application/xml, text/xml
User-Agent: RestSharp/106.6.10.0
Connection: Keep-Alive
Accept-Encoding: gzip, deflate

HTTP/1.1 401 Unauthorized
Transfer-Encoding: chunked
Content-Type: application/problem+json; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Sun, 04 Aug 2019 18:08:33 GMT

8f
{"type":"https://tools.ietf.org/html/rfc7235#section-3.1","title":"Unauthorized","status":401,"traceId":"80000006-0003-fc00-b63f-84710c7967bb"}
0



