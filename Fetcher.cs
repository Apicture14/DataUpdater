using System.Net.Http.Json;
using MySql.Data.MySqlClient;
using System.Text.Json;
using System.Text.Json.Nodes;
using DataUpdater.DAO;
using DataUpdater.Models;

namespace DataUpdater.Core;

public class Fetcher
{
    /// <summary>
    /// MTR Web线路图的地址
    /// </summary>
    public string Host { get; set; }
    
    /// <summary>
    /// 默认的UserAgent
    /// </summary>
    private const string UA =
        @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36";
    
    /// <summary>
    /// 项目默认的线路图地址
    /// </summary>
    private const string DefaultHost = @"https://tetsudou-map.haruto.city";

    /// <summary>
    /// 维度枚举
    /// </summary>
    public enum Dimensions
    {
        OverWorld = 0,
        Nether = 2,
        TheEnd = 1
    }

    /// <summary>
    /// 获取数据的类型
    /// </summary>
    [Obsolete]
    public enum FetchMode
    {
        Stations = 0,
        Routes = 1,
        StationAndRoutes = 2
    }
    
    private const string UrlStationAndRoutes = "/mtr/api/map/stations-and-routes?dimension={0}";
    private const string UrlCilents = "/mtr/api/map/clients?dimension={0}";
    private const string UrlArrivals = "/mtr/api/map/arrivals?dimension={0}";
    private static readonly HttpClient _client = new HttpClient();

    public Fetcher(string host)
    {
        this.Host = string.IsNullOrEmpty(host)?DefaultHost:host;

        _client.DefaultRequestHeaders.Add("User-Agent", UA);
        _client.DefaultRequestHeaders.Add("Accept", "application/json;text/plain;charset=UTF-8");
        _client.DefaultRequestHeaders.Add("Referer", "https://tetsudou-map.haruto.city/");
    }

//Task<KeyValuePair<List<Model.Station>,List<Model.Route>>>
    /// <summary>
    /// 从服务器更新站点到数据库
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <param name="d">要获取的维度</param>
    public async Task UpdateStationsFromServer(DataAccessor dataAccessor, Dimensions d = Dimensions.OverWorld)
    {
        string uri = Host + string.Format(UrlStationAndRoutes, (int)d);
        string sql = "INSERT INTO stations (sid,name,color,connection) VALUES ({0},{1},{2},{3})";

        HttpResponseMessage resp = await _client.GetAsync(uri);
        resp.EnsureSuccessStatusCode();
        Model.Pack p = await resp.Content.ReadFromJsonAsync<Model.Pack>();
        var ls = p.data["stations"].AsArray().ToList();


        int counter = 0;
        foreach (var s in ls)
        {
            ++counter;
            Model.Station st = s.Deserialize<Model.Station>();

            Console.Write($"Dealing Item {counter}/{ls.Count} {st.name}({st.id}) {st.connections.Count} connections");

            using (var cmd = new MySqlCommand(
                       "INSERT IGNORE INTO stations (sid, name, color, connection) VALUES (@id, @name, @color, @connection)",
                       dataAccessor.Connection))
            {
                cmd.Parameters.AddWithValue("@id", st.id);
                cmd.Parameters.AddWithValue("@name", st.name);
                cmd.Parameters.AddWithValue("@color", st.color);
                cmd.Parameters.AddWithValue("@connection",
                    string.Join(",", st.connections));

                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine(" Done!");
            }
        }
        return;
    }
    
    /// <summary>
    /// 从服务器更新站点到数据库
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <param name="d">要获取的维度</param>
    public async Task UpdateRoutesFromServer(DataAccessor dataAccessor, Dimensions d = Dimensions.OverWorld)
    {
        string uri = Host + string.Format(UrlStationAndRoutes, (int)d);
        string sql = """
                     INSERT INTO routes (rid,name,number,color,stations)
                     VALUES (@rid,@name,@number,@color,@stations)
                     ON DUPLICATE KEY UPDATE
                         name = VALUES(name),
                         number = VALUES(number),
                         color = VALUES(color),
                         stations = VALUES(stations);
                     """;

        HttpResponseMessage resp = await _client.GetAsync(uri);
        resp.EnsureSuccessStatusCode();
        Model.Pack p = await resp.Content.ReadFromJsonAsync<Model.Pack>();
        var ls = p.data["routes"].AsArray().ToList();


        int counter = 0;
        foreach (var r in ls)
        {
            ++counter;

            Model.Route rr = r.Deserialize<Model.Route>();
            rr.dwells = r["stations"].AsArray().Deserialize<List<Model.Dwell>>();
            if (rr == null || ls == null || rr.dwells == null)
            {
                Console.WriteLine($"Null Reference When Deserializing {r} -> Item {counter}");
                continue;
            }
            Console.Write($"Dealing Item {counter}/{ls.Count} {rr.name}({rr.id}) {rr.dwells.Count} stations");

            using (MySqlCommand cmd = new MySqlCommand(sql, dataAccessor.Connection))
            {
                cmd.Parameters.AddWithValue("@rid", rr.id);
                cmd.Parameters.AddWithValue("@name", rr.name);
                cmd.Parameters.AddWithValue("@number", rr.number);
                cmd.Parameters.AddWithValue("@color", rr.color);
                cmd.Parameters.AddWithValue("@stations", string.Join(",", JsonSerializer.Serialize(rr.dwells)));

                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine(" Done!");
            }
        }

        return;
    }
    
    /// <summary>
    /// 从线路名称获取线路
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <param name="key">关键词</param>
    /// <returns>符合关键词的线路的集合</returns>
    public static async Task<List<Model.Route>> Name2RouteObject(DataAccessor dataAccessor, string key)
    {
        string sql = """SELECT * FROM routes WHERE name LIKE @key;""";
        List<Model.Route> result = new List<Model.Route>();

        using (MySqlCommand cmd = new MySqlCommand(sql, dataAccessor.Connection))
        {
            cmd.Parameters.AddWithValue("@key", key);

            using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    result.Add(
                        new Model.Route
                        {
                            id = rd.GetString(1),
                            name = rd.GetString(2),
                            number = rd.GetString(3),
                            color = rd.GetInt32(4),
                            dwells = JsonSerializer.Deserialize<List<Model.Dwell>>(rd.GetString(5))
                        }
                    );
                }
            }
        }
        return result;
    }
    
    /// <summary>
    /// 从站点名称获取站点
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <param name="key">关键词</param>
    /// <returns>符合关键词的站点的集合</returns>
    public static async Task<List<Model.Station>> Name2StationObject(DataAccessor dataAccessor, string key)
    {
        string sql = """SELECT * FROM stations WHERE name LIKE @key;""";
        List<Model.Station> result = new List<Model.Station>();

        using (MySqlCommand cmd = new MySqlCommand(sql, dataAccessor.Connection))
        {
            cmd.Parameters.AddWithValue("@key", key);

            using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    result.Add(
                        new Model.Station
                        {
                            id = rd.GetString(1),
                            name = rd.GetString(2),
                            color = rd.GetInt32(3),
                            connections = rd.GetString(4).Split(",").ToList()
                        }
                    );
                }
            }
        }
        return result;
    }
    
    /// <summary>
    /// 从站点Id获取站点
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <param name="sid">Id</param>
    /// <returns>符合Id的唯一站点</returns>
    public static async Task<Model.Station?> Sid2StaionObject(DataAccessor dataAccessor, string sid)
    {
        string sql = """SELECT * FROM stations WHERE sid = @sid;""";
        using (MySqlCommand cmd = new MySqlCommand(sql, dataAccessor.Connection))
        {
            cmd.Parameters.AddWithValue("@sid", sid);

            using (var rd = await cmd.ExecuteReaderAsync())
            {
                if (!rd.HasRows)
                {
                    return null;
                }

                await rd.ReadAsync();
                return new Model.Station
                {
                    id = rd.GetString(1),
                    name = rd.GetString(2),
                    color = rd.GetInt32(3),
                    connections = rd.GetString(4).Split(",").ToList()
                };
            }
        }
    }

    /// <summary>
    /// 从线路Id获取线路
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <param name="sid">Id</param>
    /// <returns>符合Id的唯一线路</returns>
    public static async Task<Model.Route?> Rid2RouteObject(DataAccessor dataAccessor, string rid)
    {
        string sql = """SELECT * FROM routes WHERE rid = @rid;""";
        using (MySqlCommand cmd = new MySqlCommand(sql, dataAccessor.Connection))
        {
            cmd.Parameters.AddWithValue("@rid", rid);

            using (var rd = await cmd.ExecuteReaderAsync())
            {
                if (!rd.HasRows)
                {
                    return null;
                }

                await rd.ReadAsync();
                return new Model.Route
                {
                    id = rd.GetString(1),
                    name = rd.GetString(2),
                    number = rd.GetString(3),
                    color = rd.GetInt32(4),
                    dwells = JsonSerializer.Deserialize<List<Model.Dwell>>(rd.GetString(5))
                };
            }
        }
    }
    
    /// <summary>
    /// 从站点Id获取站点名称
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <param name="sid">Id</param>
    /// <returns>符合Id的唯一站点的名称</returns>
    public static async Task<string?> Sid2NameFromDb(DataAccessor dataAccessor, string sid)
    {
        string sql = """SELECT name FROM stations WHERE sid = @sid;""";
        using (MySqlCommand cmd = new MySqlCommand(sql, dataAccessor.Connection))
        {
            cmd.Parameters.AddWithValue("@sid", sid);

            using (var rd = await cmd.ExecuteReaderAsync())
            {
                if (!rd.HasRows)
                {
                    return null;
                }

                await rd.ReadAsync();
                return rd.GetString(0);
            }
        }
    }
    
    /// <summary>
    /// 从本地数据库获取包含站点Id,名称的字典
    /// </summary>
    /// <param name="dataAccessor">数据库访问对象</param>
    /// <returns>包含站点Id,名称的字典</returns>
    public static async Task<Dictionary<string,string>?> GetSidNameTable(DataAccessor dataAccessor)
    {
        string sql = """SELECT sid,name FROM stations;""";
        Dictionary<string, string> d = new();
        using (MySqlCommand cmd = new MySqlCommand(sql, dataAccessor.Connection))
        {
            // cmd.Parameters.AddWithValue("@sid",sid);
            using (var rd = await cmd.ExecuteReaderAsync())
            {
                if (!rd.HasRows)
                {
                    return null;
                }

                while (await rd.ReadAsync())
                {
                    d.Add(rd.GetString(0), rd.GetString(1));
                }

                return d;
            }
        }
    }

    /// <summary>
    /// 从服务器获取在线的玩家列表
    /// </summary>
    /// <param name="d">要获取的维度</param>
    /// <returns>包含玩家对象的列表</returns>
    public async Task<List<Model.Client>> FetchCilents(Dimensions d = Dimensions.OverWorld)
    {
        string uri = Host+string.Format(UrlCilents, (int)d);
        using (var resp = await _client.GetAsync(uri))
        {
            resp.EnsureSuccessStatusCode();
            var x = await resp.Content.ReadFromJsonAsync<JsonObject>();
            return (x["data"]["clients"].Deserialize<List<Model.Client>>()).Where(c=>!string.IsNullOrEmpty(c.name)).ToList();
        }
    }

    /// <summary>
    /// 获取Id指定站点的出发与到达信息
    /// </summary>
    /// <param name="stationId">要获取的站点Id</param>
    /// <param name="countPerPlat">固定参数？</param>
    /// <param name="d">要获取的维度</param>
    /// <returns>包含获取时间,信息列表的元组</returns>
    /// <exception cref="InvalidDataException"></exception>
    public async Task<(DateTimeOffset, List<Model.ArrivalPack>)> FetchArrivals(string stationId, int countPerPlat = 5,
        Dimensions d = Dimensions.OverWorld)
    {
        List<Model.ArrivalPack> r = null;
        string uri = Host + string.Format(UrlArrivals,(int)d);
        using (var resp = await _client.PostAsJsonAsync(uri,
                   new { stationIdsHex = new[] {stationId}, maxCountPerPlatform = countPerPlat }))
        {
            resp.EnsureSuccessStatusCode();
            var j = await resp.Content.ReadFromJsonAsync<JsonObject>();
            if (j["code"].GetValue<int>()!=200) throw new InvalidDataException(nameof(resp));
            var n = j["data"].AsObject()["arrivals"].AsArray();
            r = n.Select(x=>x.Deserialize<Model.ArrivalPack>()).ToList();
            return (DateTimeOffset.FromUnixTimeMilliseconds(j["data"]["currentTime"].GetValue<long>()), r);
        }
    }
}