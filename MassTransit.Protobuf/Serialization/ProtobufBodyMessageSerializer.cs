using System.Net.Mime;

namespace MassTransit.Serialization;

public class ProtobufBodyMessageSerializer : IMessageSerializer
{
    private readonly ProtobufMessageEnvelope _envelope;

    public ProtobufBodyMessageSerializer(MessageEnvelope envelope) => _envelope = new ProtobufMessageEnvelope(envelope);

    public ContentType ContentType => ProtobufMessageSerializer.ProtobufContentType;

    public MessageBody GetMessageBody<T>(SendContext<T> context)
        where T : class
    {
        _envelope.Update(context);

        return new ProtobufMessageBody<T>(context, _envelope);
    }

    public void Overlay(object message) => _envelope.Message = message;
}