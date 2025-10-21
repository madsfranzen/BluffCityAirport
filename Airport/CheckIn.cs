using System.Text;
using RabbitMQ.Client;

class CheckIn
{
    public static async Task Run()
    {

        string json1 = """
            {
              "Flight": {
                "@attributes": {
                  "number": "SK123",
                  "Flightdate": "20250301"
                },
                "Origin": "COPENHAGEN",
                "Destination": "PARIS"
              },
              "Passenger": [
                {
                  "ReservationNumber": "CA123250301001",
                  "FirstName": "Lars",
                  "LastName": "Hansen"
                },
                {
                  "ReservationNumber": "CA123250301002",
                  "FirstName": "Maria",
                  "LastName": "Hansen"
                },
                {
                  "ReservationNumber": "CA123250301003",
                  "FirstName": "Peter",
                  "LastName": "Jensen"
                }
              ],
              "Luggage": [
                { "Id": "CA123250301001", "Identification": 1, "Category": "Normal", "Weight": "18.5" },
                { "Id": "CA123250301002", "Identification": 1, "Category": "Normal", "Weight": "16.8" },
                { "Id": "CA123250301002", "Identification": 2, "Category": "Handbag", "Weight": "7.1" },
                { "Id": "CA123250301003", "Identification": 1, "Category": "Large", "Weight": "23.0" }
              ]
            }
            """;

        string json2 = """
{
  "Flight": {
    "@attributes": {
      "number": "SK791",
      "Flightdate": "20250302"
    },
    "Origin": "STOCKHOLM",
    "Destination": "NEW YORK"
  },
  "Passenger": [
    { "ReservationNumber": "CA791250302100", "FirstName": "Oskar", "LastName": "Nilsson" },
    { "ReservationNumber": "CA791250302101", "FirstName": "Sara", "LastName": "Nilsson" },
    { "ReservationNumber": "CA791250302102", "FirstName": "Erik", "LastName": "Lind" },
    { "ReservationNumber": "CA791250302103", "FirstName": "Anna", "LastName": "Berg" }
  ],
  "Luggage": [
    { "Id": "CA791250302100", "Identification": 1, "Category": "Large", "Weight": "25.3" },
    { "Id": "CA791250302101", "Identification": 1, "Category": "Normal", "Weight": "18.9" },
    { "Id": "CA791250302102", "Identification": 1, "Category": "Large", "Weight": "26.1" },
    { "Id": "CA791250302103", "Identification": 1, "Category": "Normal", "Weight": "19.4" },
    { "Id": "CA791250302103", "Identification": 2, "Category": "Handbag", "Weight": "6.0" }
  ]
}
""";

        string json3 = """
{
  "Flight": {
    "@attributes": {
      "number": "SK654",
      "Flightdate": "20250303"
    },
    "Origin": "OSLO",
    "Destination": "BERLIN"
  },
  "Passenger": [
    { "ReservationNumber": "CA654250303020", "FirstName": "Jonas", "LastName": "Bakken" },
    { "ReservationNumber": "CA654250303021", "FirstName": "Kari", "LastName": "Bakken" }
  ],
  "Luggage": [
    { "Id": "CA654250303020", "Identification": 1, "Category": "Normal", "Weight": "17.8" },
    { "Id": "CA654250303021", "Identification": 1, "Category": "Normal", "Weight": "19.0" },
    { "Id": "CA654250303021", "Identification": 2, "Category": "Handbag", "Weight": "6.5" }
  ]
}
""";

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Logger.LogInfo(channel, "CheckIn", "warn", "Initializing Check-In...");

        await Task.Delay(2000);

        await channel.QueueDeclareAsync(
            queue: "checkin_queue",
            durable: false,
            exclusive: false,
            autoDelete: false
        );

        var body = Encoding.UTF8.GetBytes(json1);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "checkin_queue",
            body: body
        );

        body = Encoding.UTF8.GetBytes(json2);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "checkin_queue",
            body: body
        );

        body = Encoding.UTF8.GetBytes(json3);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "checkin_queue",
            body: body
        );

        Logger.LogInfo(channel, "CheckIn", "Info", "Succesfully delivered JSON to CheckInQueue");

    }
}
