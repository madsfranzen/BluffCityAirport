using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Schemas;

class Splitter
{
    public static async Task Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Logger.LogInfo(channel, "splitter", "info", "Splitter Ready! Awaiting input...");

        await channel.QueueDeclareAsync("checkin_queue", false, false, false, null);
        await channel.QueueDeclareAsync("scrambler_queue", false, false, false, null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                await Task.Delay(250);

                var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
                var checkInData = JsonSerializer.Deserialize<CheckInJSON>(jsonString);

                if (checkInData == null)
                {
                    Logger.LogInfo(channel, "splitter", "error", "Unexpected error! Checkin-JSON might be null!");
                    return;
                }

                Logger.LogInfo(channel, "splitter", "info", $"Received {checkInData.Luggage.Count} luggage items.");

                // STEP 1: Build luggage correlation map
                var luggageMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var lug in checkInData.Luggage)
                {
                    luggageMap[lug.Id] = luggageMap.GetValueOrDefault(lug.Id) + 1;
                }

                // STEP 2: Enrich and prepare
                var enrichedLuggage = new List<Luggage>();
                foreach (var lug in checkInData.Luggage)
                {
                    lug.TotalCorrelation = luggageMap[lug.Id];
                    lug.Flight = checkInData.Flight;
                    enrichedLuggage.Add(lug);
                }

                // STEP 3: Delegate sending
                await PublishLuggageBatch(channel, enrichedLuggage);

                Logger.LogInfo(channel, "splitter", "info", $"Done splitting {checkInData.Luggage.Count} luggages. All sent off to scrambler_queue!");
            }
            catch (Exception ex)
            {
                Logger.LogInfo(channel, "splitter", "error", $"Error in splitter: {ex}");
            }

            await Task.Yield();
        };

        await channel.BasicConsumeAsync(queue: "checkin_queue", autoAck: true, consumer: consumer);
        await Task.Delay(-1);
    }

    private static async Task PublishLuggageBatch(IChannel channel, List<Luggage> luggageBatch)
    {
        foreach (var lug in luggageBatch)
        {
            try
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(lug);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: "scrambler_queue",
                    body: body
                );

                Logger.LogInfo(channel, "splitter", "info", $"Sent luggage {lug.Id} to scrambler_queue");
            }
            catch (Exception ex)
            {
                Logger.LogInfo(channel, "splitter", "error", $"Failed to send luggage {lug.Id}: {ex.Message}");
            }

            // Optional: small delay to avoid message burst
            await Task.Delay(50);
        }
    }
}

