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

* stationIdHex : 查询的车站id列表
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

#### Route

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
