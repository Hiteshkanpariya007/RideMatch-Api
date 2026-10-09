namespace Data.Entities
{
    public class Driver
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public User User { get; set; } = null!;
        public ICollection<Ride> Rides { get; set; } = new List<Ride>();
        public ICollection<DriverLocation> Locations { get; set; } = new List<DriverLocation>();
    }
}
