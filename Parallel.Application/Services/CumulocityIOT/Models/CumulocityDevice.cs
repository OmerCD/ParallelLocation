using Parallel.Application.Services.CumulocityIOT.Interfaces;

namespace Parallel.Application.Services.CumulocityIOT.Models
{
    public class CumulocityDevice : ICumulocityDevice
    {
        public string ClientId { get; set; }
        public string DeviceName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}