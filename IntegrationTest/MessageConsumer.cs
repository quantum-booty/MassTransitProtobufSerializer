using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Protocol;

namespace IntegrationTest;

public static class Counter
{
    public static int Count { get; set; }
    public static readonly DateTime StartTime = DateTime.UtcNow;
}

public partial class MessageConsumer : IConsumer<Message>
{
    private readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<Message> context)
    {
        var secondPassed = (DateTime.UtcNow - Counter.StartTime).TotalSeconds;
        var messagePerSec = Counter.Count / secondPassed;
        LogReceived(_logger, context.CorrelationId.ToString(), context.Message.Text, Counter.Count, secondPassed, messagePerSec);
        Counter.Count += 1;
        var headers = context.Headers;

        return Task.CompletedTask;
    }

    [LoggerMessage(1, LogLevel.Information, "{correlationId} Received Text: {Text} {count} {secondPassed} {messagePerSec}")]
    private static partial void LogReceived(ILogger logger, string correlationId, string text, int count, double secondPassed, double messagePerSec);
}