# Haruto City API List

* <u>**提供的所有API完整URL是每部分的Location + 提供的API (如果不以http开头)**</u>

* <u>**所有未经验证的内容以*斜体*表示**</u>

## Part I . MTR Map APIs

本部分为从MTR浏览器地图提供的API

Location:  https://tetsudou-map.haruto.city

### 1 站点和线路 Stations And Routes

```text
/mtr/api/map/stations-and-routes?dimension={0}
```

#### Method 

Get

#### Parameters

{0} : 要获取的维度,维度枚举的值之一.

#### Returns

```json
{
    "code": 200,
    "currentTime": 123456,
    "text": "xxxx",
    "version": 1,
    "data": {
        "stations": [
            #Station对象的列表 
        ],
        "routes": [
           	#Route对象的列表
        ]
    }
    
}
```

* code : 状态码
* currentTime : 返回数据的时间戳 ms
* sations : 服务器所有的站点
* routes: 服务器所有的线路
* *version : 版本*		

> Station Route 对象参见Part3



### 2 站点到达出发时间 Arrivals And Departures

```text
/mtr/api/map/arrivals?dimension={0}
```

#### Method 

Post

#### Parameters

```json
{
    "stationIdsHex":["3FAADB50B112ADDE"],
 	"maxCountPerPlatform":5
}
```

* stationIdHex : 查询的车站id **包裹在列表中**
* *maxCountPerPlatform : 每站台的最大数量*

#### Returns

```json
{
    "code": 200,
    "currentTime": 12345,
    "text": "xxxx",
    "version": 1,
    "data": {
        "currentTime":67890,
        "arrivals":[
            #Arrival对象列表
        ]
    }
}
```

* code : 状态码
* currentTime : 返回数据的时间戳 ms
* arrivals : 所有的到达出发数据
* *version : 版本*





### 3 在线客户端 Clients

```
/mtr/api/map/clients?dimension={0}
```

#### Method 

Get

#### Parameters

{0} : 获取的维度

#### Returns

```json
{
    "code": 200,
    "currentTime": 1777384158716,
    "text": "OK - clients",
    "version": 1,
    "data": {
        "cachedResponseTime": 1777384158716,
        "clients": [
            {
                "id": "2240dbd0-e600-359c-8fcc-1ceee56f9094",
                "name": "",
                "x": -20191,
                "z": -19505,
                "stationId": "895A77C788006A0C",
                "routeId": "",
                "routeStationId1": "",
                "routeStationId2": ""
            },
           	#...Client对象列表
        ]
    }
}
```

* clients : 查询维度玩家列表

**使用时应过滤所有没有名字的幽灵玩家**

## Part II . Server APIs

服务器提供的api

### *0 全部的站点名称*

```
https://tetsudou.haruto.city/stations.json
```

#### Method

Get

#### Returns

```json
[
    "大龙山|Tai Lung Shan",
    "广淮村|GuangHuaiCun",
    "千森大矿场|Chin Sum Quarry",
    "千森大道西|Chin Sum Avenue West",
    "十字[SU-23]|Jūji",
    "樱花林|Ying Fa Lum",
    ...
]
```

*以Json列表形式返回全部的站名*

### 1 定期卷

```
https://api.tetsudou.haruto.city/pass_search
```

#### Method 

Post

#### Parameters

```json
{
    "departureStation":"小卓|Otaku",
    "arrivalStation":"浮水|FooSui",
    "durationDays":7
}
```

* departureStation : 出发站名
* arrivalStation : 终到站名
* durationDays : 定期卷有效期 <u>**仅支持7天**</u>

#### Returns

```json
{
    "methodList": [
        {
            "route": [
                {
                    "company_name": "花丸鉄道",
                    "line_name": "",
                    "start_station": "小卓|Otaku",
                    "end_station": "遗忘十字|Forgotten Crossroad"
                },
                {
                    "company_name": "花丸鉄道",
                    "line_name": "",
                    "start_station": "遗忘十字|Forgotten Crossroad",
                    "end_station": "浮水|FooSui"
                }
            ],
            "UniversityStudentFare": 13448,
            "StudentFare": 11767,
            "childFare": 8405,
            "regularFare": 16810
        }
    ],
    "error": 0
}
```

* error : 错误信息或代码 **<u>!!这里有时返回int有时是string有时没有!!</u>**

* *methodList : 从起点到终点的方式列表*
* regularFare : 通常票价
* childFare : 儿童票价
* StudentFare : 学生票价
* UnversityStudentFare : 大学生票价

## Part III . HCR APIs

Location : https://content.haruto.city

标*的为需要Token，登录的api

### 0 登录 (so-called HWL)

```
/login_hwl
```

#### Method 

Post

#### Parameters

```
--{2}
Content-Disposition: form-data; name="username"

{0}
--{2}
Content-Disposition: form-data; name="password"

{1}
--{2}--
```

> **本接口接受表单数据**

> **要发送表单 你的Content-Type应该为**

> **`multipart/form-data; boundary={2}`**

{0} : 用户名

{1} : 密码

{2} : 表单分隔符 (可自定义)

#### Sample

`content-type`

`multipart/form-data; boundary=----WebKitFormBoundarygS3wvos7aMmRtPNJ`

```
------WebKitFormBoundarygS3wvos7aMmRtPNJ
Content-Disposition: form-data; name="username"

Steve
------WebKitFormBoundarygS3wvos7aMmRtPNJ
Content-Disposition: form-data; name="password"

12345
------WebKitFormBoundarygS3wvos7aMmRtPNJ--
```

#### Returns

```json
{
    "code": 0,
    "data": {
        "token": "-----"
    }
}
```

* code : 状态码 **失败返回-1和message字段**
* token : token

##### Token分析

```
eyJhbGciOiJIUzI1NiIsInRxxxxxxxxxxxxx.eyJ1c2VyX2lkIjo1LCJleHAiOjE3NzxxxxxxxxxsImlhdCI6MTc3NzY0NTA1OX0._Yp2a-tnXJGPaBcwOvimS0xxxxxxxxxxxxxxxxxxxxx
```

得到的Token为1个由两个`.` 分割的Base64字符串 [JWT Token格式](https://zhuanlan.zhihu.com/p/1932939645169164990)

以`.`分割成三段再将中间段长度用`=`补齐到4的倍数

Base64解码得

```json
{
    "user_id":114514,
    "exp":1778249859,
    "iat":1777645059
}
```

| Field   | Description        |
| ------- | ------------------ |
| user_id | 网站用户id         |
| exp     | token过期时间戳 ms |
| iat     | token发放时间戳 ms |

##### Token使用

在请求头中加入

`Authorization`值为

```
Bearer
{0}
```

{0} : 你的token

> 这一步推荐使用web库或工具,手拼字符串大概率不识别

### 1 获取会社列表

```
/system/detail_list?company_type={0} 
```

#### Method

Get

#### Parameters

{0} : 会社类型 参见第四部分

#### Returns

```json
{
    "code": 0,
    "data": [
        {
            "id": 2,
            "color": "#001eff",
            "abbr": "HRT",
            "name": {
                "ja": "晴都臨海高速鉄道",
                "zh": "晴都临海高速铁道",
                "en": "Haruto Rinkai Rapid Railway Ltd."
            },
            "sub": {
                "ja": "",
                "zh": "",
                "en": ""
            },
            "desc": {
                "ja": "",
                "zh": "",
                "en": ""
            },
            "routes": [
                {
                    "id": 1,
                    "companyId": 2,
                    "name": {
                        "ja": "りんかい線",
                        "zh": "临海线",
                        "en": "Rinkai Line"
                    },
                    "color": "#0008ff",
                    "status": 1,
                    "stations": 9,
                    "from": {
                        "ja": "新木場",
                        "zh": "新木厂",
                        "en": "Shin-kiba"
                    },
                    "rank": 11,
                    "rankComment": "333",
                    "to": {
                        "ja": "河原町",
                        "zh": "河原町",
                        "en": "Kawaramachi"
                    }
                }
            ],
            "cm": [
                {
                    "id": 1,
                    "companyId": 2,
                    "type": 1,
                    "typeName": "",
                    "name": "NakasuKasumi1",
                    "status": 1
                },
               	// Member 对象列表
            ],
            "companyType": 0
        },
        // FullComany对象列表
     ]
}
        
```

* data : 以FullComany对象列表表示的全部当前种类会社

> 参见PART 4

### *2 会社修改 EditCompany

```
/api/detail/edit_company
```

**本接口需要Token使用**

#### Method

Post

#### Parameters

原始请求数据

```json
{
    "id":0,
    "name":{
        "ja":"TestJP",
        "zh":"TestZH",
        "en":"TestEN"
    },
    "sub":{
        "ja":"", // 留空时应该为此种形式
        "zh":"", //
        "en":""  //
    },
    "desc":{
        "ja":"",
        "zh":"",
        "en":""
    },
    "abbr":"123",
    "color":"#007229",
    "companyType":0
}
```

精简为

```c#
public struct ShortCompany
{
    public string abbr { get; set; }
    public string color { get; set; }
    public int id { get; set; }
    public LangHolder name { get; set; }
    public LangHolder sub { get; set; }
    public LangHolder desc { get; set; }
    public int companyType { get; set; }
}
```

> HCR网站上会较多用到LangHolder结构 请参阅第四部分

| Field       | Type   | Desc                                         |
| ----------- | ------ | -------------------------------------------- |
| abbr        | string | 会社简写 **必须**                            |
| color       | string | 会社颜色 **必须**                            |
| id          | number | 会社id **修改时填要修改的会社id，新建时填0** |
| name        | object | 表示会社名称文字的json对象                   |
| sub         | object | 表示会社小标题文字的json对象                 |
| desc        | object | 表示会社描述文字的json对象                   |
| companyType | number | 会社种类 参见第四部分                        |

从铁道会社转为巴士会社时需保证所有线路名称满足<u>要求</u>

详情查看***6 线路修改**

#### Returns 

```json
{
	"code":0,
	"message":"success"
}
```

* code : 状态码 **错误返回-1**
* message : 信息

### **3 删除会社 DeleteCompany*

```
/api/detail/delete_company
```

<u>***似乎有Bug***</u>

#### Method

Post

#### Parameters

```json
{	
	"id":98
}
```

* id : 要删除的会社id

#### Returns

```json
{
  "code": 0,
  "message": "success"
}
```

失败返回-1和提示信息

### *4 会社成员修改 EditMember

```
/api/detail/edit_member
```

#### Method

Post

#### Parameters

```json
{
  "id": 1919,
  "companyId": 1145,
  "type": 0,
  "typeName": "Pet",
  "name": "柰子",
  "status": 1
}
```

就是一个Member对象,在此不多赘述

id为要修改的成员的id(当已存在) 或者为0(新增成员)

#### Returns

```json
{
  "code": 0,
  "message": "success"
}
```

失败返回-1与提示信息

### *5 删除会社成员 DeleteMember

```
/api/detail/delete_member
```

#### Method

Post

#### Parameters

```json
{	
	"id":98
}
```

* id : 要删除的成员id

#### Returns

```json
{
  "code": 0,
  "message": "success"
}
```

失败返回-1和提示信息

### *6 线路修改 EditLine

```
/api/detail/edit_line
```

#### Method

Post

#### Parameters

```
{
  "id": 51,
  "companyId": 3,
  "name": {
    "ja": "2",
    "zh": "2",
    "en": "2"
  },
  "from": {
    "ja": "",
    "zh": "",
    "en": ""
  },
  "to": {
    "ja": "",
    "zh": "",
    "en": ""
  },
  "color": "#0068b7",
  "status": 2,
  "stations": 0
}
```

一个没有rank和rankComment的Route(HCR)对象，不再过多赘述

id为0时新建线路

当添加巴士线路时,线路名称格式应为一下之一

![image-20260502161444875](https://github.com/Apicture14/DataUpdater/blob/master/image-20260502161444875.png?raw=true)

### *7 删除线路 DeleteLine

```
/api/detail/delete_line
```

#### Method

Post

#### Parameters

```json
{	
	"id":98
}
```

* id : 要删除的线路id

#### Returns

```json
{
  "code": 0,
  "message": "success"
}
```

失败返回-1和提示信息

## Part IV . 常量与枚举 (Constants And Enums)

### 枚举 Enums

#### 维度枚举

| Dimension          | Value |
| ------------------ | ----- |
| OVERWORLD (主世界) | 0     |
| THE END (末地)     | 1     |
| THE NETHER (下界)  | 2     |

#### 线路类型枚举

| Type | Value            |
| ---- | ---------------- |
| 普通 | train_normal     |
| 轻铁 | train_light_rail |
|      |                  |

#### 环线状态枚举

| Type       | Value         |
| ---------- | ------------- |
| 非环线     | NONE          |
| 顺时针环线 | CLOCKWISE     |
| 逆时针环线 | ANTICLOCKWISE |

------

下侧为HCR网站枚举

#### 交通类型枚举(会社种类)

| Type | Value |
| ---- | ----- |
| 铁路 | 0     |
| 巴士 | 2     |

#### 线路评级枚举

| Type | Value |
| ---- | ----- |
| UR   | 11    |
| N    | 10    |
| R-   | 9     |
| R    | 8     |
| R+   | 7     |
| SR-  | 6     |
| SR   | 5     |
| SR+  | 4     |
| SSR- | 3     |
| SSR  | 2     |
| SSR+ | 1     |

#### 成员职位枚举

| Type           | Value |
| -------------- | ----- |
| 其他           | 0     |
| 代表取締役     | 1     |
| 代表取締役社長 | 2     |
| 取締役         | 3     |
| 専務取締役     | 4     |
| 常務取締役     | 5     |
| 監査役         | 6     |
| 執行役員       | 7     |
| 社長           | 8     |
| 会長           | 9     |

#### 线路状态枚举

| Type             | Value |
| ---------------- | ----- |
| RUNNING 正常运营 | 1     |
| BUILDING 建设中  | 2     |
| PLANNING 计划中  | 3     |
| STOPPED 已停止   | 4     |

### 常量 Constants And Structures

#### Station

```c#
public struct Station
{
    public string id {get;set;} 
    public string name {get;set;} 
    public int color {get;set;} 
    public List<string> connections {get;set;}
}
```

| Property  | Description              |
| --------- | ------------------------ |
| id        | 车站的唯一Id             |
| name      | 车站的名称               |
| color     | 十进制表示的颜色值       |
| conntions | 与之相连的车站的id的列表 |

```json
{
    "id": "534ED2232A37CE73",
    "name": "天馬|Tenma|T-16",
    "color": 16777215,
    "zone1": 1,  //zone与收费区相关，已弃用
    "zone2": 0,	 //
    "zone3": 0,	 //
    "connections": [
        "C150DA81655FDA9D",
        "4F5C306695690F22",
        "21363ABDD0817192"
    ]
}
```

#### Route (MTR)

```c#
public record Route
{
    public string id {get;set;}
    public string name {get;set;}
    public string number {get;set;}
    public int color {get;set;}
    public string type {get;set;}
    public string circularState {get;set;}
    public List<string> depots {get;set;}
    public bool hidden {get;set;}
    public List<Dwell> stations {get;set;}
    public List<int> durations {get;set;}
}
```

| Property      | Description                          |
| ------------- | ------------------------------------ |
| id            | 线路的唯一Id                         |
| name          | 线路的名称                           |
| number        | 线路编号                             |
| color         | 十进制表示的颜色值                   |
| depots        | 线路车厂 以字车厂名称 字符串列表表示 |
| type          | 线路类型 参见枚举                    |
| circularState | 环线状态 参见枚举                    |
| hidden        | 是否隐藏                             |
| stations      | 所有停站信息 以Dwell对象列表表示     |
| durations     | 两站之间的行程时间 以毫秒记          |

```json
{
    "id": "10B325E2036302BA",
    "name": "虹ヶ咲線|清水ヶ島方面",
    "color": 16711838,
    "number": "団体",
    "type": "train_normal",
    "circularState": "NONE",
    "hidden": false,
    "stations": [
        {
            "id": "F5E0F63385FE309C",
            "x": 734,
            "y": 8,
            "z": 21280,
            "name": "1",
            "dwellTime": 16700
        },
        # ... Dwell对象列表
    ],
    "durations": [
        102960,
        53995
    ],
    "depots": [
        "虹咲线"
    ]
}
```

#### Dwell

```c#
public struct Dwell
{
    public string id {get;set;}
    public string name {get;set;}
    public int dwellTime {get;set;}
    public long x {get;set;}
    public int y {get;set;}
    public long z {get;set;}
}
```

| Property  | Description       |
| --------- | ----------------- |
| id        | 停车车站的id      |
| name      | 停车的站台名称    |
| dwellTime | 停车时间 以毫秒记 |
| x         | 停车站台的x坐标   |
| y         | 停车站台的y坐标   |
| z         | 停车站台的z坐标   |

```json
{
    "id": "F5E0F63385FE309C",
    "x": 734,
    "y": 8,
    "z": 21280,
    "name": "1",
    "dwellTime": 16700
}	
```

#### Arrival

##### Definitions

> 属性太多，我仅提供已知作用的

```json
{
    "destination": "浮水|FooSui",
    "arrival": 1777384841745,
    "dwellTIme": -1,
    "routeTypeColor": 13643626,
    "departure": 1777384858445,
    "deviation": -38661868,
    "realtime": true,
    "departureIndex": 3,
    "isTerminating": true,
    "routeId": 3112341273829401428,
    "destinationStationNumber": "",
    "destinationStationRouteColor": 0,
    "routeName": "2号线|Line 2||Out Bound",
    "routeNumber": "普通|Local",
    "routeColor": 12784106,
    "circularState": "NONE",
    "platformId": 551091618040428305,
    "platformName": "1",
    "cars": [
        {
            "vehicleId": "sp1900_cab_1",
            "occupancy": 0.0
        },
        {
            "vehicleId": "sp1900_trailer",
            "occupancy": 0.0
        },
        {
            "vehicleId": "sp1900_trailer",
            "occupancy": 0.0
        },
        {
            "vehicleId": "sp1900_cab_2",
            "occupancy": 0.0
        }
    ],
    "isStop": true
}
```

| Property                       | Description                  |
| ------------------------------ | ---------------------------- |
| arrival                        | 到站时间戳 ms                |
| departure                      | 出发时间戳 ms                |
| destination                    | 终点站                       |
| isTerminating                  | 是否终到                     |
| platformName                   | 到达的站台                   |
| realTime                       | 是否已经发车                 |
| routeName                      | 线路名称                     |
| routeNumber                    | 线路号码                     |
| deviation                      | 到站时间差 ms (早点-或晚点+) |
| routeId                        | 十进制线路id                 |
| routeColor                     | 十进制线路颜色               |
| circularState                  | 环线状态                     |
| *destinationStationRouteColor* | *在换乘站显示为当前线路颜色* |
|                                |                              |
#### Client

##### Definitions

```c#
public record Client
{
    public string id {get;set;}
    public string name {get;set;}
    public string routeId {get;set;}
    public string routeStationId1 {get;set;}
    public string routeStationId2 {get;set;}
    public string stationId {get;set;}
    public int x {get;set;}
    public int z {get;set;}
}
```

| Property        | Description                |
| --------------- | -------------------------- |
| id              | 玩家的uuid                 |
| name            | 玩家名称                   |
| routeId         | 玩家所乘坐的线路id         |
| routeStationId1 | 玩家乘坐线路区间起点站的id |
| routeStationId2 | 玩家乘坐线路区间终点站的id |
| stationId       | 玩家位于车站的车站id       |
| x               | 玩家x坐标                  |
| z               | 玩家z坐标                  |

#### LangHolder

##### Definitions

```C#
public struct LangHolder
{
    public string ja { get; set; }
    public string zh { get; set; }
    public string en { get; set; }
}
```

| Property | Description |
| -------- | ----------- |
| ja       | 日语文字    |
| zh       | 中文文字    |
| en       | 英语文字    |

##### Serialization

```json
{
    "ja": "Ja Text",
    "zh": "Zh Text",
    "en": "En Text"
}
```

#### Route (HCR)

##### Definition

```c#
public struct Route{
	public int id { get; set; }
    public int companyId { get; set; }
    public LangHolder name { get; set; }
    public LangHolder from { get; set; }
    public LangHolder to { get; set; }
    public string color { get; set; }
    public int status { get; set; } // 1 for RUNNING , 3 for PLANNING , 4 for PAUSING
    public int stations { get; set; }
    public int rank { get; set; }
    public string rankComment { get; set; }
}
```

| Property    | Description                  |
| ----------- | ---------------------------- |
| id          | 线路id                       |
| companyId   | 所属会社id                   |
| name        | 线路名称文字的LangHolder对象 |
| from        | 起点站文字的LangHolder对象   |
| to          | 终点站文字的LangHolder对象   |
| color       | #rrggbb格式十六进制色值      |
| status      | 线路状态 查阅枚举            |
| stations    | 车站数                       |
| rank        | 线路评级 查阅枚举            |
| rankComment | 线路评级留言                 |

##### Serialization

```json
{
    "id": 4,
    "companyId": 4,
    "name": {
        "ja": "東西線",
        "zh": "东西线",
        "en": "Touzai Line"
    },
    "color": "#00a1d8",
    "status": 1,
    "stations": 23,
    "from": {
        "ja": "西井",
        "zh": "西井",
        "en": "Nishii"
    },
    "rank": 11,
    "rankComment": "good",
    "to": {
        "ja": "桜田",
        "zh": "樱田",
        "en": "Sakurada"
    }
}
```



#### ShortCompany

##### Definition

```c#
public struct ShortCompany
{
    public string abbr { get; set; }
    public string color { get; set; }
    public int id { get; set; }
    public LangHolder name { get; set; }
    public LangHolder sub { get; set; }
    public LangHolder desc { get; set; }
    public int companyType { get; set; }
}
```

> HCR网站上会较多用到LangHolder结构

| Property    | Type   | Desc                                         |
| ----------- | ------ | -------------------------------------------- |
| abbr        | string | 会社简写 **必须**                            |
| color       | string | 会社颜色 **必须**                            |
| id          | number | 会社id **修改时填要修改的会社id，新建时填0** |
| name        | object | 表示会社名称文字的json对象                   |
| sub         | object | 表示会社小标题文字的json对象                 |
| desc        | object | 表示会社描述文字的json对象                   |
| companyType | number | 会社种类 参见第四部分                        |

#### FullCompany

##### Definition

```c#
public struct FullCompany
{
    public string abbr { get; set; }
    public string color { get; set; }
    public int id { get; set; }
    public int companyType { get; set; }
    public LangHolder name { get; set; }
    public LangHolder sub { get; set; }
    public LangHolder desc { get; set; }
    public List<Route> routes { get; set; }
    public List<Member> cm { get; set; }
}
```

> 在ShortComany中有的内容不过多赘述

| Property | Type  | Desc             |
| -------- | ----- | ---------------- |
| routes   | Array | 会社下的所有线路 |
| cm       | Array | 会社成员         |

##### Serialization

```json
{
    "id": 2,
    "color": "#001eff",
    "abbr": "HRT",
    "name": {...}, // LangHolder
    "sub": {...},  // LangHolder
    "desc": {...}, // LangHolder
    "routes": [
        ... // Route (HCR)
    ],
    "cm": [
        {
            "id": 1,
            "companyId": 2,
            "type": 1,
            "typeName": "",
            "name": "NakasuKasumi1",
            "status": 1
        },
       	... // Member
    ],
    "companyType": 0
}
```

#### Member

##### Definition

```c#
public struct Member{
	public int companyId { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public int status { get; set; }
    public int type { get; set; }
    public string typeName { get; set; }
}
```

| Property  | Description                |
| --------- | -------------------------- |
| companyId | 会社id                     |
| id        | 成员id                     |
| name      | 成员名称                   |
| status    | 1 在任 \| 0 退任           |
| type      | 职位 查阅枚举              |
| typeName  | 当职位为"其他"时显示的名称 |

##### Serialization

```json
{
    "id": 1,
    "companyId": 2,
    "type": 1,
    "typeName": "",
    "name": "NakasuKasumi1",
    "status": 1
}
```

