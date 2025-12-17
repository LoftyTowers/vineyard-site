using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VineyardApi.Domain.Content;

public sealed class PageContent
{
    [JsonPropertyName("blocks")]
    public List<PageBlock> Blocks { get; init; } = new();
}

public sealed class PageBlock
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    // Holds the block payload (text, image object, people list, etc.)
    [JsonPropertyName("content")]
    public JsonElement Content { get; init; }
}
