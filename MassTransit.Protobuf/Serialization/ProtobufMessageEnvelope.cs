using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MassTransit;
using MassTransit.Metadata;

namespace MassTransit.Serialization;

[Serializable]
public class ProtobufMessageEnvelope : MessageEnvelope
{
    public string? MessageId { get; set; }
    public string? RequestId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ConversationId { get; set; }
    public string? InitiatorId { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public string? ResponseAddress { get; set; }
    public string? FaultAddress { get; set; }
    public string[]? MessageType { get; set; }
    public DateTime? ExpirationTime { get; set; }
    public DateTime? SentTime { get; set; }
    public HostInfo? Host { get; set; }

    public object? Message { get; set; }

    public Dictionary<string, object?>? Headers { get; set; }

    public ProtobufEnvelopeMessage ToDto()
    {
        var byteString = Message != null ? ((IMessage)Message)?.ToByteString() : null;
        return new()
        {
            MessageId = MessageId,
            RequestId = RequestId,
            CorrelationId = CorrelationId,
            ConversationId = ConversationId,
            InitiatorId = InitiatorId,
            DestinationAddress = DestinationAddress,
            ResponseAddress = ResponseAddress,
            FaultAddress = FaultAddress,
            MessageType = { MessageType },
            Message = byteString,
            ExpirationTime = ExpirationTime.HasValue ? Timestamp.FromDateTime(ExpirationTime.Value) : null,
            SentTime = SentTime.HasValue ? Timestamp.FromDateTime(SentTime.Value) : null,
            Headers = UnsafeByteOperations.UnsafeWrap(JsonSerializer.SerializeToUtf8Bytes(Headers)),
            Host = Host != null ? new HostInfoMessage
            {
                MachineName = Host.MachineName,
                ProcessName = Host.ProcessName,
                ProcessId = Host.ProcessId,
                Assembly = Host.Assembly,
                AssemblyVersion = Host.AssemblyVersion,
                FrameworkVersion = Host.FrameworkVersion,
                MassTransitVersion = Host.MassTransitVersion,
                OperatingSystemVersion = Host.OperatingSystemVersion,
            } : null,
        };
    }

    public ProtobufMessageEnvelope()
    {
    }

    public ProtobufMessageEnvelope(SendContext context, object? message, string[] messageTypeNames)
    {
        MessageId = context.MessageId?.ToString();
        RequestId = context.RequestId?.ToString();
        CorrelationId = context.CorrelationId?.ToString();
        ConversationId = context.ConversationId?.ToString();
        InitiatorId = context.InitiatorId?.ToString();
        SourceAddress = context.SourceAddress?.ToString();
        DestinationAddress = context.DestinationAddress?.ToString();
        ResponseAddress = context.ResponseAddress?.ToString();
        FaultAddress = context.FaultAddress?.ToString();
        MessageType = messageTypeNames;
        Message = message;
        if (context.TimeToLive.HasValue)
            ExpirationTime = DateTime.UtcNow + context.TimeToLive;
        SentTime = context.SentTime ?? DateTime.UtcNow;
        Headers = context.Headers.GetAll().ToDictionary(x => x.Key, x => (object?)x.Value, StringComparer.OrdinalIgnoreCase);
        Host = HostMetadataCache.Host;
    }

    public ProtobufMessageEnvelope(MessageContext context, object? message, string[] messageTypeNames)
    {
        MessageId = context.MessageId?.ToString();
        RequestId = context.RequestId?.ToString();
        CorrelationId = context.CorrelationId?.ToString();
        ConversationId = context.ConversationId?.ToString();
        InitiatorId = context.InitiatorId?.ToString();
        SourceAddress = context.SourceAddress?.ToString();
        DestinationAddress = context.DestinationAddress?.ToString();
        ResponseAddress = context.ResponseAddress?.ToString();
        FaultAddress = context.FaultAddress?.ToString();
        MessageType = messageTypeNames;
        Message = message;
        if (context.ExpirationTime.HasValue)
            ExpirationTime = context.ExpirationTime;
        SentTime = context.SentTime ?? DateTime.UtcNow;
        Headers = context.Headers.GetAll().ToDictionary(x => x.Key, x => (object?)x.Value, StringComparer.OrdinalIgnoreCase);
        Host = HostMetadataCache.Host;
    }

    public ProtobufMessageEnvelope(MessageEnvelope envelope)
    {
        MessageId = envelope.MessageId;
        RequestId = envelope.RequestId;
        CorrelationId = envelope.CorrelationId;
        ConversationId = envelope.ConversationId;
        InitiatorId = envelope.InitiatorId;
        SourceAddress = envelope.SourceAddress;
        DestinationAddress = envelope.DestinationAddress;
        ResponseAddress = envelope.ResponseAddress;
        FaultAddress = envelope.FaultAddress;
        MessageType = envelope.MessageType;
        Message = envelope.Message;
        ExpirationTime = envelope.ExpirationTime;
        SentTime = envelope.SentTime ?? DateTime.UtcNow;
        Headers = envelope.Headers != null
            ? new Dictionary<string, object?>(envelope.Headers, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        Host = envelope.Host ?? HostMetadataCache.Host;
    }

    public ProtobufMessageEnvelope(ProtobufEnvelopeMessage envelope)
    {
        MessageId = envelope.MessageId;
        RequestId = envelope.RequestId;
        CorrelationId = envelope.CorrelationId;
        ConversationId = envelope.ConversationId;
        InitiatorId = envelope.InitiatorId;
        SourceAddress = envelope.SourceAddress;
        DestinationAddress = envelope.DestinationAddress;
        ResponseAddress = envelope.ResponseAddress;
        FaultAddress = envelope.FaultAddress;
        MessageType = envelope.MessageType?.ToArray();
        Message = envelope.Message;
        ExpirationTime = envelope.ExpirationTime?.ToDateTime();
        SentTime = envelope.SentTime?.ToDateTime();
        Headers = JsonSerializer.Deserialize<Dictionary<string, object?>?>(envelope.Headers.Span)?.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        if (envelope.Headers.IsEmpty)
            Headers = new(StringComparer.OrdinalIgnoreCase);
        else
        {
            var deserialisedHeaders = JsonSerializer.Deserialize<Dictionary<string, object?>?>(envelope.Headers.Span);
            Headers = deserialisedHeaders != null
                ? new(deserialisedHeaders, StringComparer.OrdinalIgnoreCase)
                : new(StringComparer.OrdinalIgnoreCase);
        }
        Host = Host != null ? new ProtobufHostInfo
        {
            MachineName = envelope.Host.MachineName,
            ProcessName = envelope.Host.ProcessName,
            ProcessId = envelope.Host.ProcessId,
            Assembly = envelope.Host.Assembly,
            AssemblyVersion = envelope.Host.AssemblyVersion,
            FrameworkVersion = envelope.Host.FrameworkVersion,
            MassTransitVersion = envelope.Host.MassTransitVersion,
            OperatingSystemVersion = envelope.Host.OperatingSystemVersion,
        } : null;
    }

    public void Update(SendContext context)
    {
        DestinationAddress = context.DestinationAddress?.ToString();

        if (context.SourceAddress != null)
            SourceAddress = context.SourceAddress.ToString();

        if (context.ResponseAddress != null)
            ResponseAddress = context.ResponseAddress.ToString();

        if (context.FaultAddress != null)
            FaultAddress = context.FaultAddress.ToString();

        if (context.MessageId.HasValue)
            MessageId = context.MessageId.ToString();

        if (context.RequestId.HasValue)
            RequestId = context.RequestId.ToString();

        if (context.ConversationId.HasValue)
            ConversationId = context.ConversationId.ToString();

        if (context.CorrelationId.HasValue)
            CorrelationId = context.CorrelationId.ToString();

        if (context.InitiatorId.HasValue)
            InitiatorId = context.InitiatorId.ToString();

        if (context.TimeToLive.HasValue)
            ExpirationTime = DateTime.UtcNow + (context.TimeToLive > TimeSpan.Zero ? context.TimeToLive : TimeSpan.FromSeconds(1));

        foreach (var header in context.Headers.GetAll())
            if (Headers != null)
                Headers[header.Key] = header.Value;
            else
                Headers = new(StringComparer.OrdinalIgnoreCase);
    }
}