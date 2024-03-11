namespace RabbitMqSample.Handling.Test;

public class TestRequest : IRabbitMqRequest
{
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    #region 自定义字段

    /// <summary>
    /// id
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }

    #endregion
}