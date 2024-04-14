using System;
using System.Net.Mime;
using System.Runtime.Serialization;

namespace MassTransit.Serialization;

public class ProtobufMessageDeserializer : IMessageDeserializer
{
    private readonly IObjectDeserializer _objectDeserializer;

    public ProtobufMessageDeserializer() => _objectDeserializer = SystemTextJsonMessageSerializer.Instance;

    public void Probe(ProbeContext context)
    {
        var scope = context.CreateScope("protobuf");
        scope.Add("contentType", ContentType.MediaType);
    }

    public ContentType ContentType => ProtobufMessageSerializer.ProtobufContentType;

    public ConsumeContext Deserialize(ReceiveContext receiveContext)
        => new BodyConsumeContext(receiveContext, Deserialize(receiveContext.Body, receiveContext.TransportHeaders, receiveContext.InputAddress));

    public SerializerContext Deserialize(MessageBody body, Headers headers, Uri? destinationAddress = null)
    {
        try
        {
            var envelopeMessage = ProtobufEnvelopeMessage.Parser.ParseFrom(body.GetStream());
            var envelope = new ProtobufMessageEnvelope(envelopeMessage);

            return envelope == null
                ? throw new SerializationException("The message envelope was not found.")
                : (SerializerContext)new ProtobufSerializerContext(_objectDeserializer, envelope);
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SerializationException("An exception occurred while deserializing the message envelope", ex);
        }
    }

    public MessageBody GetMessageBody(string text)
        => throw new NotSupportedException("ProtobufMessageDeserializer does not support deserializing from text.");
}