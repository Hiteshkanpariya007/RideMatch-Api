namespace Common.Constants
{
    public sealed class MatchingOptions
    {
        public const string SectionName = "Matching";

        public double InitialRadiusKm { get; set; } = 5;
        public double RadiusIncrementKm { get; set; } = 5;
        public double MaxRadiusKm { get; set; } = 20;
        public int MaxCandidates { get; set; } = 10;
        public int DriverLocationExpirySeconds { get; set; } = 30;
    }
}
