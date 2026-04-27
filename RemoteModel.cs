using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Linq;
using System.IO;
using System.Text.Json.Serialization;
using DataUpdater.Core;
using DataUpdater.DAO;


namespace DataUpdater.Models;

public abstract class Model
{
    public record Station
    {
        public string id {get;set;}
        public string name {get;set;}
        public int color {get;set;}
        public List<string> connections {get;set;}
        public override string ToString()
        {
            if (string.IsNullOrEmpty(name))
            {
                return $"Null({id})";
            }
            return this.name;
        }
    }
    public record Route
    {
        public string id {get;set;}
        public string name {get;set;}
        public string number {get;set;}
        public int color {get;set;}
        public List<string> depots {get;set;}
        public List<Dwell> dwells {get;set;}
        public async Task Inspect(DataAccessor dataAccessor = null)
        {
            Console.WriteLine($"[Overall] 线路:{id} 名称:{name}[{number}] 通过{dwells.Count}个车站\n");
            int c = 0;
            Dictionary<string, string> d = (dataAccessor is null)?null:await Fetcher.GetSidNameTable(dataAccessor);
            foreach (var x in dwells)
            {
                c++;
                StringBuilder sb = new StringBuilder();
                sb.Append($"[{c}/{dwells.Count}]".PadRight(8));
                sb.Append(("名称:" + (d.TryGetValue(x.station, out string n) ? n : x.station)).PadRight(30));
                sb.Append(("站台:" + x.platform).PadRight(5));
                sb.Append($"位置:[x:{x.x} y:{x.y} z:{x.z}]");
                // t += "\n";
                Console.WriteLine(sb.ToString());
            }    
        }

        public async Task<Tuple<bool,object?>> Export(DataAccessor dataAccessor,FileStream? outputFs=null)
        {
            OutRoute outRoute = new OutRoute()
            {
                code = name.Split("|")[0],
                color = color,
                storedStations = new List<OutStation>()
            };
            foreach (var x in dwells)
            {
                var rs = await Fetcher.Sid2NameFromDb(dataAccessor, x.station);
                var s = rs.Split("|");
                outRoute.storedStations.Add(new OutStation()
                {
                    cn = s[0],
                    ncn = s[1],
                });
            }
            if (outputFs == null) return new(true,outRoute);
            using (outputFs)
            {
                if (!outputFs.CanWrite){return new(false,null);}

                await JsonSerializer.SerializeAsync(outputFs, outRoute,new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            return new (true,outRoute);
        }
    }
    public record Pack
    {
        public int code {get;set;}
        public string text {get;set;}
        public int version {get;set;}
        public JsonObject data {get;set;}
    }
    public record Dwell
    {
        public string station {get=>id;}
        public string platform {get=>name;}
        public string id {get;set;}
        public string name {get;set;}
        public int dwellTime {get;set;}
        public long x {get;set;}
        public int y {get;set;}
        public long z {get;set;}
    }
    public record Client
    {
        public MySqlConnection db { get; set; }

        public string id {get;set;}
        public string name {get;set;}
        public string routeId {get;set;}
        public string routeStationId1 {get;set;}
        public string routeStationId2 {get;set;}
        public string stationId {get;set;}
        public int x {get;set;}
        public int z {get;set;}
        public Point pos => new Point(x, z);

        public async Task Inspect(DataAccessor dataAccessor)
        {
            string s = """
                       玩家:{0}
                       uid:{1}
                       位置:[{2},{3}]
                       状态:
                       """;
            string st = "{1}\n位于车站:{0}";
            string mv = """
                        {3}
                        正在乘坐:{0}
                        从:{1}
                        到:{2}
                        """;
            s = string.Format(s, name, id, x, z);
            if (routeId == "")
            {
                st = string.Format(st, (dataAccessor == null) ? stationId : await Fetcher.Sid2StaionObject(dataAccessor, stationId) ?? new Station(){id = stationId},"At Station");
                s += st;
            }
            else
            {
                if (dataAccessor != null)
                {
                    routeId = (await Fetcher.Rid2RouteObject(dataAccessor, routeId)).name;
                    routeStationId1 = (await Fetcher.Sid2StaionObject(dataAccessor,routeStationId1)).name;
                    routeStationId2 = (await Fetcher.Sid2StaionObject(dataAccessor, routeStationId2)).name;
                }
                mv = string.Format(mv, routeId, routeStationId1, routeStationId2,"On Ride");
                s += mv;
            }
            Console.WriteLine(s);
        }

    }
    public record OutStation
    {
        public string cn { get; set; }
        public string ncn { get; set; }
    }

    public record OutRoute
    {
        public string code {get; set;}
        public string name {get; set;}
        public int color {get;set;}
        public List<OutStation> storedStations { get; set; }  
    }
    
    public struct MTRStr
    {
        public string Cjk { get; set; }
        public string NonCjk { get; set; }
        public List<string> Extras  { get; set; }

        public MTRStr(string src)
        {
            if (!src.Contains("|"))
            {
                Cjk = src;
                return;
            }
            if (src.Contains("||"))
            {
                var y = src.Split("||");
                Extras = y.Skip(1).ToList();
                src = y[0];
            }
            var x = src.Split("|");
            Cjk = x[0];
            NonCjk = x[1];
        }
    }

    public record ArrivalPack
    {
        public long arrival { get; set; }
        public long departure { get; set; }
        public string destination { get; set; }
        public bool isTerminating { get; set; }
        public string platformName {get;set;}
        public bool realTime { get; set; }
        public string routeName {get;set;}
        public string routeNumber {get;set;}
    }

    public record Arrival
    {
        public DateTimeOffset arrivalTime {get;set;}
        public DateTimeOffset departureTime {get;set;}
        public Station? destination {get;set;}
        public string destinationName {get;set;}
        public bool isTerminating {get;set;}
        public string platformName {get;set;}
        public bool isScheduled {get;set;}
        public Route? routeFrom {get;set;}
    }
}

