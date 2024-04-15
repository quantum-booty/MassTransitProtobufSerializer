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
        // while (!stoppingToken.IsCancellationRequested)
        // {
            await _bus.Publish(new Batch<Message> { Text = $"The time is {DateTimeOffset.Now}" }, context =>
            {
                // context.ContentType = ProtobufMessageSerializer.ProtobufContentType;
                // context.CorrelationId = NewId.Next().ToGuid();
                context.Headers.Set("test", "yaya");
            }, stoppingToken);

            // await Task.Delay(1, stoppingToken);
        // }
    }
}