using System.Text.Json.Serialization;

namespace Schemas
{
    public class CheckInJSON
    {
        public required Flight Flight { get; set; } = null!;
        public required List<Passenger> Passenger { get; set; } = null!;
        public required List<Luggage> Luggage { get; set; } = null!;

        [JsonConstructor]
        public CheckInJSON(Flight flight, List<Passenger> passenger, List<Luggage> luggage)
        {
            Flight = flight;
            Passenger = passenger;
            Luggage = luggage;
        }
    }

}
