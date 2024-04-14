using System;
using System.Collections.Generic;
using Google.Protobuf;
using MassTransit.Util;

namespace MassTransit.Serialization;

public static class ProtobufParserUtils
{
    public static T Parse<T>(ByteString byteString) where T : class, IMessage<T>, new() => new MessageParser<T>(() => new T()).ParseFrom(byteString);
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
        var deserialised = typeof(ProtobufParserUtils).GetMethod(nameof(Parse))!.MakeGenericMethod(typeof(T)).Invoke(this, new object?[] { _message }) ?? throw new InvalidOperationException();
        if (deserialised != null)
        {
            message = (T)deserialised;
            return true;
        }

        message = default;
        return false;
    }

    private static T Parse<T>(ByteString byteString) where T : class, IMessage<T>, new() => new MessageParser<T>(() => new T()).ParseFrom(byteString);

    public override bool TryGetMessage(Type messageType, out object message)
    {
        var deserialised = typeof(ProtobufParserUtils).GetMethod(nameof(Parse))!.MakeGenericMethod(messageType).Invoke(this, new object?[] { _message }) ?? throw new InvalidOperationException();
        if (deserialised != null)
        {
            message = deserialised;
            return true;
        }

        message = null;
        return false;
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