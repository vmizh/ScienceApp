using RedisProvider.Interfaces;
using StackExchange.Redis;

namespace RedisProvider;

public class RedisSubscriber : IAsyncDisposable, IRedisSubscriber
{
    private readonly ConnectionMultiplexer _conn;
    private readonly ISubscriber _sub;

    public RedisSubscriber(string connectionString)
    {
        _conn = ConnectionMultiplexer.Connect(connectionString);
        _sub = _conn.GetSubscriber();
    }

    public async ValueTask DisposeAsync()
    {
        await _conn.CloseAsync();
        await _conn.DisposeAsync();
    }

    public async Task SubscribeToChannelsMatching(string channelMatch, Func<string, string, Task> handler)
    {
        if (string.IsNullOrWhiteSpace(channelMatch))
            throw new ArgumentException("Channel match must not be empty.", nameof(channelMatch));
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));
        var channel = new RedisChannel(channelMatch, RedisChannel.PatternMode.Pattern);

        await _sub.SubscribeAsync(channel, async (ch, msg) =>
        {
            // ch = actual channel name, msg = message
            await handler(ch, msg).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task SubscribeAsync(string channel, Func<string, Task> handler)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel must not be empty.", nameof(channel));
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        await _sub.SubscribeAsync(channel, async (ch, value) =>
        {
            // Fire-and-forget with Task.Run, or await directly but beware of blocking
            await handler(value!).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task UnsubscribeAsync(string channel)
    {
        await _sub.UnsubscribeAsync(channel).ConfigureAwait(false);
    }
}
