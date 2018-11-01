using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Table5
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            var ratingIdString = (string)((HttpRequest)req).Query["ratingId"] ?? await RatingIdFromBody(req);
            if (ratingIdString == null) 
                return new BadRequestErrorMessageResult("Please pass a ratingId on the query string or in the request body");
            if (!Guid.TryParse(ratingIdString, out Guid ratingId))
            {
                return new BadRequestErrorMessageResult("RatingId is not a valid GUID");
            }
            var rating = await new Repository().GetRating(ratingId);
            //var value = JsonConvert.SerializeObject(new RatingResponse
            //{
            //    Id = Guid.NewGuid(), LocationName = "Norway", ProductId = Guid.NewGuid(), Rating = 900, UserId = Guid.NewGuid(),
            //    UserNotes = "Mmm, delectable"
            //});
            return rating == default(RatingResponse) ? (IActionResult)new NotFoundResult() : new OkObjectResult(rating);
        }
        private static async Task<string> RatingIdFromBody(HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            return data?.name;
        }
    }
}