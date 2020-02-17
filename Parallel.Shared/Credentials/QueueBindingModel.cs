using Newtonsoft.Json;

namespace Parallel.Shared.Credentials
{
    public class QueueBindingModel
    {
        // { 
        //     "source":"Receiving",
        //     "vhost":"/",
        //     "destination":"Offline",
        //     "destination_type":"queue",
        //     "routing_key":"OfflineRoute",
        //     "arguments":{ 
        //
        //     },
        //     "properties_key":"OfflineRoute"
        // }

        [JsonProperty("source")] public string Source { get; set; }
        [JsonProperty("vhost")] public string Vhost { get; set; }
        [JsonProperty("destination")] public string Destination { get; set; }
        [JsonProperty("destination_type")] public string DestinationType { get; set; }
        [JsonProperty("routing_key")] public string RoutingKey { get; set; }
        [JsonProperty("arguments")] public object Arguments { get; set; }
        [JsonProperty("properties_key")] public string PropertiesKey { get; set; }
    }
}