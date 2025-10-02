using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Splitter
{
    public static async Task Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Logger.LogInfo(channel, "splitter", "info", "Splitter Ready! Awaiting input...");

        await channel.QueueDeclareAsync(
            queue: "checkin_queue",
            durable: false,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            await Task.Delay(250);
            var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
            var checkInData = JsonSerializer.Deserialize<CheckInJSON>(jsonString);

            var checkInDataString = JsonSerializer.Serialize(
                checkInData,
                new JsonSerializerOptions { WriteIndented = false }
            );

            Logger.LogInfo(channel, "splitter", "info", $"Received message on checkin_queue: {checkInDataString}");

            await Task.Yield();
        };

        await channel.BasicConsumeAsync(queue: "checkin_queue", autoAck: true, consumer: consumer);

        await Task.Delay(-1);
    }
}
