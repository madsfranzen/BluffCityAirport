Console.WriteLine("Starting Bluff City Airport...");

var checkInTask = CheckIn.Run();
await Task.Delay(500);

// var splitterTask = Splitter.Run();
await Task.Delay(500);

await Task.WhenAll(checkInTask);

await Task.Delay(200);
Console.WriteLine("All modules completed!");
await Task.Delay(1000);
