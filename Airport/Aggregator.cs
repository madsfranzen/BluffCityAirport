using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Schemas;

class Aggregator
{

    private static async Task PrintFinalObject(List<CheckInJSON> finalObject, IChannel channel)
    {
        await Task.Delay(20000);

        Logger.LogInfo(channel, "aggregator", "info", $"Check-In closes in 5 seconds!");
        await Task.Delay(5000);

        foreach (CheckInJSON checkIn in finalObject)
        {
            Logger.LogInfo(channel, "aggregator", "info", $"{checkIn}");
        }
    }

    public static async Task Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Logger.LogInfo(channel, "aggregator", "Warn", "Aggregator Ready!");

        await channel.QueueDeclareAsync("aggregator_queue", false, false, false, null);

        List<CheckInJSON> fullObjects = new List<CheckInJSON>();

        var consumer = new AsyncEventingBasicConsumer(channel);

        _ = PrintFinalObject(fullObjects, channel); // fire-and-forget

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                await Task.Delay(250);

                var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());

                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("Flight", out var flightElement))
                {
                    root.TryGetProperty("Passenger", out var passengerElement);
                    var newFlight = flightElement.Deserialize<Flight>();
                    var newPassenger = passengerElement.Deserialize<List<Passenger>>();

                    if (newFlight != null && !fullObjects.Any(c => c.Flight.Attributes.number == newFlight.Attributes.number))
                    {
                        var newCheckIn = new CheckInJSON
                        {
                            Flight = newFlight,
                            Passenger = newPassenger ?? new List<Passenger>(),
                            Luggage = new List<Luggage>()
                        };

                        fullObjects.Add(newCheckIn);
                    }
                }
                else if (root.TryGetProperty("flightNumber", out var flightNumberElement))
                {
                    var flightNumber = flightNumberElement.GetString();

                    if (flightNumber == null)
                    {
                        Logger.LogInfo(channel, "aggregator", "warn", $"flightNumber in message is null!");
                        return;
                    }

                    var checkIn = fullObjects.Find(c => c.Flight.Attributes.number == flightNumber);

                    if (checkIn == null)
                    {
                        Logger.LogInfo(channel, "aggregator", "warn", $"Flight {flightNumber} not found in fullObjects!");
                        return;
                    }

                    if (root.TryGetProperty("luggagesPackage", out var luggagesPackageElement))
                    {
                        var luggageItems = luggagesPackageElement.Deserialize<List<Luggage>>();

                        if (luggageItems != null)
                        {
                            checkIn.Luggage.AddRange(luggageItems);
                        }
                        else
                        {
                            Logger.LogInfo(channel, "aggregator", "warn", $"luggagesPackage is empty or invalid for flight {flightNumber}");
                        }
                    }
                    else
                    {
                        Logger.LogInfo(channel, "aggregator", "warn", $"No luggagesPackage found in message for flight {flightNumber}");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(channel, "aggregator", "error", $"Error: {ex}");
            }

            await Task.Yield();
        };
        await channel.BasicConsumeAsync(queue: "aggregator_queue", autoAck: true, consumer: consumer);
        await Task.Delay(-1);
    }
}
