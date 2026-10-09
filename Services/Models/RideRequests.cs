namespace Services.Models;

public sealed class CreateRideRequest
{
    public decimal PickupLatitude { get; set; }
    public decimal PickupLongitude { get; set; }
    public decimal DestinationLatitude { get; set; }
    public decimal DestinationLongitude { get; set; }
}

public sealed class LocationRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

public sealed class RideResponse
{
    public Guid Id { get; set; }
    public Guid RiderId { get; set; }
    public Guid? DriverId { get; set; }
    public string Status { get; set; }
    public decimal PickupLatitude { get; set; }
    public decimal PickupLongitude { get; set; }
    public decimal DestinationLatitude { get; set; }
    public decimal DestinationLongitude { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal? Fare { get; set; }
}

public sealed class DriverStatusResponse
{
    public Guid DriverId { get; set; }
    public bool IsOnline { get; set; }
    public bool IsAvailable { get ;set;}
    public decimal? CurrentLatitude { get ;set;}
    public decimal? CurrentLongitude { get ;set;}
    public DateTime? LastLocationUpdatedAt { get ;set;}
}

public sealed class PagedResponse<T>
{
    public int Page { get ;set;}
    public int PageSize { get ;set;}
    public int TotalCount { get ;set;}
    public int TotalPages { get ;set;}
    public IReadOnlyList<T> Items { get ;set;}
}
