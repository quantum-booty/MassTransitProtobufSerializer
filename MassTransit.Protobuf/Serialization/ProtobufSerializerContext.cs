using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Google.Protobuf;
using MassTransit.Util;

namespace MassTransit.Serialization;

public static class ProtobufParserUtils
{
    private static readonly ConcurrentDictionary<Type, Func<ByteString, object>> _cache = new();
    public static T Parse<T>(ByteString byteString) where T : class, IMessage<T>, new() => new MessageParser<T>(() => new T()).ParseFrom(byteString);
    public static Func<ByteString, object> GetParser(Type type)
        => _cache.GetOrAdd(
                type,
                _ =>
                {
                    var method = typeof(ProtobufParserUtils).GetMethod(nameof(Parse))!.MakeGenericMethod(type);
                    var func = (Func<ByteString, object>)method.CreateDelegate(typeof(Func<ByteString, object>));
                    return func;
                }
        );
}

public abstract class ProtobufBodySerializerContext :
    BaseSerializerContext
{
    private readonly object? _message;

    protected ProtobufBodySerializerContext(IObjectDeserializer objectDeserializer, MessageContext messageContext,
        object? message, string[] supportedMessageTypes)
        : base(objectDeserializer, messageContext, supportedMessageTypes)
        => _message = message;

    public override bool TryGetMessage<T>(out T? message)
        where T : class
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        message = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        if (_message == null)
            return false;
        var parser = ProtobufParserUtils.GetParser(typeof(T));
        message = parser((ByteString)_message) as T;
        return message != null;
    }

    private static T Parse<T>(ByteString byteString) where T : class, IMessage<T>, new() => new MessageParser<T>(() => new T()).ParseFrom(byteString);

    public override bool TryGetMessage(Type messageType, out object message)
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        message = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        if (_message == null)
            return false;
        var parser = ProtobufParserUtils.GetParser(messageType);
        message = parser((ByteString)_message);
        return message != null;
    }

    public override Dictionary<string, object> ToDictionary<T>(T? message)
        where T : class => ConvertObject.ToDictionary(message);
}

public class ProtobufSerializerContext : ProtobufBodySerializerContext
{
    private readonly MessageEnvelope _envelope;

    public ProtobufSerializerContext(IObjectDeserializer objectDeserializer, MessageEnvelope envelope)
        : base(objectDeserializer, new EnvelopeMessageContext(envelope, objectDeserializer), envelope.Message,
            envelope.MessageType ?? Array.Empty<string>())
        => _envelope = envelope;

    public override IMessageSerializer GetMessageSerializer() => new ProtobufBodyMessageSerializer(_envelope);

    public override IMessageSerializer GetMessageSerializer<T>(MessageEnvelope envelope, T message)
    {
        var serializer = new ProtobufBodyMessageSerializer(envelope);
        serializer.Overlay(message);
        return serializer;
    }

    public override IMessageSerializer GetMessageSerializer(object message, string[] messageTypes)
    {
        var envelope = new ProtobufMessageEnvelope(this, message, messageTypes);
        return new ProtobufBodyMessageSerializer(envelope);
    }
}