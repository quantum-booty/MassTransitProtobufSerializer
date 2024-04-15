using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using MassTransit.Configuration;

namespace IntegrationTest;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.AddConsumer<MessageConsumer>();
                    x.AddConsumer<TestRequestConsumer>();
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.UseProtobufSerializer();
                        cfg.ConfigureEndpoints(context);
                    });
                    //x.UsingRabbitMq((context,cfg) =>
                    //{
                    //    cfg.ConfigureEndpoints(context);
                    //});
                });

                services.AddHostedService<Worker>();
            });
}