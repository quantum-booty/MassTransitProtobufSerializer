using Google.Protobuf;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace MassTransit.Serialization;

public class ProtobufMessageBody<TProtoMessage> : MessageBody where TProtoMessage : class
{
    private readonly SendContext<TProtoMessage> _context;
    private readonly byte[]? _bytes;
    private ProtobufMessageEnvelope? _envelope;
    private string? _string;

    public ProtobufMessageBody(SendContext<TProtoMessage> context, ProtobufMessageEnvelope? envelope = null)
    {
        _context = context;
        _envelope = envelope;
    }

    public long? Length => _bytes?.Length;

    public byte[] GetBytes()
    {
        if (_bytes != null)
            return _bytes;

        try
        {
            var envelope = _envelope ??= new ProtobufMessageEnvelope(_context, _context.Message, MessageTypeCache<TProtoMessage>.MessageTypeNames);
            return envelope.ToDto().ToByteArray();
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SerializationException("Failed to serialize the message", ex);
        }
    }

    public Stream GetStream() => new MemoryStream(GetBytes(), false);

    public string GetString()
    {
        if (_string != null)
            return _string;

        _string = Encoding.UTF8.GetString(GetBytes());
        return _string;
    }
}