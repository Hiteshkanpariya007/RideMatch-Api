using Common.Enums;

namespace Data.Entities
{
    public class Ride
    {
        public Guid Id { get; set; }
        public Guid RiderId { get; set; }
        public Guid? DriverId { get; set; }
        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }
        public decimal DestinationLatitude { get; set; }
        public decimal DestinationLongitude { get; set; }
        public RideStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public decimal? Fare { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public Rider Rider { get; set; } = null!;
        public Driver? Driver { get; set; }
    }
}
