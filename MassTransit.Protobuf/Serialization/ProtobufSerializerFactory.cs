using System.Net.Mime;

namespace MassTransit.Serialization;

public class ProtobufSerializerFactory : ISerializerFactory
{
    public ContentType ContentType => ProtobufMessageSerializer.ProtobufContentType;
    public IMessageSerializer CreateSerializer() => new ProtobufMessageSerializer();
    public IMessageDeserializer CreateDeserializer() => new ProtobufMessageDeserializer();
}