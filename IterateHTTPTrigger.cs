using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iterate.GPT
{
    public static class IterateHTTPTrigger
    {
        static readonly HttpClient client = new HttpClient();

        [FunctionName("IterateHTTPTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = new StringContent( //gpt-4-1106-preview, gpt-3.5-turbo
                "{ \"model\": \"gpt-3.5-turbo\", \"messages\": [{\"role\": \"system\", \"content\": \"Return only well linted Java code for the following prompt! No class or method definitions. No extra comments or print statements.\"}, {\"role\": \"user\", \"content\": \"" + req.Query["prompt"] + "\"}], \"max_tokens\": 200 }",
                Encoding.UTF8,
                "application/json"
            );

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Content = requestBody
            };

            request.Headers.Add("Authorization", "Bearer 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");
            
            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);
            var json_obj = JsonConvert.DeserializeObject<JObject>(responseBody);
            responseBody = json_obj["choices"][0]["message"]["content"].ToString();
            if (responseBody.Contains("```java\n")) {
                responseBody = responseBody.Split("```java\n")[1].Split("\n```")[0];
            } else if (responseBody.Contains("```\n")) {
                responseBody = responseBody.Split("```\n")[1].Split("\n```")[0];
            }
            return new OkObjectResult(responseBody);
        }
    }
}
 