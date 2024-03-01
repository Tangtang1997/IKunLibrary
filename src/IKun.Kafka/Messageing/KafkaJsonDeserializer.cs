namespace IKun.Kafka.Messageing;

/// <summary>
/// kafka序列化
/// </summary>
/// <typeparam name="T"></typeparam>
public class KafkaJsonDeserializer<T> : IDeserializer<T> where T : IKafkaRequest
{
    private readonly ILogger<KafkaJsonDeserializer<T>> _logger;

    public KafkaJsonDeserializer(ILogger<KafkaJsonDeserializer<T>> logger)
    {
        _logger = logger;
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            return default!;  
        }
        var json = Encoding.UTF8.GetString(data);

        _logger.LogDebug($"Consumer message: {json}");

        try
        {
            var t = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
            return t!;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Consumer message deserialize error: {json}");
            return default!;
        }
    }
}