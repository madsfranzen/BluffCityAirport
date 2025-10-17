namespace Schemas
{

    public class Luggage
    {
        public required string Id { get; set; }
        public required int Identification { get; set; }
        public int TotalCorrelation { get; set; } = 0;
        public required string Category { get; set; }
        public required string Weight { get; set; }
        public Flight? Flight { get; set; }

        public override string ToString()
        {
            return ($"Luggage [Id={Id}, Identification={Identification}, Category={Category}, Weight={Weight}, TotalCorrelation={TotalCorrelation}, Flight={(Flight != null ? Flight.ToString() : "None")}]");
        }
    }

}
