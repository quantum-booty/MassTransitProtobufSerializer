using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Protocol;

namespace IntegrationTest;

public partial class MessageConsumer : IConsumer<Message>
{
    private readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<Message> context)
    {
        LogReceived(_logger, context.Message.Text);

        return Task.CompletedTask;
    }

    [LoggerMessage(1, LogLevel.Information, "Received Text: {Text}")]
    private static partial void LogReceived(ILogger logger, string text);
}