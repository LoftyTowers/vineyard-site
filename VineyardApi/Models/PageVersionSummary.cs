using System;

namespace VineyardApi.Models
{
    public record PageVersionSummary(Guid Id, int VersionNo, DateTime? PublishedUtc, string? ChangeNote);
}
