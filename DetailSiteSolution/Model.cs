using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DataUpdater.HCR.Types;

// Version 1.1
// Add Rank Support

public struct LangHolder
{
    public string ja { get; set; }
    public string zh { get; set; }
    public string en { get; set; }

    public LangHolder(string ja, string zh = "", string en = "")
    {
        this.zh = zh;
        this.en = en;
        this.ja = ja;
    }

    public static LangHolder allFromText(string text)
    {
        return new LangHolder(text, text, text);
    }
}

public struct ShortCompany
{
    public string abbr { get; set; }
    public string color { get; set; }
    public int id { get; set; }
    public LangHolder name { get; set; }
    public LangHolder sub { get; set; }
    public LangHolder desc { get; set; }

    [JsonIgnore]
    public Color Color
    {
        get => Color.FromArgb(Convert.ToInt32(color.TrimStart(), 16));
    }
}

public struct Member
{
    public enum EmployeeTypes
    {
        NotExist = -1,
        Other = 0, // その他
        RepresentativeDirector = 1, // 代表取締役
        RepresentativeDirectorPresident = 2, // 代表取締役社長
        Director = 3, // 取締役
        SeniorManagingDirector = 4, // 専務取締役
        ManagingDirector = 5, // 常務取締役
        Auditor = 6, // 監査役
        ExecutiveOfficer = 7, // 執行役員
        President = 8, // 社長
        Chairman = 9 // 会長
    }

    public int companyId { get; set; } = 0;
    public required int id { get; set; } = 0;
    public required string name { get; set; }
    public required int status { get; set; }
    public required int type { get; set; }
    public string typeName { get; set; } = "";

    [JsonIgnore] public EmployeeTypes memberType => (EmployeeTypes)type;
    [JsonIgnore] public string memberTypeName => memberType.ToString();

    public Member()
    {
    }

    public static Member Empty => new Member()
    {
        companyId = 0,
        id = 0,
        name = "",
        status = 0,
        type = -1,
    };
}

public struct Route
{
    public enum Ranks
    {
        UNDEFINED = 0,
        SSR_plus = 1,
        SSR = 2,
        SSR_minus = 3,
        SR_plus = 4,
        SR = 5,
        SR_minus = 6,
        R_plus = 7,
        R = 8,
        R_minus = 9,
        N = 10,
        UR = 11
    }
    public enum RouteStatus
    {
        Stopped = 4,
        Running = 1,
        Planning = 3
    }

    public int id { get; set; }
    public int companyId { get; set; }
    public LangHolder name { get; set; }
    public LangHolder from { get; set; }
    public LangHolder to { get; set; }
    public string color { get; set; }
    public int status { get; set; } // 1 for RUNNING , 3 for PLANNING , 4 for PAUSING
    public int stations { get; set; }
    [JsonIgnore] public int rank { get; set; }
    [JsonIgnore] public string rankComment { get; set; }
    [JsonIgnore] public Color Color => Color.FromArgb(Convert.ToInt32(color.TrimStart(), 16));
    [JsonIgnore] public Ranks rankInRanks => (Ranks)rank;
    [JsonIgnore] public RouteStatus statusInRouteStatus => (RouteStatus)status;
    [JsonIgnore] public bool hasComment => (rankInRanks != Ranks.UNDEFINED && !string.IsNullOrEmpty(rankComment));

    public Route(LangHolder name, LangHolder from, LangHolder to, int stations, Color color)
    {
        this.name = name;
        this.color = "#" + color.ToArgb().ToString("x6").Substring(2);
        this.from = from;
        this.to = to;
        this.stations = stations;
    }
}

public class RouteBuilder
{
    public static class RouteNameBuilder
    {
        private static readonly Func<string,bool> hasNum = (string s) =>
        {
            return s.Any(x => x >= '0' && x <= '9');
        };
        private static readonly Func<string, bool> hasLetter = (string s) =>
        {
            return s.ToLower().Any(x => x >= 'a' && x <= 'z');
        };
        private static readonly Func<char, bool> isLetter = (char c) =>
        {
            return c >= 'a' && c <= 'z';
        };

        public static string c2n2(string character,int number)
        {
            if (hasNum(character)) { throw new ArgumentException(nameof(character)); }
            return character[..Math.Min(character.Length,2)] + number.ToString("D2")[..2];
        }
        public static string n2(int number)
        {
            return number.ToString("D2");
        }
        public static string c2n2l(string character,int number,char letter)
        {
            if (!isLetter(letter)) { throw new ArgumentException(nameof(letter)); }
            return c2n2(character,number) + letter;
        }
        public static string n2l(int number,char letter)
        {
            if (!isLetter(letter)) { throw new ArgumentException(nameof(letter)); }
            return n2(number) + letter; 
        }
        public static string c2n2_dash_n2(string character,int number1,int number2)
        {
            return c2n2(character,number1) + "-" + n2(number2);
        }
    }
    private Route _route;
    private bool freezed = false;
    protected virtual void OnFreezeOperation()
    {
        throw new InvalidOperationException("对象已被冻结！");
    }
    public RouteBuilder()
    {
        _route = new Route()
        {
            companyId = -1,
            id = 0,
            color = "#ff0000",
            name = LangHolder.allFromText("线路01"),
            from = new LangHolder(),
            to = new LangHolder(),
            stations = 0,
            status = 3
        };
    }
    public RouteBuilder name(LangHolder text)
    {
        if (freezed) OnFreezeOperation();
        _route.name = text;
        return this;
    }
    public RouteBuilder name(string text)
    {
        if (freezed) OnFreezeOperation();
        _route.name = LangHolder.allFromText(text);
        return this;
    }
    public RouteBuilder name(string ja, string en = "", string zh = "")
    {
        if (freezed) OnFreezeOperation();
        _route.name = new LangHolder(ja, en, zh);
        return this;
    }
    public RouteBuilder from(LangHolder text)
    {
        if (freezed) OnFreezeOperation();
        _route.from = text;
        return this;
    }
    public RouteBuilder from(string text)
    {
        if (freezed) OnFreezeOperation();
        _route.from = LangHolder.allFromText(text);
        return this;
    }
    public RouteBuilder from(string ja, string en = "", string zh = "")
    {
        if (freezed) OnFreezeOperation();
        _route.from = new LangHolder(ja, en, zh);
        return this;
    }
    public RouteBuilder to(LangHolder text)
    {
        if (freezed) OnFreezeOperation();
        _route.to = text;
        return this;
    }
    public RouteBuilder to(string text)
    {
        if (freezed) OnFreezeOperation();
        _route.to = LangHolder.allFromText(text);
        return this;
    }
    public RouteBuilder to(string ja, string en = "", string zh = "")
    {
        if (freezed) OnFreezeOperation();
        _route.to = new LangHolder(ja, en, zh);
        return this;
    }
    public RouteBuilder color(Color color)
    {
        if (freezed) OnFreezeOperation();
        _route.color = "#" + color.ToArgb().ToString("x6").Substring(2);
        return this;
    }
    public RouteBuilder color(ushort A,ushort R,ushort G,ushort B)
    {
        // if (freezed) OnFreezeOperation();
        return color(Color.FromArgb(A,R,G,B));
    }
    public RouteBuilder color(string colorString)
    {
        colorString = colorString.ToLower();
        Func<string,bool> isValid = (string s) =>
        {
            if (s.Length < 6 || s.Length > 7) return false;
            foreach(var c in s)
            {
                if (c >= 'a' && c <= 'z') continue;
                if (c >= '0' && c <= '9') continue;
                if (c == '#') continue;
                return false;
            }
            return true;
        };
        if (freezed) OnFreezeOperation();
        if (!isValid(colorString)) throw new ArgumentException("Invalid Color String");
        _route.color = colorString.StartsWith("#") ? colorString : "#" + colorString;
        return this;
    }
    public RouteBuilder station(uint count)
    {
        if (freezed) OnFreezeOperation();
        _route.stations = (int)count;
        return this;
    }
    public RouteBuilder status(Route.RouteStatus status)
    {
        if (freezed) OnFreezeOperation();
        _route.status = (int)status;
        return this;
    }
    public RouteBuilder companyId(uint id)
    {
        if (freezed) OnFreezeOperation();
        _route.companyId = (int)id;
        return this;
    }
    public RouteBuilder id(uint id)
    {
        if (freezed) OnFreezeOperation();
        _route.id = (int)id;
        return this;
    }
    public void freeze()
    {
        freezed = true;
    }
    public Route build(bool reuse=false)
    {
        freezed = !reuse;
        return _route;
    }

}

public struct FullCompany
{
    public string abbr { get; set; }
    public string color { get; set; }
    public int id { get; set; }
    public int companyType { get; set; }
    public LangHolder name { get; set; }
    public LangHolder sub { get; set; }
    public LangHolder desc { get; set; }

    [JsonIgnore]
    public Color Color
    {
        get => Color.FromArgb(Convert.ToInt32(color.TrimStart(), 16));
    }

    public List<Route> routes { get; set; }
    public List<Member> cm { get; set; }

    public Member.EmployeeTypes HasEmployee(int id)
    {
        return this.cm.FirstOrDefault(x => x.id == id).memberType;
    }

    public Member.EmployeeTypes HasEmployee(string usrname)
    {
        return this.cm.FirstOrDefault(x => x.name == usrname).memberType;
    }
}