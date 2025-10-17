using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Schemas;

class Scrambler
{
    public static async Task Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Logger.LogInfo(channel, "scrambler", "info", "Scrambler Ready! Awaiting input...");

        await channel.QueueDeclareAsync("scrambler_queue", false, false, false, null);
        await channel.QueueDeclareAsync("resequencer_queue", false, false, false, null);

        var random = new Random();

        await Task.Delay(1000);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var luggage = JsonSerializer.Deserialize<Luggage>(jsonString);

                    if (luggage == null)
                    {
                        Logger.LogInfo(channel, "scrambler", "error", "Received null luggage object!");
                        return;
                    }

                    int delay = random.Next(1000, 10000);
                    await Task.Delay(delay);

                    var body = JsonSerializer.SerializeToUtf8Bytes(luggage);

                    // Simulate publish
                    Logger.LogInfo(channel, "scrambler", "info",
                        $"Forwarded luggage {luggage.Id} ({luggage.Identification} of {luggage.TotalCorrelation}) after {delay}ms delay");

                    await channel.BasicPublishAsync("", "resequencer_queue", body);
                }
                catch (Exception ex)
                {
                    Logger.LogInfo(channel, "scrambler", "error", $"Error in scrambler: {ex}");
                }
            });

            await Task.Yield();
        };

        await channel.BasicConsumeAsync(queue: "scrambler_queue", autoAck: true, consumer: consumer);

        await Task.Delay(-1);
    }
}

