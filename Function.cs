using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using dotAPNS; // Install-Package dotAPNS
using Newtonsoft.Json; // Install-Package Newtonsoft.Json
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ApnsHttp2Serverless
{
    public class Functions
    {
        /// <summary>
        /// A Lambda function to respond to HTTP GET and POST methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse HttpRequest(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine(request.Body);
            var jRequest = JObject.Parse(request.Body);
            var certificate = jRequest["certificate"].ToObject<byte[]>();
            var certPassword = jRequest["certPassword"].ToObject<string>();
            var message = jRequest["message"].ToObject<string>();
            var destination = jRequest["destination"].ToObject<string>();
            var sendResult = Send(certificate, certPassword, message, destination);
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(sendResult,Formatting.Indented),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
            return response;
        }

       
        private ApnsResponse Send(
            byte[] certificate, 
            string certPassword, 
            string message, 
            string destination)
        {
            var x509 = new X509Certificate2(certificate, certPassword);
            var applePushNotificationService = ApnsClient.CreateUsingCert(x509);
            var push = new ApplePush(ApplePushType.Alert)
                .AddAlert(message)
                .AddToken(destination);
            return applePushNotificationService.Send(push).Result;
        }
    }
}
