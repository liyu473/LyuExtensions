

# LyuExtensions

[![NuGet](https://img.shields.io/nuget/v/LyuExtensions.svg)](https://www.nuget.org/packages/LyuExtensions/)
[![GitHub](https://img.shields.io/github/license/liyu473/LyuExtensions)](https://github.com/liyu473/LyuExtensions)

一个聚焦日常开发场景的 .NET 扩展库，提供 AOP 特性和扩展方法，助力快速构建业务代码。

## 目录

- [AOP 特性](#aop-特性)
  - [TryCatchAttribute - 自动异常处理](#trycatchattribute---自动异常处理)
  - [TimingAttribute - 方法耗时统计](#timingattribute---方法耗时统计)
  - [ServiceAttribute - 自动 DI 注册](#serviceattribute---自动-di-注册)
  - [Observable - 自动属性通知](#observable---自动属性通知)
  - [Inject - 自动注入](#inject---自动注入)
- [扩展方法](#扩展方法)

---

## AOP 特性

基于 Metalama 框架的 AOP 特性，通过简单的特性标注实现横切关注点。

### TryCatchAttribute - 自动异常处理

自动为方法添加 try-catch 包装，捕获异常、记录日志并返回默认值，无需手动编写异常处理代码。

#### 使用示例

**基础使用 - 自动捕获异常并记录日志：**

```csharp
[TryCatch]
public string? GetUserName(int userId)
{
    // 如果出现异常，会自动记录日志并返回 null
    return database.Query("SELECT name FROM users WHERE id = " + userId);
}

// 调用时不需要 try-catch
var name = GetUserName(123); // 异常时返回 null
```

**指定默认返回值：**

```csharp
[TryCatch(DefaultValue = "Unknown")]
public string GetUserName(int userId)
{
    // 如果出现异常，返回 "Unknown"
    return database.Query("SELECT name FROM users WHERE id = " + userId);
}

[TryCatch(DefaultValue = 0)]
public int CalculateTotal(List<int> numbers)
{
    // 如果出现异常，返回 0
    return numbers.Sum();
}

[TryCatch(DefaultValue = false)]
public bool ValidateData(string data)
{
    // 如果出现异常，返回 false
    return data.Length > 0 && data.Contains("valid");
}
```

---

### TimingAttribute - 方法耗时统计

自动统计方法执行耗时，支持自定义日志级别记录。

#### 使用示例

**基础使用 - 默认 Information 级别记录日志：**

```csharp
[Timing]
public async Task ProcessData()
{
    await Task.Delay(1000);
    // 业务逻辑
}

// 日志输出 (Information): 方法执行完成: YourNamespace.YourClass.ProcessData, 耗时: 1002ms
```

**自定义日志级别：**

```csharp
// 使用 Debug 级别记录
[Timing(LogLevelValue = 1)]
public void Calculate()
{
    // 复杂计算
}

// 使用 Warning 级别记录
[Timing(LogLevelValue = 3)]
public void ImportantOperation()
{
    // 重要操作
}

// 不记录日志 (None)
[Timing(LogLevelValue = 6)]
public void QuietOperation()
{
    // 不会记录任何日志
}
```

**异常处理：**

```csharp
[Timing]
public void RiskyOperation()
{
    throw new Exception("出错了");
}

// 即使抛出异常，也会记录耗时（使用 Error 级别）
// 日志输出: 方法执行异常: YourNamespace.YourClass.RiskyOperation, 耗时: 5ms
// 异常会被重新抛出
```

**异步方法支持：**

```csharp
[Timing]
public async Task<List<User>> GetUsersAsync()
{
    return await httpClient.GetFromJsonAsync<List<User>>("api/users");
}

// 日志输出: 方法执行完成: YourNamespace.YourClass.GetUsersAsync, 耗时: 234ms
```

#### 日志级别说明

| LogLevelValue | 日志级别 | 说明 |
|---------------|----------|------|
| 0 | Trace | 最详细的日志 |
| 1 | Debug | 调试信息 |
| 2 | Information | 常规信息（默认） |
| 3 | Warning | 警告信息 |
| 4 | Error | 错误信息 |
| 5 | Critical | 严重错误 |
| 6 | None | 不记录日志 |

#### 属性说明

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `LogLevelValue` | `int` | `2` | 日志记录级别，默认为 Information |

#### 注意事项

- 异常发生时会使用 Error 级别记录日志，并重新抛出异常
- 日志通过依赖注入的 `ILogger` 记录，确保已配置日志服务
- 设置 `LogLevelValue = 6` 可以完全禁用日志记录

---

### ServiceAttribute - 自动 DI 注册

通过特性标注自动注册服务到 DI 容器，告别繁琐的手动注册。

#### 特性列表

- `[Singleton]` - 注册为单例服务
- `[Scoped]` - 注册为作用域服务
- `[Transient]` - 注册为瞬态服务
- `[HostedService]` - 注册为后台服务

#### 使用示例

**1. 标记服务类：**

```csharp
// 注册为单例
[Singleton]
public class CacheService
{
    public void Set(string key, object value) { }
    public object Get(string key) { return null; }
}

// 注册为作用域服务
[Scoped]
public class OrderService
{
    public void CreateOrder() { }
}

// 注册为瞬态服务
[Transient]
public class EmailSender
{
    public void Send(string to, string subject) { }
}

// 注册为后台服务
[HostedService]
public class DataSyncService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 后台任务逻辑
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

**2. 注册接口实现：**

```csharp
public interface IUserService
{
    void CreateUser(string name);
}

[Singleton(ServiceType = typeof(IUserService))]
public class UserService : IUserService
{
    public void CreateUser(string name) { }
}

// 使用时注入接口
public class UserController
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
}
```

**3. 多实现场景 - 使用 ServiceKey：**

```csharp
public interface IPaymentProvider
{
    void Pay(decimal amount);
}

[Singleton(ServiceType = typeof(IPaymentProvider), ServiceKey = "Alipay")]
public class AlipayProvider : IPaymentProvider
{
    public void Pay(decimal amount) { }
}

[Singleton(ServiceType = typeof(IPaymentProvider), ServiceKey = "WeChat")]
public class WeChatPayProvider : IPaymentProvider
{
    public void Pay(decimal amount) { }
}

// 使用时通过 Key 注入
public class PaymentService
{
    private readonly IPaymentProvider _alipay;
    private readonly IPaymentProvider _wechat;
    
    public PaymentService(
        [FromKeyedServices("Alipay")] IPaymentProvider alipay,
        [FromKeyedServices("WeChat")] IPaymentProvider wechat)
    {
        _alipay = alipay;
        _wechat = wechat;
    }
}
```

**4. 在 Program.cs 中注册：**

```csharp
using LyuExtensions.Aspects;

var builder = WebApplication.CreateBuilder(args);

// 扫描并注册当前程序集中所有带特性的服务
builder.Services.RegisterServices();

// 或者扫描指定程序集
builder.Services.RegisterServices(typeof(UserService).Assembly);

// 或者扫描多个程序集
builder.Services.RegisterServices(
    typeof(UserService).Assembly,
    typeof(OrderService).Assembly
);

var app = builder.Build();
app.Run();
```

---

### Observable - 自动属性通知

基于 Metalama.Patterns.Observability 的自动属性变更通知

#### 特性

- 自动实现 `INotifyPropertyChanged` 接口
- 自动为所有属性生成 `PropertyChanged` 事件
- 支持依赖属性自动通知

---

### Inject - 自动注入

~~~C#
 [Inject]
 private readonly ILogger<MainViewModel> _logger;
~~~

自动将日志注入到当前实例，前提是双方都已注入

---

## 扩展方法

<details>
<summary>点击展开查看所有扩展方法</summary>

### 扩展方法列表

- [对象克隆扩展](#对象克隆扩展) - 深拷贝对象
- [Double 扩展](#double-扩展) - 数值格式化
- [枚举扩展](#枚举扩展) - 获取枚举描述
- [环境变量扩展](#环境变量扩展) - 简化路径获取
- [集合扩展](#集合扩展) - 批量操作集合
- [可枚举扩展](#可枚举扩展) - ForEach 遍历
- [JSON 扩展](#json-扩展) - JSON 序列化
- [对象扩展](#对象扩展) - 属性复制
- [字符串扩展](#字符串扩展) - 字符串判断

---

## 对象克隆扩展

提供两种深拷贝方式：基于 MemoryPack 的二进制高性能克隆和基于 JSON 的通用克隆。

实测：对于普通的类 ZClone 比JClone快了10倍以上

### 方法

#### ZClone（推荐）

使用 MemoryPack 二进制序列化实现极致性能的深拷贝。

**注意：** 需要在类上添加 `[MemoryPackable]` 特性并标注分部。

**示例：**
```csharp
using MemoryPack;

[MemoryPackable]
public partial class User
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var original = new User { Name = "张三", Age = 25 };
var cloned = original.ZClone();

cloned.Name = "李四";
Console.WriteLine(original.Name); // 输出: 张三
Console.WriteLine(cloned.Name);   // 输出: 李四
```

#### JClone

使用 JSON 序列化实现的深拷贝

**性能对比：**

首次调用性能差异：

![首次克隆性能对比](images/clone-performance-first.png)

后续调用性能：

![后续克隆性能对比](images/clone-performance-after.png)

---

## Double 扩展

提供 double 类型的四舍五入和百分比格式化功能。

---

## 枚举扩展

获取枚举值的 `[Description]` 特性描述文本。

### 方法

#### GetEnumDescription

获取枚举值的描述文本，如果没有描述则返回枚举名称。

**示例：**
```csharp
public enum OrderStatus
{
    [Description("待支付")]
    Pending,
    
    [Description("已支付")]
    Paid,
    
    [Description("已发货")]
    Shipped,
    
    Completed  // 没有 Description
}

var status = OrderStatus.Pending;
Console.WriteLine(status.GetEnumDescription()); // 输出: 待支付

var status2 = OrderStatus.Completed;
Console.WriteLine(status2.GetEnumDescription()); // 输出: Completed
```

---

## 环境变量扩展

简化 `Environment.SpecialFolder` 的路径获取。

### 方法

#### GetFolderPath

获取系统特殊文件夹路径。

**示例：**
```csharp
using static System.Environment;

// 获取用户桌面路径
string desktop = SpecialFolder.Desktop.GetFolderPath();
Console.WriteLine(desktop); // 例如: C:\Users\Username\Desktop

// 获取应用数据路径
string appData = SpecialFolder.ApplicationData.GetFolderPath();
Console.WriteLine(appData); // 例如: C:\Users\Username\AppData\Roaming

// 使用选项获取路径
string myDocs = SpecialFolder.MyDocuments.GetFolderPath(
    SpecialFolderOption.DoNotVerify
);
```

---

## 集合扩展

扩展 `ICollection<T>` 接口，提供批量添加AddRange功能。

---

## 可枚举扩展

为 `IEnumerable<T>` 提供 ForEach 遍历方法，支持同步和异步操作。

### 方法

#### ForEach

对每个元素执行指定操作。

**示例：**
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };

// 打印每个元素
numbers.ForEach(n => Console.WriteLine(n));

// 链式调用
var result = numbers
    .Where(n => n > 2)
    .ForEach(n => Console.WriteLine($"大于2的数: {n}"));
```

#### ForEachAsync

对每个元素执行异步操作，顺序等待执行。

**签名：**
```csharp
Task<IEnumerable<T>> ForEachAsync<T>(
    this IEnumerable<T> values,
    Func<T, Task> func,
    CancellationToken cancellationToken = default
)
```

**示例：**
```csharp
var urls = new[] {
    "https://api.example.com/user/1",
    "https://api.example.com/user/2",
    "https://api.example.com/user/3"
};

using var client = new HttpClient();

// 顺序处理每个 URL
await urls.ForEachAsync(async url =>
{
    var response = await client.GetStringAsync(url);
    Console.WriteLine($"获取: {url}");
});

// 支持取消令牌
var cts = new CancellationTokenSource();
await urls.ForEachAsync(
    async url => await ProcessAsync(url),
    cts.Token
);
```

---

## JSON 扩展

提供简单的 JSON 序列化扩展方法，基于 `System.Text.Json`。

### 默认配置

- **命名策略**：camelCase
- **格式化输出**：启用缩进
- **编码器**：支持完整 Unicode（包括中文）
- **忽略条件**：不忽略任何属性

### 方法概览

- **序列化**：`ToJson`
- **反序列化**：`FromJson`、`TryFromJson`
- **JSON 片段读取**：`GetJsonFragment`、`GetJsonValue`、`HasJsonPath`

### 序列化

#### ToJson - 标准序列化

将对象序列化为格式化的 JSON 字符串（带缩进）。

```csharp
public record Person(string Name, int Age, string City);

var person = new Person("张三", 25, "北京");
string json = person.ToJson();
Console.WriteLine(json);
/* 输出:
{
  "name": "张三",
  "age": 25,
  "city": "北京"
}
*/
```

### 反序列化

#### FromJson - 标准反序列化

将 JSON 字符串反序列化为对象。

```csharp
string json = """{"name":"张三","age":25,"city":"北京"}""";
var person = json.FromJson<Person>();
Console.WriteLine(person.Name); // 输出: 张三
```

#### TryFromJson - 安全反序列化

尝试反序列化，失败时不抛出异常。

```csharp
string json = """{"name":"张三","age":25}""";

if (json.TryFromJson<Person>(out var person))
{
    Console.WriteLine($"成功: {person.Name}");
}
else
{
    Console.WriteLine("反序列化失败");
}

// 无效的 JSON
string invalidJson = "{invalid json}";
if (invalidJson.TryFromJson<Person>(out var result))
{
    // 不会执行
}
else
{
    Console.WriteLine("JSON 无效"); // 输出: JSON 无效
}
```

### JSON 片段读取

#### GetJsonFragment - 提取JSON片段

从 JSON 字符串中提取指定路径的片段。

**路径语法：**
- 使用点号分隔属性：`"user.name"`
- 使用中括号访问数组：`"items[0]"`
- 组合使用：`"user.address.city"` 或 `"orders[0].total"`

```csharp
string json = """
{
  "user": {
    "name": "张三",
    "age": 25,
    "address": {
      "city": "北京",
      "street": "长安街"
    }
  },
  "orders": [
    {"id": 1, "total": 299.9},
    {"id": 2, "total": 499.5}
  ]
}
""";

// 提取嵌套属性
var city = json.GetJsonFragment("user.address.city");
Console.WriteLine(city); // 输出: "北京"

// 提取数组元素
var firstOrder = json.GetJsonFragment("orders[0]");
Console.WriteLine(firstOrder); // 输出: {"id": 1, "total": 299.9}

// 提取数组元素的属性
var total = json.GetJsonFragment("orders[1].total");
Console.WriteLine(total); // 输出: 499.5
```

#### GetJsonValue - 提取并反序列化

提取 JSON 片段并直接反序列化为指定类型。

```csharp
public record Address(string City, string Street);
public record Order(int Id, double Total);

// 提取并反序列化对象
var address = json.GetJsonValue<Address>("user.address");
Console.WriteLine(address.City); // 输出: 北京

// 提取并反序列化数组元素
var order = json.GetJsonValue<Order>("orders[0]");
Console.WriteLine(order.Total); // 输出: 299.9

// 提取基本类型
var age = json.GetJsonValue<int>("user.age");
Console.WriteLine(age); // 输出: 25
```

#### HasJsonPath - 检查路径是否存在

验证 JSON 中是否存在指定路径。

```csharp
if (json.HasJsonPath("user.address.city"))
{
    Console.WriteLine("城市信息存在");
}

if (!json.HasJsonPath("user.phone"))
{
    Console.WriteLine("电话信息不存在");
}

// 检查数组索引
if (json.HasJsonPath("orders[0]"))
{
    Console.WriteLine("第一个订单存在");
}

if (!json.HasJsonPath("orders[10]"))
{
    Console.WriteLine("第11个订单不存在");
}
```

### 实际应用示例

```csharp
// API 响应处理
string apiResponse = """
{
  "code": 200,
  "message": "success",
  "data": {
    "users": [
      {"id": 1, "name": "张三", "email": "zhang@example.com"},
      {"id": 2, "name": "李四", "email": "li@example.com"}
    ],
    "total": 2
  }
}
""";

// 检查响应是否成功
var code = apiResponse.GetJsonValue<int>("code");
if (code == 200)
{
    // 提取用户列表
    var users = apiResponse.GetJsonValue<List<User>>("data.users");
    
    // 或者只提取第一个用户的邮箱
    var firstEmail = apiResponse.GetJsonValue<string>("data.users[0].email");
    Console.WriteLine(firstEmail); // 输出: zhang@example.com
    
    // 提取总数
    var total = apiResponse.GetJsonValue<int>("data.total");
    Console.WriteLine($"共 {total} 个用户");
}

// 配置文件读取
string configJson = """
{
  "database": {
    "host": "localhost",
    "port": 5432,
    "credentials": {
      "username": "admin",
      "password": "secret"
    }
  }
}
""";

// 安全地提取配置值
if (configJson.HasJsonPath("database.credentials.username"))
{
    var username = configJson.GetJsonValue<string>("database.credentials.username");
    var port = configJson.GetJsonValue<int>("database.port");
    Console.WriteLine($"连接到 {username}@localhost:{port}");
}
```

---

## 字符串扩展

提供字符串判空扩展方法。

### 方法

#### IsNullOrWhiteSpace

判断字符串是否为 null 或空白。

**示例：**
```csharp
string? str1 = null;
string str2 = "";
string str3 = "   ";
string str4 = "hello";

Console.WriteLine(str1.IsNullOrWhiteSpace()); // true
Console.WriteLine(str2.IsNullOrWhiteSpace()); // true
Console.WriteLine(str3.IsNullOrWhiteSpace()); // true
Console.WriteLine(str4.IsNullOrWhiteSpace()); // false
```

#### IsNullOrEmpty

判断字符串是否为 null 或空字符串。

**示例：**
```csharp
string? str1 = null;
string str2 = "";
string str3 = "   ";
string str4 = "hello";

Console.WriteLine(str1.IsNullOrEmpty()); // true
Console.WriteLine(str2.IsNullOrEmpty()); // true
Console.WriteLine(str3.IsNullOrEmpty()); // false (包含空格)
Console.WriteLine(str4.IsNullOrEmpty()); // false
```

</details>

---

