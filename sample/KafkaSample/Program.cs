IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddApplicationServices();
    })
    .Build();

await host.RunAsync();