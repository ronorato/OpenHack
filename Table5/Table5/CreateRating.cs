using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Table5
{
    public static class CreateRating
    {
        public const string BaseUri = "https://serverlessohproduct.trafficmanager.net/api/GetProduct";

        private static string GetProductUri(Guid productId) =>
            $"GetProduct?productId={productId}";

        [FunctionName("CreateRating")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            var request = await req.Content.ReadAsAsync<PostRatingRequest>();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BaseUri);

            //validate the product exists
            var getProductResponse = await httpClient.GetAsync(GetProductUri(request.ProductId));
            if(!getProductResponse.IsSuccessStatusCode)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, new ArgumentException("the provided product id is invalid or does not exist", nameof(request.ProductId)));
            }
            
            if (!IsRatingValid(request)) return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Rating must be between 0 and 5");
            if (request.UserId == default(Guid)) return req.CreateErrorResponse(HttpStatusCode.BadRequest, "UserId must be provided");
            if (!await IsValidUser(request)) return req.CreateErrorResponse(HttpStatusCode.BadRequest, "User is not valid");
            var rating = new RatingResponse
            {
                Id = Guid.NewGuid(),
                LocationName = request.LocationName,
                ProductId = request.ProductId,
                Rating = request.Rating,
                UserId = request.UserId,
                UserNotes = request.UserNotes,
                // TODO set this to the timestamp in the data store
                TimeStamp = DateTime.UtcNow
            };

            await new Repository().AddRating(rating);

            return req.CreateResponse(HttpStatusCode.Created, rating);
        }
        private static async Task<bool> IsValidUser(PostRatingRequest request)
        {
            var client = new HttpClient {BaseAddress = new Uri("https://serverlessohuser.trafficmanager.net/api/")};
            return (await client.GetAsync($"GetUser?userId={request.UserId}")).StatusCode == HttpStatusCode.OK;
        }
        private static bool IsRatingValid(PostRatingRequest request) => request.Rating > 0 && request.Rating <= 5;
    }
}