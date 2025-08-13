using System.Collections.Generic;

namespace VineyardApi.Domain.Content;

public sealed class PageContent
{
    public List<ContentBlock> Blocks { get; init; } = new();
}

public abstract class ContentBlock
{
    public string Type { get; init; } = default!;
}

public sealed class RichTextBlock : ContentBlock
{
    public string Html { get; init; } = string.Empty;
}

public sealed class ImageBlock : ContentBlock
{
    public string Url { get; init; } = string.Empty;
    public string Alt { get; init; } = string.Empty;
}
