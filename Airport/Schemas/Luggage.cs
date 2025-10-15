namespace Schemas
{

    public class Luggage
    {
        public required string Id { get; set; }
        public required string Identification { get; set; }
        public int TotalCorrelation { get; set; } = 0;
        public required string Category { get; set; }
        public required string Weight { get; set; }
        public Flight? Flight { get; set; }
    }
}
