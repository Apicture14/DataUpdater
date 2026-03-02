using System;
using System.Drawing;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace DataUpdater;

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
        public async Task Inspect(MySqlConnection db=null)
        {
            Console.WriteLine($"[Overall] 线路:{id} 名称:{name}[{number}] 通过{dwells.Count}个车站\n");
            int c = 0;
            Dictionary<string, string> d = (db==null)?null:await Fetcher.GetSidNameTable(db);
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

        public async Task<OutRoute> ExportToObject(MySqlConnection c)
        {
            OutRoute outRoute = new OutRoute()
            {
                code = (number==""?name.Split("|")[0]:number),
                color = color,
            };
            foreach (var x in dwells)
            {
                
                var rs = await Fetcher.Sid2NameFromDb(c, x.name);
                var s = rs.Split("|");
                outRoute.outStations.Add(new OutStation()
                {
                    cn = s[0],
                    ncn = s[1],
                });
            }
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

        public async Task Inspect(MySqlConnection db)
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
                st = string.Format(st, (db == null) ? stationId : await Fetcher.FindStationBySidFromDb(db, stationId),"At Station");
                s += st;
            }
            else
            {
                
                if (db != null)
                {
                    routeId = (await Fetcher.FindRouteByRidFromDb(db, routeId)).name;
                    routeStationId1 = (await Fetcher.FindStationBySidFromDb(db,routeStationId1)).name;
                    routeStationId2 = (await Fetcher.FindStationBySidFromDb(db, routeStationId2)).name;
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
        public List<OutStation> outStations { get; set; }  
    }
}

