namespace RedisProvider.Interfaces;

public interface IRedisRepository
{
    #region String
    Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> StringGetAsync(string key);
    Task<bool> StringDeleteAsync(string key);
    Task<bool> StringUpdateIfExistsAsync(string key, string value);
    #endregion

    #region Hash
    Task<bool> HashSetAsync(string key, string field, string value);
    Task<string?> HashGetAsync(string key, string field);
    Task<bool> HashDeleteAsync(string key, string field);
    Task<Dictionary<string, string>> HashGetAllAsync(string key);
    #endregion

    #region List
    Task<long> ListLeftPushAsync(string key, string value);
    Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1);
    Task<bool> ListSetByIndexAsync(string key, long index, string value);
    Task<long> ListRemoveAsync(string key, long count, string value);
    Task<string?> ListRightPopAsync(string key);
    #endregion

    #region Set
    Task<bool> SetAddAsync(string key, string member);
    Task<bool> SetRemoveAsync(string key, string member);
    Task<HashSet<string>> SetMembersAsync(string key);
    #endregion

    #region SortedSet
    Task<bool> SortedSetAddAsync(string key, string member, double score);
    Task<bool> SortedSetRemoveAsync(string key, string member);

    Task<List<string>> SortedSetRangeByScoreAsync(string key, double min = double.NegativeInfinity,
        double max = double.PositiveInfinity);
    #endregion

    #region Stream
    Task<string?> StreamAddAsync(string key, IDictionary<string, string> values);
    Task<List<RedisRepository.StreamEntry>> StreamRangeAsync(string key, string start = "-", string end = "+",
        int count = 100);
    Task<bool> StreamDeleteAsync(string key, string id);
    #endregion

    #region Bitmap
    Task<bool> BitmapSetBitAsync(string key, long offset, bool bit);
    Task<bool> BitmapGetBitAsync(string key, long offset);
    #endregion

    #region MyRegion
    Task<long[]> BitFieldAsync(string key, params string[] subcommands);
    Task<long[]> BitFieldIncrByAsync(string key, string type, long offset, long increment);
    #endregion

    #region Geo 
    Task<bool> GeoAddAsync(string key, double longitude, double latitude, string member);
    Task<(double longitude, double latitude)?> GeoPosAsync(string key, string member);

    Task<List<RedisRepository.GeoResult>> GeoRadiusAsync(string key, double longitude, double latitude, double radius,
        string unit = "m", int count = 10);
    #endregion
}
