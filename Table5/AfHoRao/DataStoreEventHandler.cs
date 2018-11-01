// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace AfHoRao
{
    public static class DataStoreEventHandler
    {
        [FunctionName("DataStoreEventHandler")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent, [OrchestrationClient]DurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            var url = ((string)((dynamic)eventGridEvent.Data).url);
            var start = url.LastIndexOf('/') + 1;
            var end = url.LastIndexOf('-');
            var id = url.Substring(start, end - start);
            log.LogInformation($"{url}; Start: {start} - End: {end} - Id: {id}");
            var instanceId = await starter.StartNewAsync("OrderProcessor", id, url);
            log.LogInformation($"instanceId is {instanceId}");
        }
    }
}
