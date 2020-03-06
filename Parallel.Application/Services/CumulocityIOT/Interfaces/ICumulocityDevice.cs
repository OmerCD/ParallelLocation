namespace Parallel.Application.Services.CumulocityIOT.Interfaces
{
    public interface ICumulocityDevice
    {
        string ClientId { get; set; }
        string DeviceName { get; set; }
        string User { get; set; }
        string Password { get; set; }
    }
}