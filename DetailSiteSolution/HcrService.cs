using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DataUpdater.HCR.Types;
using HtmlAgilityPack;


namespace DataUpdater.HCR.Core;

public class HcrService
{
    private HttpClient _httpClient;
    private string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"; 

    private string token = "";
    public bool LoggedIn => !string.IsNullOrEmpty(token);

    public static class ServiceUrls
    {
        public static string Base = "https://content.haruto.city";
        public static string RefererBase = "https://hcr.haruto.city";
        public static string Login => Base + "/login_hwl"; 
        public static string EditCompany => Base + "/api/detail/edit_company";
        public static string EditLine => Base + "/api/detail/edit_line";
        public static string Companys => Base + "/system/detail_list?company_type={0}";   
        public static string EditMember => Base + "/api/detail/edit_member"; 
        public static string DeleteMember => Base + "/api/detail/delete_member";
    }
    private enum MemberStates
    {
        InPosition,
        OutOfPosition
    }
   

    public HcrService(int requestTimeout = 10)
    {
        this._httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(requestTimeout),
        };
        _httpClient.DefaultRequestHeaders.Add("UserAgent",userAgent);
        _httpClient.DefaultRequestHeaders.Referrer = new Uri(ServiceUrls.RefererBase);
        _httpClient.DefaultRequestHeaders.Add("Origin",ServiceUrls.RefererBase);
    }
    public static (int Uid,DateTimeOffset Starts, DateTimeOffset Ends) GetTokenInfo(string token)
    {
        string v = token.Split(".")[1];
        var r = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(v + new string('=', Math.Abs(v.Length % 4 - 4))));
        var j = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(r);
        // Console.WriteLine(r);
        return (
            Uid: j["user_id"].GetValue<int>(),
            Starts: DateTimeOffset.FromUnixTimeSeconds(j["iat"].GetValue<int>()).ToLocalTime(),
            Ends: DateTimeOffset.FromUnixTimeSeconds(j["exp"].GetValue<int>()).ToLocalTime()
        );
    }

    public async Task<T> WithAuth<T>(Func<Task<T>> task)
    {
        try
        {
            if (!LoggedIn) throw new AuthenticationException("在未登录的情况下执行权限操作！");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);

            return await task();
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }
    }

    public async Task<JsonObject> EnsureSuccess(HttpResponseMessage r)
    {
        r.EnsureSuccessStatusCode();

        // Console.WriteLine(await r.Content.ReadAsStringAsync());

        var j = await r.Content.ReadFromJsonAsync<JsonObject>();
        if (j == null || j["code"] == null) throw new InvalidDataException("错误的响应");
        if (j["code"]!.GetValue<int>()!=0) throw new InvalidOperationException($"获取失败 {j["code"].GetValue<int>().ToString()} {(j["code"].GetValue<int>()==-1?j["message"].GetValue<string>() : "No Info")}");
        return j;
    }

    public bool ValidateResult(JsonObject j)
    {
        if (j["code"] != null)
        {
            return j["code"]!.GetValue<int>() == 0;
        }
        return false;
    }
    
    public async Task<string> Login(string usr,string pwd)
    {
        try
        {    
            var f = new MultipartFormDataContent();
            f.Add(new StringContent(usr),"username");
            f.Add(new StringContent(pwd),"password");

            var resp = await _httpClient.PostAsync(ServiceUrls.Login,f);
            resp.EnsureSuccessStatusCode();
            var ro = await resp.Content.ReadFromJsonAsync<JsonObject>();
            
            if (ro == null) throw new InvalidDataException("错误的响应");
            if (ro["code"]!.GetValue<int>()!=0) throw new InvalidOperationException($"登录失败 {ro["code"].GetValue<int>()}");

            token = ro["data"]!["token"]!.GetValue<string>(); 

            return token;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Add("ContentType","application/json");
        }

    }

    public void LoginWithToken(string token)
    {
        this.token = token;

        //var h = _httpClient.DefaultRequestHeaders;

        //CookieContainer cc = new CookieContainer();
        //cc.Add(new Uri("https://hcr.haruto.city"),new Cookie("haruto_cookie", token));


        //HttpClientHandler hc = new HttpClientHandler()
        //{
        //    CookieContainer = cc,
        //    UseCookies = true
        //};

        //_httpClient = new HttpClient(hc); 
    }
    

    public async Task<List<FullCompany>> GetAllCompanies(bool isBus = false)
    {
        using (var resp = await _httpClient.GetAsync(string.Format(ServiceUrls.Companys, isBus ? 2 : 0)))
        {
           var j = await EnsureSuccess(resp);

            return j["data"].Deserialize<List<FullCompany>>();
            
        }
    }

    public async Task<List<Route>> GetAllRoutes(bool isBus = false)
    {
        using (var resp = await _httpClient.GetAsync(string.Format(ServiceUrls.Companys, isBus ? 2 : 0)))
        {
           var j = await EnsureSuccess(resp);

           var x = j["data"].Deserialize<List<FullCompany>>();

           return x!.Where(x=>x.routes != null).SelectMany(x=>x.routes).ToList();
        }
    }

    [LoginRequired]
    public async Task<bool> EditCompanyById(ushort companyId,ShortCompany sc)
    {
        sc.id = companyId;
        return await WithAuth<bool>(async () =>
        {
            using (var resp = await _httpClient.PostAsJsonAsync(ServiceUrls.EditCompany, sc))
            {
                return ValidateResult(await EnsureSuccess(resp));
            }
        });
    }   

    [LoginRequired]
    public async Task<bool> EditLineById(ushort companyId,ushort routeId, Route newInfo)
    {
        newInfo.id = routeId;
        newInfo.companyId = companyId;
        
        HttpRequestMessage req = new HttpRequestMessage();
        req.Method = HttpMethod.Post;
        req.RequestUri = new Uri(ServiceUrls.EditLine);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer",token);
        req.Content = JsonContent.Create(newInfo,MediaTypeHeaderValue.Parse("application/json"),new JsonSerializerOptions(){Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping});
        // req.Content = new StringContent(t);
        req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        Console.WriteLine(await req.Content.ReadAsStringAsync());
        req.Headers.Add("Origin","https://hcr.haruto.city");
        req.Headers.Referrer = new Uri("https://hcr.haruto.city");

        using (var resp = await _httpClient.SendAsync(req))
        {
            return ValidateResult(await EnsureSuccess(resp));
        }
    }

    [LoginRequired]
    public async Task<bool> EditMemberById(ushort companyId, Member member)
    {
        return await WithAuth(async () =>
            {
                using (var resp = await _httpClient.PostAsJsonAsync(ServiceUrls.EditMember, member))
                {
                    return  ValidateResult(await EnsureSuccess(resp));
                }
            });
    }

    [LoginRequired]
    public async Task<bool> DeleteMemberById(ushort memberId)
    {
        return await WithAuth(async () =>
        {
            using (var resp = await _httpClient.PostAsJsonAsync(ServiceUrls.DeleteMember, new { id = memberId }))
            {
                return ValidateResult(await EnsureSuccess(resp));
            }
        });
    }
    
}