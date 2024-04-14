using MassTransit.Serialization;

namespace MassTransit.Configuration;

public static class ProtobufConfigurationExtensions
{
    /// <summary>
    /// Registers the Protobuf serializer with the bus, using the default Protobuf message contract.
    /// </summary>
    public static void UseProtobufSerializer(this IBusFactoryConfigurator configurator)
    {
        var factory = new ProtobufSerializerFactory();

        configurator.AddSerializer(factory);
        configurator.AddDeserializer(factory, isDefault: true);
    }

    /// <summary>
    /// Register the Protobuf deserializer on the receive endpoint.
    /// </summary>
    public static void UseProtobufDeserializer(this IReceiveEndpointConfigurator configurator, bool isDefault = true)
    {
        var factory = new ProtobufSerializerFactory();

        configurator.AddDeserializer(factory, isDefault);
    }
}