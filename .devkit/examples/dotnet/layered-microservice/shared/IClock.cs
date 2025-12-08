using System;

namespace LayeredMicroservice.Shared;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
