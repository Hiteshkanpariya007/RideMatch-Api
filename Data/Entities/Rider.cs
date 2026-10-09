namespace Data.Entities
{
    public class Rider
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Ride> Rides { get; set; } = new List<Ride>();
    }
}
