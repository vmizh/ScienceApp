namespace RedisProvider.Interfaces;

public interface IRedisPublisher
{
    ValueTask DisposeAsync();
    Task<long> PublishAsync(string channel, string message);
}
