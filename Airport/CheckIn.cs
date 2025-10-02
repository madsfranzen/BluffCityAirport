using System.Text;
using RabbitMQ.Client;

class CheckIn
{
    public static async Task Run()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[CheckIn] Check In Started...");

	    await Task.Delay(2000);

        string json = """
            	{
            	  "Flight": {
            		"@attributes": {
            		  "number": "SK937",
            		  "Flightdate": "20220225"
            		},
            		"Origin": "ARLANDA",
            		"Destination": "LONDON"
            	  },
            	  "Passenger": [
            		{
            		  "ReservationNumber": "CA937200305252",
            		  "FirstName": "Anders",
            		  "LastName": "And"
            		},
            		{
            		  "ReservationNumber": "CA937200305253",
            		  "FirstName": "Andersine",
            		  "LastName": "And"
            		}
            	  ],
            	  "Luggage": [
            		{
            		  "Id": "CA937200305252",
            		  "Identification": "1",
            		  "Category": "Normal",
            		  "Weight": "17.3"
            		},
            		{
            		  "Id": "CA937200305252",
            		  "Identification": "2",
            		  "Category": "Large",
            		  "Weight": "22.7"
            		},
            		{
            		  "Id": "CA937200305253",
            		  "Identification": "1",
            		  "Category": "Large",
            		  "Weight": "24.2"
            		},
            		{
            		  "Id": "CA937200305253",
            		  "Identification": "2",
            		  "Category": "Large",
            		  "Weight": "21.4"
            		}
            	  ]
            	}
            """;

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "checkin_queue",
            durable: false,
            exclusive: false,
            autoDelete: false
        );

        var body = Encoding.UTF8.GetBytes(json);

        // await channel.BasicPublishAsync(
        //     exchange: string.Empty,
        //     routingKey: "checkin_queue",
        //     body: body
        // );

		Logger.LogInfo(channel, "CheckInn", "Info", "Succesfully delivered JSON to CheckInQueue");

    }
}
