# LyuExtensions

一个聚焦日常开发场景的 .NET 扩展方法集，助力快速构建业务代码。

## 扩展方法列表

- [HttpClient 扩展](#httpclient-扩展) - 简化 HTTP 请求
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

## HttpClient 扩展

简化 HttpClient 的 POST 请求操作，自动处理序列化和错误回调。

---

## 对象克隆扩展

提供两种深拷贝方式：基于 MemoryPack 的二进制高性能克隆和基于 JSON 的通用克隆。

实测：对于普通的类 ZClone 比JClone快了10倍以上

### 命名空间

```csharp
using Extensions;
```

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

### 命名空间

```csharp
using Extensions;
using System.ComponentModel;
```

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

### 命名空间

```csharp
using Extensions;
```

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

### 命名空间

```csharp
using Extensions;
```

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

## 对象扩展

提供对象属性复制功能，特别适用于 WPF MVVM 绑定场景。

### 方法

#### UpdatePropertiesFrom

基础属性复制，将源对象的可读写属性复制到目标对象。

#### UpdatePropertiesHighQualityFrom

高性能属性复制，使用表达式树缓存提升性能。

**示例：**
```csharp
// 适用于频繁调用的场景
for (int i = 0; i < 10000; i++)
{
    target.UpdatePropertiesHighQualityFrom(source);
}
```

#### UpdatePropertiesHighQualityExcludeGenericTypeFrom

高性能属性复制，特殊处理 `ObservableCollection<T>` 和 `BindingList<T>`。

**特性：**

- 对于集合类型，同步元素而非替换整个集合
- 保持 WPF/MVVM 的数据绑定关系

**示例：**
```csharp
using System.Collections.ObjectModel;

public class ViewModel
{
    public string Title { get; set; }
    public ObservableCollection<string> Items { get; set; }
}

var source = new ViewModel
{
    Title = "新标题",
    Items = new ObservableCollection<string> { "项目1", "项目2" }
};

var target = new ViewModel
{
    Title = "旧标题",
    Items = new ObservableCollection<string> { "旧项目" }
};

// 绑定到 UI
// DataContext = target;

// 更新属性（保持 Items 集合实例不变）
target.UpdatePropertiesHighQualityExcludeGenericTypeFrom(source);

Console.WriteLine(target.Title); // 输出: 新标题
// target.Items 集合实例未改变，但内容更新为 ["项目1", "项目2"]
// UI 绑定仍然有效
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

