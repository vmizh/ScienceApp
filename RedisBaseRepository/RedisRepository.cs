using RedisProvider.Interfaces;
using StackExchange.Redis;
using System.Globalization;

namespace RedisProvider;

public class RedisRepository : IRedisRepository
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;

    public RedisRepository(ConnectionMultiplexer connection, int database = -1)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _db = database >= 0 ? _connection.GetDatabase(database) : _connection.GetDatabase();
    }

    public static async Task<RedisRepository> ConnectAsync(string configuration, int database = -1)
    {
        var conn = await ConnectionMultiplexer.ConnectAsync(configuration).ConfigureAwait(false);
        return new RedisRepository(conn, database);
    }

    // --------------------------- String (basic) ---------------------------
    public async Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (string.IsNullOrEmpty(value)) value = string.Empty;

        if (expiry.HasValue)
        {
            var ms = (long)expiry.Value.TotalMilliseconds;
            var res = await _db.ExecuteAsync("SET", key, value, "PX", ms).ConfigureAwait(false);
            return !res.IsNull && res.ToString().Equals("OK", StringComparison.OrdinalIgnoreCase);
        }

        var r = await _db.ExecuteAsync("SET", key, value).ConfigureAwait(false);
        return !r.IsNull && r.ToString().Equals("OK", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string?> StringGetAsync(string key)
    {
        var res = await _db.ExecuteAsync("GET", key).ConfigureAwait(false);
        if (res.IsNull) return null;
        return res.ToString();
    }

    public async Task<bool> StringDeleteAsync(string key)
    {
        var res = await _db.ExecuteAsync("DEL", key).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    // Conditional update only when key exists
    public async Task<bool> StringUpdateIfExistsAsync(string key, string value)
    {
        var res = await _db.ExecuteAsync("SET", key, value, "XX").ConfigureAwait(false);
        return !res.IsNull && res.ToString().Equals("OK", StringComparison.OrdinalIgnoreCase);
    }

    // --------------------------- Hash ---------------------------
    public async Task<bool> HashSetAsync(string key, string field, string value)
    {
        var res = await _db.ExecuteAsync("HSET", key, field, value).ConfigureAwait(false);
        return Convert.ToInt64(res) >= 0; // HSET returns 1 if new field, 0 if updated
    }

    public async Task<string?> HashGetAsync(string key, string field)
    {
        var res = await _db.ExecuteAsync("HGET", key, field).ConfigureAwait(false);
        if (res.IsNull) return null;
        return res.ToString();
    }

    public async Task<bool> HashDeleteAsync(string key, string field)
    {
        var res = await _db.ExecuteAsync("HDEL", key, field).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        var res = await _db.ExecuteAsync("HGETALL", key).ConfigureAwait(false);
        var dict = new Dictionary<string, string>();
        if (res.IsNull) return dict;
        var arr = (RedisResult[])res;
        for (var i = 0; i < arr.Length; i += 2)
        {
            var f = arr[i].ToString();
            var v = arr[i + 1].IsNull ? string.Empty : arr[i + 1].ToString();
            dict[f] = v;
        }

        return dict;
    }

    // --------------------------- List ---------------------------
    public async Task<long> ListLeftPushAsync(string key, string value)
    {
        var res = await _db.ExecuteAsync("LPUSH", key, value).ConfigureAwait(false);
        return Convert.ToInt64(res);
    }

    public async Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1)
    {
        var res = await _db.ExecuteAsync("LRANGE", key, start, stop).ConfigureAwait(false);
        var list = new List<string>();
        if (res.IsNull) return list;
        var arr = (RedisResult[])res;
        list.AddRange(arr.Select(a => a.IsNull ? string.Empty : a.ToString()));
        return list;
    }

    public async Task<bool> ListSetByIndexAsync(string key, long index, string value)
    {
        var res = await _db.ExecuteAsync("LSET", key, index, value).ConfigureAwait(false);
        return !res.IsNull && res.ToString().Equals("OK", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<long> ListRemoveAsync(string key, long count, string value)
    {
        var res = await _db.ExecuteAsync("LREM", key, count, value).ConfigureAwait(false);
        return Convert.ToInt64(res);
    }

    public async Task<string?> ListRightPopAsync(string key)
    {
        var res = await _db.ExecuteAsync("RPOP", key).ConfigureAwait(false);
        if (res.IsNull) return null;
        return res.ToString();
    }

    // --------------------------- Set ---------------------------
    public async Task<bool> SetAddAsync(string key, string member)
    {
        var res = await _db.ExecuteAsync("SADD", key, member).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    public async Task<bool> SetRemoveAsync(string key, string member)
    {
        var res = await _db.ExecuteAsync("SREM", key, member).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    public async Task<HashSet<string>> SetMembersAsync(string key)
    {
        var res = await _db.ExecuteAsync("SMEMBERS", key).ConfigureAwait(false);
        var set = new HashSet<string>();
        if (res.IsNull) return set;
        var arr = (RedisResult[])res;
        foreach (var a in arr) set.Add(a.IsNull ? string.Empty : a.ToString());
        return set;
    }

    // --------------------------- Sorted Set ---------------------------
    public async Task<bool> SortedSetAddAsync(string key, string member, double score)
    {
        var res = await _db.ExecuteAsync("ZADD", key, score, member).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    public async Task<bool> SortedSetRemoveAsync(string key, string member)
    {
        var res = await _db.ExecuteAsync("ZREM", key, member).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    public async Task<List<string>> SortedSetRangeByScoreAsync(string key, double min = double.NegativeInfinity,
        double max = double.PositiveInfinity)
    {
        // Redis expects strings for +/-inf
        var smin = double.IsNegativeInfinity(min) ? "-inf" : min.ToString(CultureInfo.InvariantCulture);
        var smax = double.IsPositiveInfinity(max) ? "+inf" : max.ToString(CultureInfo.InvariantCulture);
        var res = await _db.ExecuteAsync("ZRANGEBYSCORE", key, smin, smax).ConfigureAwait(false);
        var list = new List<string>();
        if (res.IsNull) return list;
        var arr = (RedisResult[])res;
        list.AddRange(arr.Select(a => a.IsNull ? string.Empty : a.ToString()));
        return list;
    }

    public async Task<string?> StreamAddAsync(string key, IDictionary<string, string> values)
    {
        if (values == null || values.Count == 0) throw new ArgumentException("values required", nameof(values));
        var args = new List<object> { key, "*" };
        foreach (var kv in values)
        {
            args.Add(kv.Key);
            args.Add(kv.Value);
        }

        var res = await _db.ExecuteAsync("XADD", args.ToArray()).ConfigureAwait(false);
        if (res.IsNull) return null;
        return res.ToString();
    }

    public async Task<List<StreamEntry>> StreamRangeAsync(string key, string start = "-", string end = "+",
        int count = 100)
    {
        var res = await _db.ExecuteAsync("XRANGE", key, start, end, "COUNT", count).ConfigureAwait(false);
        var list = new List<StreamEntry>();
        if (res.IsNull) return list;
        var arr = (RedisResult[])res;
        foreach (var item in arr)
        {
            // each item is an array [ id, [ field, value, ... ] ]
            var pair = (RedisResult[])item;
            var id = pair[0].ToString();
            var fieldArr = (RedisResult[])pair[1];
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < fieldArr.Length; i += 2)
            {
                var f = fieldArr[i].ToString();
                var v = fieldArr[i + 1].IsNull ? string.Empty : fieldArr[i + 1].ToString();
                dict[f] = v;
            }

            list.Add(new StreamEntry { Id = id, Values = dict });
        }

        return list;
    }

    public async Task<bool> StreamDeleteAsync(string key, string id)
    {
        var res = await _db.ExecuteAsync("XDEL", key, id).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    // --------------------------- Bitmap & Bitfield ---------------------------
    public async Task<bool> BitmapSetBitAsync(string key, long offset, bool bit)
    {
        var res = await _db.ExecuteAsync("SETBIT", key, offset, bit ? 1 : 0).ConfigureAwait(false);
        // SETBIT returns previous bit
        return Convert.ToInt64(res) == 1;
    }

    public async Task<bool> BitmapGetBitAsync(string key, long offset)
    {
        var res = await _db.ExecuteAsync("GETBIT", key, offset).ConfigureAwait(false);
        return Convert.ToInt64(res) == 1;
    }

    public async Task<long[]> BitFieldAsync(string key, params string[] subcommands)
    {
        if (subcommands == null || subcommands.Length == 0)
            throw new ArgumentException("subcommands required", nameof(subcommands));
        var args = new List<object> { key };
        args.AddRange(subcommands);
        var res = await _db.ExecuteAsync("BITFIELD", args.ToArray()).ConfigureAwait(false);
        if (res.IsNull) return Array.Empty<long>();
        var arr = (RedisResult[])res;
        return arr.Select(a => Convert.ToInt64(a)).ToArray();
    }

    // convenience for INCRBY: e.g. "INCRBY u8 0 1"
    public Task<long[]> BitFieldIncrByAsync(string key, string type, long offset, long increment)
    {
        var cmd = $"INCRBY {type} {offset} {increment}";
        return BitFieldAsync(key, cmd);
    }

    // --------------------------- Geospatial ---------------------------
    public async Task<bool> GeoAddAsync(string key, double longitude, double latitude, string member)
    {
        var res = await _db.ExecuteAsync("GEOADD", key, longitude.ToString(CultureInfo.InvariantCulture),
            latitude.ToString(CultureInfo.InvariantCulture), member).ConfigureAwait(false);
        return Convert.ToInt64(res) > 0;
    }

    public async Task<(double longitude, double latitude)?> GeoPosAsync(string key, string member)
    {
        var res = await _db.ExecuteAsync("GEOPOS", key, member).ConfigureAwait(false);
        if (res.IsNull) return null;
        var arr = (RedisResult[])res;
        if (arr.Length == 0 || arr[0].IsNull) return null;
        var coords = (RedisResult[])arr[0];
        // coords[0] = longitude, coords[1] = latitude
        var lon = double.Parse(coords[0].ToString(), CultureInfo.InvariantCulture);
        var lat = double.Parse(coords[1].ToString(), CultureInfo.InvariantCulture);
        return (lon, lat);
    }

    public async Task<List<GeoResult>> GeoRadiusAsync(string key, double longitude, double latitude, double radius,
        string unit = "m", int count = 10)
    {
        // GEORADIUS key lon lat radius unit WITHDIST WITHCOORD COUNT count
        var res = await _db.ExecuteAsync("GEORADIUS", key, longitude.ToString(CultureInfo.InvariantCulture),
            latitude.ToString(CultureInfo.InvariantCulture), radius.ToString(CultureInfo.InvariantCulture), unit,
            "WITHDIST", "WITHCOORD", "COUNT", count).ConfigureAwait(false);
        var list = new List<GeoResult>();
        if (res.IsNull) return list;
        var arr = (RedisResult[])res;
        foreach (var item in arr)
        {
            // item -> [ member, distance, [ lon, lat ] ]
            var parts = (RedisResult[])item;
            var member = parts[0].ToString();
            var dist = parts[1].IsNull
                ? (double?)null
                : double.Parse(parts[1].ToString(), CultureInfo.InvariantCulture);
            var coordArr = (RedisResult[])parts[2];
            var lon = coordArr[0].IsNull
                ? (double?)null
                : double.Parse(coordArr[0].ToString(), CultureInfo.InvariantCulture);
            var lat = coordArr[1].IsNull
                ? (double?)null
                : double.Parse(coordArr[1].ToString(), CultureInfo.InvariantCulture);
            list.Add(new GeoResult { Member = member, Distance = dist, Longitude = lon, Latitude = lat });
        }

        return list;
    }

    // --------------------------- Stream ---------------------------
    public class StreamEntry
    {
        public string Id { get; set; } = string.Empty;
        public Dictionary<string, string> Values { get; set; } = new();
    }

    public class GeoResult
    {
        public string Member { get; set; } = string.Empty;
        public double? Distance { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }
}
