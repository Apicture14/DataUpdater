// See https://aka.ms/new-console-template for more information
using DataUpdater;
using System.Text.Json;
using System.Reflection;

DAO d = new DAO();

Console.Write("输入要查询的线路名称 (_=?,%=*):");
var q = Console.ReadLine();
if (q.StartsWith("_C"))
{
    var c = (await Fetcher.FetchCilents()).Where(x=>x.name!="").ToList();
    int cc = 0;
    Console.WriteLine($"找到{c.Count}个玩家");
    foreach (var p in c)
    {
        Console.WriteLine($"[{cc}] {p.name}");
        cc++;
    }
    Console.Write("输入索引:");
    if (Int32.TryParse(Console.ReadLine(),out int i) && i < c.Count && i >= 0)
    {
        await c[i].Inspect(d.Connection);
    } 
}
else 
{
    var r = await Fetcher.SearchRouteFromDb(d.Connection, $"{q}");
    Console.WriteLine($"找到{r.Count}条线路");
    if (r.Count == 0) return;
    if (r.Count == 1)
    {
        await r.First().Inspect();
    }
    else
    {
        int idx = 0;
        foreach (var rx in r)
        {
            Console.WriteLine($"[{idx}]{rx.name} ({rx.number})");
            idx++;
        }

        Console.Write("输入索引:");
        int i;
        if (int.TryParse(Console.ReadLine(), out i))
        {
            await r[i].Inspect(d.Connection);
        }
    }
}
