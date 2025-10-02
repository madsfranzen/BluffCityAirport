using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

public static class Logger
{
    public static void LogInfo(IChannel channel, string service, string level, string msg)
    {
        var log = new
        {
            service = service,
            level = level,
            message = msg,
            timestamp = DateTime.UtcNow,
        };

        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(log));
        channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "logs",
            body: body
        );
    }
}
