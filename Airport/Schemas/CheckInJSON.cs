using System.Text.Json.Serialization;
using System.Text.Json;

namespace Schemas
{
    public class CheckInJSON
    {
        public required Flight Flight { get; set; } = null!;
        public List<Passenger> Passenger { get; set; } = null!;
        public List<Luggage> Luggage { get; set; } = null!;

        public CheckInJSON() { }

        [JsonConstructor]
        public CheckInJSON(Flight flight, List<Passenger> passenger, List<Luggage> luggage)
        {
            Flight = flight;
            Passenger = passenger;
            Luggage = luggage;
        }
        public override string ToString()
        {
            var passengersJson = Passenger != null
                ? JsonSerializer.Serialize(Passenger, new JsonSerializerOptions { WriteIndented = true })
                : "[]";

            var luggageJson = Luggage != null
                ? JsonSerializer.Serialize(Luggage, new JsonSerializerOptions { WriteIndented = true })
                : "[]";

            return $"Flight: {Flight}\nPassengers: {passengersJson}\nLuggage: {luggageJson}";
        }
    }

}
