using VineyardApi.Domain.Content;

namespace VineyardApi.Models
{
    public record PageVersionContentResponse(PageContent ContentJson, int VersionNo);
}
