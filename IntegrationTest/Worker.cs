using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Serialization;
using Microsoft.Extensions.Hosting;
using Protocol;

namespace IntegrationTest;

public class Worker : BackgroundService
{
    private readonly IBus _bus;

    public Worker(IBus bus) => _bus = bus;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new Message { Text = $"The time is {DateTimeOffset.Now}" }, context =>
            {
                context.ContentType = ProtobufMessageSerializer.ProtobufContentType;
            }, stoppingToken);

            // await Task.Delay(1, stoppingToken);
        }
    }
}