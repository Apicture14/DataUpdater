using System;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Net.Http.Json;
using MySql.Data.MySqlClient;
using System.Text.Json;
using System.Text.Json.Nodes;
using Org.BouncyCastle.Cms;

namespace DataUpdater;

public class Fetcher
{
    public static string Host { get; set; } = @"https://tetsudou-map.haruto.city";

    private const string UA =
        @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36";

    public enum Dimensions
    {
        OverWorld = 0,
        Nether = 2,
        TheEnd = 1
    }

    [Obsolete]
    public enum FetchMode
    {
        Stations = 0,
        Routes = 1,
        StationAndRoutes = 2
    }

    private const string UrlStationAndRoutes = "/mtr/api/map/stations-and-routes?dimension={0}";
    private const string UrlCilents = "/mtr/api/map/clients?dimension=0";
    private static readonly HttpClient _client = new HttpClient();

    public Fetcher()
    {
        _client.DefaultRequestHeaders.Add("User-Agent", UA);
        _client.DefaultRequestHeaders.Add("Accept", "application/json;text/plain;charset=UTF-8");
        _client.DefaultRequestHeaders.Add("Referer", "https://tetsudou-map.haruto.city/");
    }

//Task<KeyValuePair<List<Model.Station>,List<Model.Route>>>
    public static async Task FetchStationsFromServer(MySqlConnection connection, Dimensions d = Dimensions.OverWorld)
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
                       connection))
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

    public static async Task FetchRoutesFromServer(MySqlConnection connection, Dimensions d = Dimensions.OverWorld)
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
            Console.Write($"Dealing Item {counter}/{ls.Count} {rr.name}({rr.id}) {rr.dwells.Count} stations");

            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
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

    public static async Task<List<Model.Route>> SearchRouteFromDb(MySqlConnection conn, string key)
    {
        string sql = """SELECT * FROM routes WHERE name LIKE @key;""";
        List<Model.Route> result = new List<Model.Route>();

        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
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

    public static async Task<Model.Station?> FindStationBySidFromDb(MySqlConnection conn, string sid)
    {
        string sql = """SELECT * FROM stations WHERE sid = @sid;""";
        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
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

    public static async Task<Model.Route?> FindRouteByRidFromDb(MySqlConnection conn, string rid)
    {
        string sql = """SELECT * FROM routes WHERE rid = @rid;""";
        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
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

    public static async Task<string> Sid2NameFromDb(MySqlConnection conn, string sid)
    {
        string sql = """SELECT name FROM stations WHERE sid = @sid;""";
        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@sid", sid);

            using (var rd = await cmd.ExecuteReaderAsync())
            {
                if (!rd.HasRows)
                {
                    return "";
                }

                await rd.ReadAsync();
                return rd.GetString(0);
            }
        }
    }

    public static async Task<Dictionary<string, string>?> GetSidNameTable(MySqlConnection conn)
    {
        string sql = """SELECT sid,name FROM stations;""";
        Dictionary<string, string> d = new();
        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
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

    public static async Task<List<Model.Client>> FetchCilents(Dimensions d = Dimensions.OverWorld)
    {
        string uri = Host+string.Format(UrlCilents, (int)d);
        using (var resp = await _client.GetAsync(uri))
        {
            resp.EnsureSuccessStatusCode();
            var x = await resp.Content.ReadFromJsonAsync<JsonObject>();
            return x["data"]["clients"].Deserialize<List<Model.Client>>();
        }
    }
}