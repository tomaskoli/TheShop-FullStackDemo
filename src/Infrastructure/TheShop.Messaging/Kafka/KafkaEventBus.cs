using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheShop.Messaging.Abstractions;
using TheShop.SharedKernel;

namespace TheShop.Messaging.Kafka;

public class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventBus> _logger;
    private DateTime _lastConnectionErrorLog = DateTime.MinValue;
    private static readonly TimeSpan ConnectionErrorLogInterval = TimeSpan.FromMinutes(1);

    public KafkaEventBus(IOptions<KafkaSettings> settings, ILogger<KafkaEventBus> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            
            // Reduce reconnection spam when Kafka is unavailable
            MessageTimeoutMs = 5000,
            SocketTimeoutMs = 5000,
            RequestTimeoutMs = 5000,
            
            // Suppress connection close logs
            LogConnectionClose = false,
            
            // Retry settings - fail faster instead of retrying forever
            MessageSendMaxRetries = 2,
            RetryBackoffMs = 1000,
            RetryBackoffMaxMs = 5000,
            
            // Reduce metadata refresh frequency when broker is down
            SocketKeepaliveEnable = true,
            MetadataMaxAgeMs = 60000,
            ReconnectBackoffMs = 1000,
            ReconnectBackoffMaxMs = 10000
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                // Only log fatal errors immediately
                if (error.IsFatal)
                {
                    _logger.LogError("Kafka fatal error: {Reason}", error.Reason);
                    return;
                }

                // Suppress transient connection errors (log only once per minute)
                if (IsConnectionError(error))
                {
                    if (DateTime.UtcNow - _lastConnectionErrorLog > ConnectionErrorLogInterval)
                    {
                        _lastConnectionErrorLog = DateTime.UtcNow;
                        _logger.LogWarning("Kafka unavailable: {Reason}", error.Reason);
                    }
                    return;
                }

                // Log other non-transient errors
                _logger.LogWarning("Kafka error: {Reason}", error.Reason);
            })
            .SetLogHandler((_, logMessage) =>
            {
                // Suppress all rdkafka internal logs (they're too noisy)
                _logger.LogDebug("Kafka: {Message}", logMessage.Message);
            })
            .Build();
        
        _logger.LogInformation("Kafka producer initialized for {Servers}", settings.Value.BootstrapServers);
    }

    private static bool IsConnectionError(Error error)
    {
        return error.Code == ErrorCode.Local_AllBrokersDown ||
               error.Code == ErrorCode.Local_Transport ||
               error.Reason.Contains("Connect to") ||
               error.Reason.Contains("Disconnected");
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent
    {
        var topic = GetTopicName<T>();
        var message = new Message<string, string>
        {
            Key = @event.Id.ToString(),
            Value = JsonSerializer.Serialize(@event)
        };

        try
        {
            var result = await _producer.ProduceAsync(topic, message, ct);
            _logger.LogInformation(
                "Published event {EventType} to topic {Topic} at offset {Offset}",
                typeof(T).Name, topic, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType} to topic {Topic}", typeof(T).Name, topic);
            throw;
        }
    }

    private static string GetTopicName<T>()
    {
        var typeName = typeof(T).Name;

        // Convert PascalCase to kebab-case and add prefix
        var kebabCase = string.Concat(
            typeName.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? "-" + c : c.ToString()))
            .ToLowerInvariant();

        // Remove "integration-event" suffix if present
        kebabCase = kebabCase.Replace("-integration-event", "");

        return kebabCase;
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}

