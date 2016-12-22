# Registration Key-based Authorization

Don deciphered the Formula used to authorize a Register DSC Agent request using a RegistrationKey, his sample code is below, Thank you Don!


Additionally, here's two additional examples based on the OSS Linux LCM:
* https://github.com/Microsoft/PowerShell-DSC-for-Linux/blob/master/LCM/scripts/RegisterHelper.sh
* https://github.com/Microsoft/PowerShell-DSC-for-Linux/blob/master/Providers/nxOMSAutomationWorker/automationworker/scripts/register.py


Here's Don's code excerpt:

```csharp
                    /*

                        Authorization HTTP header must match the HTTP body,
                        passed through a SHA-256 digest hash, 
                        encoded in Base64.
                        That then gets a newline, 
                        the x-ms-date HTTP header from the request, 
                        and is then run through a 
                        SHA-256 digest hash that uses a known RegistrationKey as an HMAC, 
                        with the result Base64 encoded.

                        This essentially is a digital signature and proof that the node knows a shared secret registration key.

                        So the received header is Authorization: Shared xxxxxxx\r\n

                    */
                    logger.LogInformation("\n\n\nPUT: Node registration");
                    string AgentId = context.GetRouteData().Values["AgentId"].ToString();
                    string Body = new StreamReader(context.Request.Body).ReadToEnd();
                    var Headers = context.Request.Headers;
                    logger.LogDebug("AgentId {AgentId}, Request Body {Body}, Headers {Headers}",AgentId,Body,Headers);

                    // get needed headers
                    // TODO: should check for these existing rather than assuming, fail if they don't
                    string xmsdate = context.Request.Headers["x-ms-date"];
                    string authorization = context.Request.Headers["Authorization"];
                    logger.LogDebug("x-ms-date {date}, Authorization {auth}",xmsdate,authorization);

                    // create signature, part 1
                    // this is the request body, hashed, then combined with the x-ms-date header
                    string contentHash = "";
                    using(var sha256 = SHA256.Create()) {
                        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Body));
                        contentHash = Convert.ToBase64String(hashedBytes);
                        //contentHash = BitConverter.ToString(hashedBytes).Replace("-","");
                    }
                    logger.LogDebug("Created content hash {hash}",contentHash);
                    string stringToSign = String.Format("{0}\n{1}", contentHash, xmsdate);
                    logger.LogDebug("String to sign is {sign}",stringToSign);

                    // HACK - we need to run a command to get the allowed registration keys
                    // and then compare each one
                    string[] registrationKeys = {"91E51A37-B59F-11E5-9C04-14109FD663AE"};
                    string result = Runner.Run("Get-Process");
                    logger.LogDebug(result);

                    // go through valid registration keys and create a final signature
                    // we do this because we might have multiple registration keys, so we
                    // have to try them all until we find one that matches
                    bool Valid = false;
                    foreach (string key in registrationKeys) {
                        logger.LogDebug("Trying registration key {key}",key);

                        // convert string key to Base64
                        byte[] byt = Encoding.UTF8.GetBytes(key);
                        string base64key = Convert.ToBase64String(byt);
    
                        // create HMAC signature using this registration key
                        var secretKeyBase64ByteArray = Convert.FromBase64String(base64key);
                        string signature = "";
                        using ( HMACSHA256 hmac = new HMACSHA256(secretKeyBase64ByteArray)) {
                            byte[] authenticationKeyBytes = Encoding.UTF8.GetBytes(stringToSign);
                            byte[] authenticationHash = hmac.ComputeHash(authenticationKeyBytes);
                            signature = Convert.ToBase64String(authenticationHash);
                        }
            
                        // compare what node sent to what we made
                        string AuthToMatch = authorization.Replace("Shared ","");
                        logger.LogDebug("Comparing keys:\nRcvd {0} \nMade {1}", AuthToMatch, signature );
                        if (AuthToMatch == signature) {
                            logger.LogDebug("Node is authorized");
                            Valid = true;
                            break;
                        }
                    }
            
                    // Because this is a PUT, we're only expected to return an HTTP status code
                    // TODO we also need to call Set-TugNodeRegistration if the node was valid
                    if (Valid) {
                        // TODO return HTTP 200
                        return context.Response.WriteAsync($"Registering node {AgentId}");
                    } else {
                        // TODO return HTTP 404(? check spec)
                        return context.Response.WriteAsync($"Registering node {AgentId}");
                    }
            
                    /*
                       TODO: Run Register-TugNode to register this node, padding node details
                             as paramaters. Note that the command must deal with duplicate
                             registrations (e.g., update data or ignore or whatever)
                    */
```

Finally, here's a PS fragment that re-implements the logic and tests it against a known set of observed values:

```PowerShell
## This fragment was used to confirm the RegKey Formula based on
## Wireshark packet captures of current LCM + xDscWebService

## This is the mutually shared RegKey
$regKey = "f65e1a0c-46b0-424c-a6a5-c3701aef32e5"

## This is what we expect and what was sent by the LCM client
$authzHeaderValue = "Shared SM095lQD5iEVzrToxnyuuoDAYfX2zA23YoZsZlZDyFU="
$xmsdateHeaderValue = "2016-12-21T23:43:48.4718366Z"
$requestBody = '{"AgentInformation":{"LCMVersion":"2.0","NodeName":"EC2AMAZ-VT1I874","IPAddress":"10.50.1.9;127.0.0.1;fe80::288e:6e98:1555:55e9%6;::2000:0:0:0;::1;::2000:0:0:0"},"ConfigurationNames":["ClientConfig2"],"RegistrationInformation":{"CertificateInformation":{"FriendlyName":"DSC-OaaS Client Authentication","Issuer":"CN=http://10.50.1.5:8080/PSDSCPullServer.svc","NotAfter":"2017-12-21T11:40:36.0000000-05:00","NotBefore":"2016-12-21T16:30:36.0000000-05:00","Subject":"CN=http://10.50.1.5:8080/PSDSCPullServer.svc","PublicKey":"U3lzdGVtLlNlY3VyaXR5LkNyeXB0b2dyYXBoeS5YNTA5Q2VydGlmaWNhdGVzLlB1YmxpY0tleQ==","Thumbprint":"AC5849ACDB6DD19FD79B6ACA2D077E71CEE31C4F","Version":3},"RegistrationMessageType":"ConfigurationRepository"}}'

#"[$requestBody]"

## Here's the formula at work
$macKey = [System.Text.Encoding]::UTF8.GetBytes($regKey)

$sha = [System.Security.Cryptography.SHA256]::Create()
$mac = New-Object System.Security.Cryptography.HMACSHA256 -ArgumentList @(,$macKey)

$arr = [System.Text.Encoding]::UTF8.GetBytes($requestBody)
$dig = $sha.ComputeHash($arr)
$digB64 = [System.Convert]::ToBase64String($dig)
$concat = "$digB64`n$xmsdateHeaderValue"
$macSig = $mac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($concat))
$sigB64 = [Convert]::ToBase64String($macSig)

## This is the sig we computed
$sigB64

## Compare
$sigB64 -eq ($authzHeaderValue -replace 'Shared ','')
```
