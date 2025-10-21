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

        Logger.LogInfo(channel, "scrambler", "warn", "Scrambler Ready! Awaiting input...");

        await channel.QueueDeclareAsync("scrambler_queue", false, false, false, null);
        await channel.QueueDeclareAsync("resequencer_queue", false, false, false, null);

        var random = new Random();

        await Task.Delay(1000);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            // We start a new task for each message we recieve, processing them asynchronously
            // This, combined with the random delay mechanism below, is what makes our scrambler work.
            _ = Task.Run(async () =>
            {
                try
                {
                    var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var luggage = JsonSerializer.Deserialize<Luggage>(jsonString);
                    var body = JsonSerializer.SerializeToUtf8Bytes(luggage);

                    // This random delay serves at the scrambling logic itself.
                    // Each luggage message is published with a randomized delay and becomes shuffled/scrambled as an effect of that.
                    int delay = random.Next(1000, 10000);
                    await Task.Delay(delay);

                    Logger.LogInfo(channel, "scrambler", "info",
                        $"Forwarded luggage {luggage!.Id} ({luggage.Identification} of {luggage.TotalCorrelation}) after {delay}ms delay");

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

