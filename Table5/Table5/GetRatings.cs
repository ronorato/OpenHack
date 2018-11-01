using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Table5
{
    public static class GetRatings
    {
        private static string BaseUrl = @"https://serverlessohuser.trafficmanager.net/api/GetUser";
        private static string GetUserUri(Guid userId) => $"GetUser?userId={userId}";

        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            // parse query parameter
            string userid = req.Query["userid"];

            if (userid == null) { return new NotFoundResult(); }

            var userguid = Guid.Parse(userid);

            if (!await ValidateUser(userguid)) return new NotFoundResult();

            var result = (new Repository()).GetRatingsForUser(userguid);
            return new OkObjectResult(result);
        }

        private static Uri GetUserUrl(Guid userId)
        {
            return new Uri(BaseUrl + "?userId=" + userId);
        }

        private static async Task<bool> ValidateUser(Guid userId)
        {
            bool valid = false;
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BaseUrl);
            HttpResponseMessage response = await httpClient.GetAsync(GetUserUri(userId));
            if (response.IsSuccessStatusCode)
            {
                GetProductsUser user = await response.Content.ReadAsAsync<GetProductsUser>();
                valid = user != null;
            }

            return valid;
        }

    }

    public class GetProductsUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
    }
}
