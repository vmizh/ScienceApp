using RedisProvider.Interfaces;
using StackExchange.Redis;

namespace RedisProvider;

public class RedisPublisher : IAsyncDisposable, IRedisPublisher
{
    private readonly ConnectionMultiplexer _conn;
    private readonly ISubscriber _sub;

    public RedisPublisher(string connectionString)
    {
        _conn = ConnectionMultiplexer.Connect(connectionString);
        _sub = _conn.GetSubscriber();
    }

    public async ValueTask DisposeAsync()
    {
        await _conn.CloseAsync();
        await _conn.DisposeAsync();
    }

    public async Task<long> PublishAsync(string channel, string message)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel must not be empty.", nameof(channel));

        return await _sub.PublishAsync(channel, message).ConfigureAwait(false);
    }
}
