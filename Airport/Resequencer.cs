using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Schemas;

class Resequencer
{
    private static Dictionary<String, ResequencedMessage> recievedMessages = new Dictionary<String, ResequencedMessage>();

    public static async Task Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Logger.LogInfo(channel, "Resequencer", "warn", "Resequencer Ready! Awaiting input...");

        await channel.QueueDeclareAsync("resequencer_queue_queue", false, false, false, null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            await Task.Delay(1200);

            try
            {

                var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
                var luggage = JsonSerializer.Deserialize<Luggage>(jsonString);

                // returns true if we are done processing this sequence
                if (ProcessLuggage(luggage!))
                {

                    List<Luggage> luggagesPackage = new List<Luggage>();

                    foreach (Luggage lug in recievedMessages[luggage!.Id].GetLuggages())
                    {
                        luggagesPackage.Add(lug);
                    }

                    // We are inserting the flightNumber at the start of the message, to help ourselves in the aggregation step.
                    // The flight information also exist in the luggage itself, but I find it to be cleaner to bundle them together like this.
                    string flightNumber = luggage!.Flight!.Attributes.number;
                    var message = new
                    {
                        flightNumber,
                        luggagesPackage
                    };

                    var body = JsonSerializer.SerializeToUtf8Bytes(message);
                    await channel.BasicPublishAsync("", "aggregator_queue", body);

                    Logger.LogInfo(channel, "resequencer", "info", $"Done processing sequence : {luggage.Id} - {luggagesPackage.Count()} of {luggage.TotalCorrelation}");

                    // Clearing the "buffer"
                    recievedMessages.Remove(luggage.Id);
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(channel, "resequencer", "error", $"Error in Resequencer: {ex}");
            }

            await Task.Yield();
        };

        await channel.BasicConsumeAsync(queue: "resequencer_queue", autoAck: true, consumer: consumer);

        await Task.Delay(-1);
    }

    private static bool ProcessLuggage(Luggage luggage)
    {
        try
        {
            if (!recievedMessages.ContainsKey(luggage.Id))
            {
                recievedMessages[luggage.Id] = new ResequencedMessage(luggage.TotalCorrelation);
            }
            recievedMessages[luggage.Id].addLuggage(luggage);


            return recievedMessages[luggage.Id].isComplete();
        }
        catch (System.Exception)
        {
            throw new Exception("Something went wrong while processing luggage.");
        }
    }

    private class ResequencedMessage
    {
        private Luggage[] luggages;
        private int fillCount = 0;

        public ResequencedMessage(int size)
        {
            this.luggages = new Luggage[size];
        }

        public int getFillCount() => this.fillCount;

        public Luggage[] GetLuggages() => (Luggage[])luggages.Clone();

        public void addLuggage(Luggage luggage)
        {
            luggages[luggage.Identification - 1] = luggage;
            fillCount++;
        }

        public bool isComplete()
        {
            if (fillCount == luggages.Length)
            {
                return true;
            }
            else return false;
        }

        public override string ToString()
        {
            var result = $"{Environment.NewLine}ResequencedMessage: fillCount={fillCount} Luggages:{Environment.NewLine}";

            for (int i = 0; i < luggages.Length; i++)
            {
                if (luggages[i] != null)
                {
                    result += luggages[i].ToString();
                }
                else
                {
                    result += "null";
                }

                result += Environment.NewLine;
            }
            return result;
        }
    }
}

