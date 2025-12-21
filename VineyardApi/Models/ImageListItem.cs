using System;

namespace VineyardApi.Models
{
    public class ImageListItem
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string? Alt { get; set; }
        public string? Caption { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
