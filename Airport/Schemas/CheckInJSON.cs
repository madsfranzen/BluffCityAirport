using System.Text.Json.Serialization;

public class CheckInJSON
{
    public required Flight Flight { get; set; }
    public required List<Passenger> Passenger { get; set; }
    public required List<Luggage> Luggage { get; set; }
}

public class Flight
{
    [JsonPropertyName("@attributes")]
    public required FlightAttributes Attributes { get; set; }

    public required string Origin { get; set; }
    public required string Destination { get; set; }
}

public class FlightAttributes
{
    public required string number { get; set; }
    public required string Flightdate { get; set; }
}

public class Passenger
{
    public required string ReservationNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}

public class Luggage
{
    public required string Id { get; set; }
    public required string Identification { get; set; }
    public required string Category { get; set; }
    public required string Weight { get; set; }
}
