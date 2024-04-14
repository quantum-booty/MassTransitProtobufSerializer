using System.Net.Mime;

namespace MassTransit.Serialization;

public class ProtobufMessageSerializer : IMessageSerializer
{
    // might be application/vnd.google.protobuf or application/x-protobuf or application/x-google-protobuf or application/protobuf
    public const string ContentTypeHeaderValue = "application/vnd.masstransit+pbuf";
    public static readonly ContentType ProtobufContentType = new(ContentTypeHeaderValue);

    public ContentType ContentType => ProtobufContentType;

    public MessageBody GetMessageBody<T>(SendContext<T> context) where T : class
        => new ProtobufMessageBody<T>(context);
}