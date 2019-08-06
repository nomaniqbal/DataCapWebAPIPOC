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
POST https://localhost:44346/api/values HTTP/1.1
Accept: application/json, text/json, text/x-json, text/javascript, application/xml, text/xml
User-Agent: RestSharp/106.6.10.0
Connection: Keep-Alive
Accept-Encoding: gzip, deflate
Content-Type: application/json
Content-Length: 741
Host: localhost:44346

{"ISO8583_BASE64":"AhkwMjAw8jxEgQCAgCAAAAAAAAAAIjE2NTQ5OTk5MDEyMzQ1Njc4MTAwMzAwMDAwMDAwMDAwMDAyODA4MDExODQ2MTAwMDAwNDUxMjQ2MTAwODAxMjAwNTU4MTIwMTEwMDA2MDIzNDAwMTQ2NjMgICA4NDAwMTIwMDAwMDEwMDAwMDEwMTUyMTExMDE2NTQwMDExMDEwMDAzODREAAQAAAAAADMyYzBmM2VlOTA1ODU1NDMzNzg2ZGE0MTdmZTFiNDNjNTcxMTAwMzM1MjE0QWRkaXRpb25hbERhdGExMTAyMTFFeHRlcm5hbFRJRDE4MTAxMzI2NjUyMThHbG9iYWxQT1NFbnRyeU1vZGUyMTIyMTAxMDA3MDAwMDAyMjBHbG9iYWxQcm9jZXNzaW5nQ29kZTE2MDAzMDAwMjE4R2xvYmFsVGVybWluYWxUeXBlMTNEQzIxNkxhbmVJRDEwMjEwTWFya2V0RGF0YTIxM2FGdDAwMDAwMDAwMDEyMThQdXJjaGFzaW5nQ2FyZERhdGEyOTA8P3htbCB2ZXJzaW9uPSIxLjAiIGVuY29kaW5nPSJ1dGYtOCI/PjxQdXJjaGFzaW5nQ2FyZERhdGE+PENvbnRhY3QgLz48L1B1cmNoYXNpbmdDYXJkRGF0YT4yMTdUcmFuc2FjdGlvblN0YXR1czExMDIxN1Zpc2FFQ29tbUdvb2RzSW5kMTA="}

HTTP/1.1 401 Unauthorized
Transfer-Encoding: chunked
Content-Type: application/problem+json; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Tue, 06 Aug 2019 18:46:18 GMT

8f
{"type":"https://tools.ietf.org/html/rfc7235#section-3.1","title":"Unauthorized","status":401,"traceId":"8000000a-0002-ff00-b63f-84710c7967bb"}
0


-----------------------------------------------------------------
Typical for UserA & UserB which are "valid" (recognized & enabled)
-----------------------------------------------------------------
POST https://localhost:44346/api/values HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NTExNzE4NiwiYm9keWhhc2giOiI2MDQwM2JiZDYzNTU0OWFmMzg4NjFkN2UyNzM5MTMyOWM4ZTQzMzk1N2JhMWUwZWNhOTJjZWVkZDAyZjFmODIzIiwiaGFzaHR5cGUiOiJzaGEyNTYifQ.cLpIelisj5sviPlZU0ryQSO9gxYnVCUC_l6hPARx1kbzB-mx32TAzNiGK_OgbLmAiIcmBCdVIyzmDjj2gHNs__hGbrAyvutd81jBDk0nkVvZtq6yr4DdcFkiQGVLhJlY7iaNnORMUtyiR7RuFbcu7QkGqLRVsNUjo6mWeLtXqpNGjN_xqbGXDvDgKNQpz4lmN6_zvx3DR4s8VWNCLKYCCBGkdYwXox9q54m2vEWARBAATT2OdOewFL0tEhawBzT28I3Pyijq8-Z1ty5OnCzRYT1gbWEkgN5jw5vrxwCen188oWSp8cayOyb-sada6dcH0j5E8P9T1Ia4G757-kugOA
Accept: application/json, text/json, text/x-json, text/javascript, application/xml, text/xml
User-Agent: RestSharp/106.6.10.0
Connection: Keep-Alive
Accept-Encoding: gzip, deflate
Content-Type: application/json
Content-Length: 741
Host: localhost:44346

{"ISO8583_BASE64":"AhkwMjAw8jxEgQCAgCAAAAAAAAAAIjE2NTQ5OTk5MDEyMzQ1Njc4MTAwMzAwMDAwMDAwMDAwMDAyODA4MDExODQ2MTAwMDAwNDUxMjQ2MTAwODAxMjAwNTU4MTIwMTEwMDA2MDIzNDAwMTQ2NjMgICA4NDAwMTIwMDAwMDEwMDAwMDEwMTUyMTExMDE2NTQwMDExMDEwMDAzODREAAQAAAAAADMyYzBmM2VlOTA1ODU1NDMzNzg2ZGE0MTdmZTFiNDNjNTcxMTAwMzM1MjE0QWRkaXRpb25hbERhdGExMTAyMTFFeHRlcm5hbFRJRDE4MTAxMzI2NjUyMThHbG9iYWxQT1NFbnRyeU1vZGUyMTIyMTAxMDA3MDAwMDAyMjBHbG9iYWxQcm9jZXNzaW5nQ29kZTE2MDAzMDAwMjE4R2xvYmFsVGVybWluYWxUeXBlMTNEQzIxNkxhbmVJRDEwMjEwTWFya2V0RGF0YTIxM2FGdDAwMDAwMDAwMDEyMThQdXJjaGFzaW5nQ2FyZERhdGEyOTA8P3htbCB2ZXJzaW9uPSIxLjAiIGVuY29kaW5nPSJ1dGYtOCI/PjxQdXJjaGFzaW5nQ2FyZERhdGE+PENvbnRhY3QgLz48L1B1cmNoYXNpbmdDYXJkRGF0YT4yMTdUcmFuc2FjdGlvblN0YXR1czExMDIxN1Zpc2FFQ29tbUdvb2RzSW5kMTA="}


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
  "exp": 1565117186,
  "bodyhash": "60403bbd635549af38861d7e27391329c8e433957ba1e0eca92ceedd02f1f823",
  "hashtype": "sha256"
}


HTTP/1.1 200 OK
Transfer-Encoding: chunked
Content-Type: application/json; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Tue, 06 Aug 2019 18:46:27 GMT

2e9
{"isValid":true,"user":"UserA","jwtToken":"eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NTExNzE4NiwiYm9keWhhc2giOiI2MDQwM2JiZDYzNTU0OWFmMzg4NjFkN2UyNzM5MTMyOWM4ZTQzMzk1N2JhMWUwZWNhOTJjZWVkZDAyZjFmODIzIiwiaGFzaHR5cGUiOiJzaGEyNTYifQ.cLpIelisj5sviPlZU0ryQSO9gxYnVCUC_l6hPARx1kbzB-mx32TAzNiGK_OgbLmAiIcmBCdVIyzmDjj2gHNs__hGbrAyvutd81jBDk0nkVvZtq6yr4DdcFkiQGVLhJlY7iaNnORMUtyiR7RuFbcu7QkGqLRVsNUjo6mWeLtXqpNGjN_xqbGXDvDgKNQpz4lmN6_zvx3DR4s8VWNCLKYCCBGkdYwXox9q54m2vEWARBAATT2OdOewFL0tEhawBzT28I3Pyijq8-Z1ty5OnCzRYT1gbWEkgN5jw5vrxwCen188oWSp8cayOyb-sada6dcH0j5E8P9T1Ia4G757-kugOA"}
0



-----------------------------------------------------------------
Typical for UserC which is "disabled"
-----------------------------------------------------------------
POST https://localhost:44346/api/values HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IkRCNzIxMDhEOTJCOEU1MUJDREJBREM2NUREMjBEMzM3NjkyNEI0N0YiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5zb21lZGlzYWJsZWRjb21wYW55LmNvbS8iLCJhdWQiOiJodHRwczovL3d3dy53b3JsZHBheS5jb20vIiwiZXhwIjoxNTY1MTE3MzIzLCJib2R5aGFzaCI6IjYwNDAzYmJkNjM1NTQ5YWYzODg2MWQ3ZTI3MzkxMzI5YzhlNDMzOTU3YmExZTBlY2E5MmNlZWRkMDJmMWY4MjMiLCJoYXNodHlwZSI6InNoYTI1NiJ9.BJWHkXajGgsuX6kuMRA8DzUcV4inYLvgvmDr5_xn4XV2tNlf2G2ClX0IOhnI1VLBxHHe7xmKGOb0r8C89LW4qCzcsoGOW_HEdvpP4QXJ_PVWKD98l6l-e9NLQd-9InUk4Df2y-qbpQtHGjrTjpVRTpvvSFpPPwqDoll-IDEn3xeHmffyMvjJTJ720xk7u4SJY6E-LFNQHMjcVyLGNy2_BSxQEXCnqJzPeO34hUqD06-qWbHv4pnPaHN_oGPx8GNrf2z4vsxZMMCmj-E_YxuKgId2O6WhzIEOEw4oDeup8ciy73XjmAhu97X8wqqQErocE-iqReApGazOlmSFtve1ug
Accept: application/json, text/json, text/x-json, text/javascript, application/xml, text/xml
User-Agent: RestSharp/106.6.10.0
Connection: Keep-Alive
Accept-Encoding: gzip, deflate
Content-Type: application/json
Content-Length: 741
Host: localhost:44346

{"ISO8583_BASE64":"AhkwMjAw8jxEgQCAgCAAAAAAAAAAIjE2NTQ5OTk5MDEyMzQ1Njc4MTAwMzAwMDAwMDAwMDAwMDAyODA4MDExODQ2MTAwMDAwNDUxMjQ2MTAwODAxMjAwNTU4MTIwMTEwMDA2MDIzNDAwMTQ2NjMgICA4NDAwMTIwMDAwMDEwMDAwMDEwMTUyMTExMDE2NTQwMDExMDEwMDAzODREAAQAAAAAADMyYzBmM2VlOTA1ODU1NDMzNzg2ZGE0MTdmZTFiNDNjNTcxMTAwMzM1MjE0QWRkaXRpb25hbERhdGExMTAyMTFFeHRlcm5hbFRJRDE4MTAxMzI2NjUyMThHbG9iYWxQT1NFbnRyeU1vZGUyMTIyMTAxMDA3MDAwMDAyMjBHbG9iYWxQcm9jZXNzaW5nQ29kZTE2MDAzMDAwMjE4R2xvYmFsVGVybWluYWxUeXBlMTNEQzIxNkxhbmVJRDEwMjEwTWFya2V0RGF0YTIxM2FGdDAwMDAwMDAwMDEyMThQdXJjaGFzaW5nQ2FyZERhdGEyOTA8P3htbCB2ZXJzaW9uPSIxLjAiIGVuY29kaW5nPSJ1dGYtOCI/PjxQdXJjaGFzaW5nQ2FyZERhdGE+PENvbnRhY3QgLz48L1B1cmNoYXNpbmdDYXJkRGF0YT4yMTdUcmFuc2FjdGlvblN0YXR1czExMDIxN1Zpc2FFQ29tbUdvb2RzSW5kMTA="}

HTTP/1.1 401 Unauthorized
Transfer-Encoding: chunked
Content-Type: application/problem+json; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Tue, 06 Aug 2019 18:48:44 GMT

8f
{"type":"https://tools.ietf.org/html/rfc7235#section-3.1","title":"Unauthorized","status":401,"traceId":"8000000e-0000-ff00-b63f-84710c7967bb"}
0

-----------------------------------------------------------------
Typical for valid user when the body has been modified
-----------------------------------------------------------------
POST https://localhost:44346/api/values HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjFERTJERjQ2NkQyMTg4RDMyRjc0ODdCMjlCQzc2OTExNURDNTM0NzIiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJodHRwczovL3d3dy5kYXRhY2Fwc3lzdGVtcy5jb20vIiwiYXVkIjoiaHR0cHM6Ly93d3cud29ybGRwYXkuY29tLyIsImV4cCI6MTU2NTExNzE5NSwiYm9keWhhc2giOiI2MDQwM2JiZDYzNTU0OWFmMzg4NjFkN2UyNzM5MTMyOWM4ZTQzMzk1N2JhMWUwZWNhOTJjZWVkZDAyZjFmODIzIiwiaGFzaHR5cGUiOiJzaGEyNTYifQ.I2Aj_bMj9YCWeY56h-bC1uSLu4a9GUQIC3SwbYRnWSBsg_JiAaRrs6bK5iycyavXMA-9pUseBC6HurYGiIGYStwro7RZbrn83UGOnKFOBHLvB3mmQ4DQ4DP4dr1_JaZDpPvO0E9nwv4SBkjUZ-iVOuZ0b2Q2HrK0I5WJVIZouij_5GiHdkt97LoHZODQPkV30jNeVdj-52kwC5HjKvgrGAa6WDchRDzaY6JjwAcngVZy9rhclf23ajj0GmPOcWjfviakh8zZZK7BANZnpQ8NvsxBVWlhIwnFVHglqKA9Jvz5aASIlS-QGz4M0kCehsPLeQXupvzekjFe6Rxz1zFJKw
Accept: application/json, text/json, text/x-json, text/javascript, application/xml, text/xml
User-Agent: RestSharp/106.6.10.0
Connection: Keep-Alive
Accept-Encoding: gzip, deflate
Content-Type: application/json
Content-Length: 750
Host: localhost:44346

"{\"ISO8583_BASE64\":\"AhkwMjAw8jxEgQCAgCAAAAAAAAAAIjE2NTQ5OTk5MDEyMzQ1Njc4MTAwMzAwMDAwMDAwMDAwMDAyODA4MDExODQ2MTAwMDAwNDUxMjQ2MTAwODAxMjAwNTU4MTIwMTEwMDA2MDIzNDAwMTQ2NjMgICA4NDAwMTIwMDAwMDEwMDAwMDEwMTUyMTExMDE2NTQwMDExMDEwMDAzODREAAQAAAAAADMyYzBmM2VlOTA1ODU1NDMzNzg2ZGE0MTdmZTFiNDNjNTcxMTAwMzM1MjE0QWRkaXRpb25hbERhdGExMTAyMTFFeHRlcm5hbFRJRDE4MTAxMzI2NjUyMThHbG9iYWxQT1NFbnRyeU1vZGUyMTIyMTAxMDA3MDAwMDAyMjBHbG9iYWxQcm9jZXNzaW5nQ29kZTE2MDAzMDAwMjE4R2xvYmFsVGVybWluYWxUeXBlMTNEQzIxNkxhbmVJRDEwMjEwTWFya2V0RGF0YTIxM2FGdDAwMDAwMDAwMDEyMThQdXJjaGFzaW5nQ2FyZERhdGEyOTA8P3htbCB2ZXJzaW9uPSIxLjAiIGVuY29kaW5nPSJ1dGYtOCI/PjxQdXJjaGFzaW5nQ2FyZERhdGE+PENvbnRhY3QgLz48L1B1cmNoYXNpbmdDYXJkRGF0YT4yMTdUcmFuc2FjdGlvblN0YXR1czExMDIxN1Zpc2FFQ29tbUdvb2RzSW5kMTA=\"}123"

HTTP/1.1 400 Bad Request
Content-Type: application/problem+json; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Tue, 06 Aug 2019 18:46:36 GMT
Content-Length: 144

{"type":"https://tools.ietf.org/html/rfc7231#section-6.5.1","title":"Bad Request","status":400,"traceId":"80000002-0002-fd00-b63f-84710c7967bb"}