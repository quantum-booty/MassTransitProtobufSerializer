using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Protocol;

namespace IntegrationTest;

public static class Counter
{
    public static decimal Count { get; set; }
    public static readonly DateTime StartTime = DateTime.UtcNow;
}

public partial class MessageConsumer : IConsumer<Message>
{
    private readonly ILogger<MessageConsumer> _logger;
    private readonly IRequestClient<RequestMessage> _client;

    public MessageConsumer(ILogger<MessageConsumer> logger, IRequestClient<RequestMessage> client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task Consume(ConsumeContext<Message> context)
    {
        var secondPassed = (DateTime.UtcNow - Counter.StartTime).TotalSeconds;
        var messagePerSec = Counter.Count / (decimal)secondPassed;
        if (Counter.Count % 1 == 0)
            LogReceived(_logger, context.CorrelationId.ToString(), context.Message.Text, Counter.Count, secondPassed, messagePerSec);
        Counter.Count += 1;
        var headers = context.Headers;

        // var response = await _client.GetResponse<ResponseMessage>(new RequestMessage { Text = context.Message.Text });
    }

    [LoggerMessage(1, LogLevel.Information, "{correlationId} Received Text: {Text} {count} {secondPassed} {messagePerSec}")]
    private static partial void LogReceived(ILogger logger, string? correlationId, string text, decimal count, double secondPassed, decimal messagePerSec);
}

public partial class TestRequestConsumer : IConsumer<RequestMessage>
{
    public async Task Consume(ConsumeContext<RequestMessage> context)
    {
        await context.RespondAsync<ResponseMessage>(new
        {
            Text = context.Message.Text + " response"
        });
    }
}

// public class RequestMessage
// {
//     public string Text { get; set; } = string.Empty;
// }
//
// public class ResponseMessage
// {
//     public string Text { get; set; } = string.Empty;
// }
//
// public class Message
// {
//     public string Text { get; set; } = string.Empty;
// }