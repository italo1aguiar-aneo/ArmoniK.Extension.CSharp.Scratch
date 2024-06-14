namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public record ApplicationConfiguration
{
    public string ApplicationName { get; init; }
    public string ApplicationVersion { get; init; }
    public string ApplicationNamespace { get; init; }
    public string ApplicationService { get; init; }
    public string EngineType { get; init; }
}