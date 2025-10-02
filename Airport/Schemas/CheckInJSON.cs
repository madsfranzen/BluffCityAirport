using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class CheckInJSON
{
    public Flight Flight { get; set; }
    public List<Passenger> Passenger { get; set; }
    public List<Luggage> Luggage { get; set; }
}

public class Flight
{
    [JsonPropertyName("@attributes")]
    public FlightAttributes Attributes { get; set; }

    public string Origin { get; set; }
    public string Destination { get; set; }
}

public class FlightAttributes
{
    public string? number { get; set; }
    public string? Flightdate { get; set; }
}

public class Passenger
{
    public string ReservationNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Luggage
{
    public string Id { get; set; }
    public string Identification { get; set; }
    public string Category { get; set; }
    public string Weight { get; set; }
}
