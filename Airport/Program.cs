Console.WriteLine("Starting Bluff City Airport...");

var checkInTask = CheckIn.Run();
await Task.Delay(250);

var splitterTask = Splitter.Run();
await Task.Delay(250);

var scramblerTask = Scrambler.Run();
await Task.Delay(250);

var resequencerTask = Resequencer.Run();
await Task.Delay(250);

var aggregatorTask = Aggregator.Run();

await Task.WhenAll(checkInTask, splitterTask, scramblerTask, resequencerTask, aggregatorTask);
