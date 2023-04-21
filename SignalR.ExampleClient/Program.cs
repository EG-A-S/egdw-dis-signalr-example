using SignalR.ExampleClient;

//Example app, not production ready solution

Console.WriteLine("App starting!");

var consumer = new Consumer();
await consumer.StartNotificationConnectionAsync();

Console.ReadKey();