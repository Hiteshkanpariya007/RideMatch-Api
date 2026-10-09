namespace Data.Entities
{
    public class DriverLocation
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime RecordedAt { get; set; }

        public Driver Driver { get; set; } = null!;
    }
}
