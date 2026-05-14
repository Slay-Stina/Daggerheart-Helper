using System.Text.Json.Serialization;

namespace DaggerheartHelper.Tests.Srd.Ingestion.Tests.Models;

public class TestInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public Address? Address { get; set; }
}

public class Address
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

