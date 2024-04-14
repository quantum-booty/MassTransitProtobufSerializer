using System;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<string, string?>? _headers;

    public HostInfo? Host { get; set; }

    public object? Message { get; set; }

    public Dictionary<string, object?>? Headers
    {
        get => _headers?.ToDictionary(kv => kv.Key, kv => kv.Value as object);
        set => _headers = value?.ToDictionary(kv => kv.Key, kv => kv.Value as string);
    }

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
            Headers = { _headers?.Select(x => new HeaderEntryMessage { Key = x.Key, Value = x.Value }) ?? Array.Empty<HeaderEntryMessage>() },
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
        if (context.MessageId.HasValue)
            MessageId = context.MessageId.Value.ToString();
        if (context.RequestId.HasValue)
            RequestId = context.RequestId.Value.ToString();
        if (context.CorrelationId.HasValue)
            CorrelationId = context.CorrelationId.Value.ToString();
        if (context.ConversationId.HasValue)
            ConversationId = context.ConversationId.Value.ToString();
        if (context.InitiatorId.HasValue)
            InitiatorId = context.InitiatorId.Value.ToString();
        if (context.SourceAddress != null)
            SourceAddress = context.SourceAddress.ToString();
        if (context.DestinationAddress != null)
            DestinationAddress = context.DestinationAddress.ToString();
        if (context.ResponseAddress != null)
            ResponseAddress = context.ResponseAddress.ToString();
        if (context.FaultAddress != null)
            FaultAddress = context.FaultAddress.ToString();
        MessageType = messageTypeNames;
        Message = message;
        if (context.TimeToLive.HasValue)
            ExpirationTime = DateTime.UtcNow + context.TimeToLive;
        SentTime = context.SentTime ?? DateTime.UtcNow;
        Headers = new Dictionary<string, object?>();
        foreach (var header in context.Headers.GetAll())
            Headers[header.Key] = header.Value;
        Host = HostMetadataCache.Host;
    }

    public ProtobufMessageEnvelope(MessageContext context, object? message, string[] messageTypeNames)
    {
        if (context.MessageId.HasValue)
            MessageId = context.MessageId.Value.ToString();
        if (context.RequestId.HasValue)
            RequestId = context.RequestId.Value.ToString();
        if (context.CorrelationId.HasValue)
            CorrelationId = context.CorrelationId.Value.ToString();
        if (context.ConversationId.HasValue)
            ConversationId = context.ConversationId.Value.ToString();
        if (context.InitiatorId.HasValue)
            InitiatorId = context.InitiatorId.Value.ToString();
        if (context.SourceAddress != null)
            SourceAddress = context.SourceAddress.ToString();
        if (context.DestinationAddress != null)
            DestinationAddress = context.DestinationAddress.ToString();
        if (context.ResponseAddress != null)
            ResponseAddress = context.ResponseAddress.ToString();
        if (context.FaultAddress != null)
            FaultAddress = context.FaultAddress.ToString();
        MessageType = messageTypeNames;
        Message = message;
        if (context.ExpirationTime.HasValue)
            ExpirationTime = context.ExpirationTime;
        SentTime = context.SentTime ?? DateTime.UtcNow;
        Headers = new Dictionary<string, object?>();
        foreach (var header in context.Headers.GetAll())
            Headers[header.Key] = header.Value;
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
        Headers = envelope.Headers?.ToDictionary(x => x.Key, x => (object?)x.Value, StringComparer.OrdinalIgnoreCase);
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
                Headers = new Dictionary<string, object?> { { header.Key, header.Value } };
    }
}