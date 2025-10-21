using System.Text.Json.Serialization;

namespace Schemas
{
    public class Flight
    {
        [JsonPropertyName("@attributes")]
        public required FlightAttributes Attributes { get; set; }
        public required string Origin { get; set; }
        public required string Destination { get; set; }

        public override string ToString()
        {
            return $"Flight: {Attributes.number}, {Origin} -> {Destination}";
        }
    }
}
