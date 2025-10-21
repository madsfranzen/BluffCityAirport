using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Schemas;

class Aggregator
{

    private static IChannel? channel;
    private static List<CheckInJSON> fullObjects = new List<CheckInJSON>();

    public static async Task Run()
    {
        // Setting up RabbitMQ
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync("aggregator_queue", false, false, false, null);
        var consumer = new AsyncEventingBasicConsumer(channel);

        Logger.LogInfo(channel, "aggregator", "Warn", "Aggregator Ready!");

        // This is a call to print the final JSON objects when we are done (currently simulated by a 25+5 second timer)
        _ = PrintFinalObject(fullObjects, channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            // We are delaying execution only for simulation and readability in logs - this can be omitted without worries
            await Task.Delay(250);

            // Parsing JSON Object
            var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            // Determine if we are dealing with flight+passenger message or luggage message and call appropriate method
            if (root.TryGetProperty("Flight", out var flightElement) && root.TryGetProperty("Passenger", out var passengerElement))
            {
                aggregateFlightAndPassengers(flightElement, passengerElement);
            }
            else if (root.TryGetProperty("flightNumber", out var flightNumberElement) && root.TryGetProperty("luggagesPackage", out var luggagePackageElement))
            {
                aggregateLuggages(flightNumberElement, luggagePackageElement);
            }

            await Task.Yield();
        };
        await channel.BasicConsumeAsync(queue: "aggregator_queue", autoAck: true, consumer: consumer);
        await Task.Delay(-1);
    }

    private static void aggregateFlightAndPassengers(JsonElement flightElement, JsonElement passengerElement)
    {
        try
        {
            var newFlight = flightElement.Deserialize<Flight>();
            var newPassenger = passengerElement.Deserialize<List<Passenger>>();

            if (!fullObjects.Any(c => c.Flight.Attributes.number == newFlight!.Attributes.number))
            {
                var newCheckIn = new CheckInJSON
                {
                    Flight = newFlight!,
                    Passenger = newPassenger ?? new List<Passenger>(),
                    Luggage = new List<Luggage>()
                };

                fullObjects.Add(newCheckIn);
            }
        }
        catch (Exception ex)
        {
            Logger.LogInfo(channel!, "aggregator", "error", $"Unexpected error: {ex}");
        }
    }

    private static void aggregateLuggages(JsonElement flightNumberElement, JsonElement luggagesPackageElement)
    {
        try
        {
            var flightNumber = flightNumberElement.GetString();
            var checkIn = fullObjects.Find(c => c.Flight.Attributes.number == flightNumber);
            var luggageItems = luggagesPackageElement.Deserialize<List<Luggage>>();
            checkIn!.Luggage.AddRange(luggageItems!);
        }
        catch (Exception ex)
        {
            Logger.LogInfo(channel!, "aggregator", "error", $"Unexpected error: {ex}");
        }
    }

    private static async Task PrintFinalObject(List<CheckInJSON> finalObject, IChannel channel)
    {
        await Task.Delay(25000);

        Logger.LogInfo(channel, "aggregator", "info", $"Check-In closes in 5 seconds...");
        await Task.Delay(1000);
        Logger.LogInfo(channel, "aggregator", "info", $"4...");
        await Task.Delay(1000);
        Logger.LogInfo(channel, "aggregator", "info", $"3...");
        await Task.Delay(1000);
        Logger.LogInfo(channel, "aggregator", "info", $"2...");
        await Task.Delay(1000);
        Logger.LogInfo(channel, "aggregator", "info", $"1...");
        await Task.Delay(1000);

        foreach (CheckInJSON checkIn in finalObject)
        {
            Logger.LogInfo(channel, "aggregator", "info", $"{checkIn}");
        }
    }
}
