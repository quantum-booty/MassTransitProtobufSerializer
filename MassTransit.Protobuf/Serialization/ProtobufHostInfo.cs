namespace MassTransit.Serialization;

public class ProtobufHostInfo : HostInfo
{
    public string? MachineName { set; get; }
    public string? ProcessName { set; get; }
    public int ProcessId { set; get; }
    public string? Assembly { set; get; }
    public string? AssemblyVersion { set; get; }
    public string? FrameworkVersion { set; get; }
    public string? MassTransitVersion { set; get; }
    public string? OperatingSystemVersion { set; get; }
}