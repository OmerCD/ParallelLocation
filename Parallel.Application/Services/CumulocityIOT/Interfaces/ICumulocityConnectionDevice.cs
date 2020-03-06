using Cumulocity.SDK.MQTT;
using Cumulocity.SDK.MQTT.Model;

namespace Parallel.Application.Services.CumulocityIOT.Interfaces
{
    internal interface ICumulocityConnectionDevice
    {
        ICumulocityDevice Device { get; set; }
        IMqttClient Client { get; set; }
    }
}