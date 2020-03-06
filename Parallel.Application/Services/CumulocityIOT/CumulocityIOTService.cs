using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cumulocity.SDK.MQTT;
using Cumulocity.SDK.MQTT.Model;
using Cumulocity.SDK.MQTT.Model.ConnectionOptions;
using Cumulocity.SDK.MQTT.Model.MqttMessage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Parallel.Application.Services.CumulocityIOT.Interfaces;
using Parallel.Application.ValueObjects;

namespace Parallel.Application.Services.CumulocityIOT
{
    internal class DeviceRegistrationResult
    {
        public string Owner { get; set; }
        public DateTime CreationTime { get; set; }
        public string TenantId { get; set; }
        public string Self { get; set; }
        public string Id { get; set; }
        public string Status { get; set; }
    }
    public class CumulocityIOTService
    {
        private readonly ILogger<CumulocityIOTService> _logger;
        private readonly CumulocityInfo _cumulocityInfo;
        private readonly string _baseUrlPath;
        private readonly IDictionary<string, ICumulocityDevice> _deviceDictionary;
        private readonly HttpClient _httpClient;
        private const string DeviceRegisterPath = "/devicecontrol/newDeviceRequests";
        private const string BootstrapUser = "management/devicebootstrap";
        private const string Password = "Fhdt1bb1f";

        public CumulocityIOTService(ILogger<CumulocityIOTService> logger, CumulocityInfo cumulocityInfo,
            IEnumerable<ICumulocityDevice> devices)
        {
            _logger = logger;
            _cumulocityInfo = cumulocityInfo;
            _baseUrlPath = _cumulocityInfo.ServerUrl;
            _httpClient = new HttpClient();
            _deviceDictionary = (devices != null && devices.Any())
                ? devices.ToDictionary(x => x.ClientId)
                : new Dictionary<string, ICumulocityDevice>();
            
            _httpClient.BaseAddress = new Uri(_baseUrlPath);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    $"{_cumulocityInfo.UserName}:{_cumulocityInfo.Password}")));
            
        }

        public void RegisterDevices(IEnumerable<ICumulocityDevice> devices)
        {
        }

        private async Task<DeviceRegistrationResult> SendDeviceRegistrationRequest(ICumulocityDevice device)
        {
            var body = new StringContent("{\"id\":\"" + device.ClientId + "\"}");
            var result = await _httpClient.PostAsync(DeviceRegisterPath, body);
            return JsonConvert.DeserializeObject<DeviceRegistrationResult>(await result.Content.ReadAsStringAsync());
        }

        public async Task<IMqttClient> RegisterDevice(ICumulocityDevice device)
        {
            IConnectionDetails cDetails = new ConnectionDetailsBuilder()
                .WithClientId(device.ClientId)
                .WithHost(_baseUrlPath)
                .WithCredentials(BootstrapUser, Password)
                .WithCleanSession()
                .WithProtocol(TransportType.Tcp)
                .WithCleanSession()
                .Build();
            var client = new MqttClient(cDetails);
            await client.EstablishConnectionAsync();
            await client.SubscribeAsync(new MqttMessageRequest() {TopicName = "s/dcr"});
            client.MessageReceived+= (sender, response) =>
            {
                Console.WriteLine(response.MessageContent);
            };
            // var res =  await _httpClient.PostAsync("/s/ucr", new StringContent(""));
            await client.PublishAsync(new MqttMessageRequestBuilder()
                .WithTopicName("s/ucr")
                .WithQoS(QoS.EXACTLY_ONCE)
                .WithMessageContent(string.Empty)
                .Build());
            return null;
        }

        private void ClientOnConnected(object? sender, ClientConnectedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ClientOnConnectionFailed(object? sender, ProcessFailedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ClientOnMessageReceived(object? sender, IMqttMessageResponse e)
        {
            throw new System.NotImplementedException();
        }

        private IConnectionDetails CreateConnectionDetails(ICumulocityDevice device)
        {
            IConnectionDetails connectionDetails = new ConnectionDetailsBuilder()
                .WithClientId(device.ClientId)
                .WithHost(_cumulocityInfo.ServerUrl)
                .WithCredentials(device.User, device.Password)
                .WithCleanSession()
                .WithProtocol(TransportType.Tcp)
                .Build();
            return connectionDetails;
        }
    }
}