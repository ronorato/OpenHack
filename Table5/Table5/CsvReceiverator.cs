// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace Table5
{
    public static class CsvReceiverator
    {
        [FunctionName("CsvReceiverator")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent, [OrchestrationClient]DurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            var url = ((string)((dynamic)eventGridEvent.Data).url);
            var id = url.Substring(url.LastIndexOf('/'), url.IndexOf('-'));
            log.LogInformation($"url is {url}; id is {id}");
            var instanceId = await starter.StartNewAsync("OrderComponentsWaiter", id, url);
            log.LogInformation($"instanceId is {instanceId}");
            var status = await starter.GetStatusAsync(instanceId);
            //log.LogInformation($"status is {status}");
        }
    }
    public class Silly
    {
        public string Url { get; set; }
    }
}
