using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Splitter
{
    public static async Task Run()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("[Splitter] Splitter Started - Waiting for input...");

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

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

            // Print nicely
            Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine("[Splitter] Recieved:");
            Console.WriteLine(
                JsonSerializer.Serialize(
                    checkInData,
                    new JsonSerializerOptions { WriteIndented = true }
                )
            );

            await Task.Yield(); // ensures async handler continues properly
        };

        await channel.BasicConsumeAsync(queue: "checkin_queue", autoAck: true, consumer: consumer);

        await Task.Delay(-1); // Infinite non-blocking wait
    }
}
