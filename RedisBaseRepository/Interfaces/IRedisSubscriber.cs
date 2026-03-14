namespace RedisProvider.Interfaces;

public interface IRedisSubscriber
{
    ValueTask DisposeAsync();
    Task SubscribeToChannelsMatching(string channelMatch, Func<string, string, Task> handler);
    Task SubscribeAsync(string channel, Func<string, Task> handler);
    Task UnsubscribeAsync(string channel);
}
