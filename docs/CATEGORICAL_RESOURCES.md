# 分类资源管理功能使用指南

## 概述

分类资源管理是InPartyMod Core库提供的高级功能，允许Mod开发者将资源按逻辑分类组织，每个分类可以独立启用/禁用。这为Mod用户提供了更细粒度的控制，也为Mod管理器提供了更好的配置管理能力。

## 核心概念

### 什么是分类资源？

分类资源是指将Mod资源按照功能或类型分组管理。例如，一个卡牌替换Mod可以有：
- `cards` 分类：用于替换手牌卡图
- `events` 分类：用于替换事件卡图
- `ui` 分类：用于替换UI元素

每个分类可以：
- 独立启用/禁用
- 拥有独立的资源映射配置
- 指定不同的目录路径

### 与传统方式的区别

| 特性 | 传统方式 | 分类资源方式 |
|------|---------|-------------|
| 资源组织 | 单一目录或固定目录列表 | 按逻辑分类组织 |
| 启用控制 | 整个Mod启用/禁用 | 可单独控制每个分类 |
| 配置灵活性 | 全局资源映射 | 分类级别资源映射 |
| 向后兼容 | - | 完全兼容传统方式 |

## 开发者指南

### 1. 启用分类资源管理

在Mod类中设置 `UseCategoricalResources` 属性为 `true`：

```csharp
public class MyMod : CoreMod
{
    protected override bool UseCategoricalResources => true;
    
    // ... 其他代码
}
```

### 2. 定义默认分类

重写 `InitializeDefaultCategories()` 方法定义Mod的分类结构：

```csharp
protected override void InitializeDefaultCategories()
{
    // 手牌卡图分类
    Config.SetCategory("cards", new ResourceCategory
    {
        Path = "resources/cards",           // 资源目录路径
        Description = "手牌卡图替换",       // 分类描述
        Enabled = true,                      // 默认启用状态
        ResourceMappings = new Dictionary<string, string>
        {
            // 分类特定的资源映射
            { "mod_card_01.bundle", "game_card_001.bundle" },
            { "mod_card_02.bundle", "game_card_002.bundle" },
        }
    });

    // 事件卡图分类
    Config.SetCategory("events", new ResourceCategory
    {
        Path = "resources/events",
        Description = "事件卡图替换",
        Enabled = true,
        ResourceMappings = new Dictionary<string, string>
        {
            { "mod_event_01.bundle", "game_event_001.bundle" },
        }
    });
}
```

### 3. ResourceCategory 类属性说明

| 属性 | 类型 | 说明 |
|------|------|------|
| `Enabled` | `bool` | 分类是否启用，默认为 `true` |
| `Path` | `string` | 资源目录路径，相对于Mod目录 |
| `Description` | `string` | 分类描述，用于显示和管理 |
| `ResourceMappings` | `Dictionary<string, string>` | 分类特定的资源映射表 |

### 4. 目录结构

使用分类资源管理的Mod建议采用以下目录结构：

```
MyMod/
├── MyMod.dll              # Mod程序集
├── config.json            # 配置文件（自动生成）
└── resources/             # 资源根目录
    ├── cards/             # cards分类资源
    │   ├── card_01.bundle
    │   └── card_02.bundle
    └── events/            # events分类资源
        ├── event_01.bundle
        └── event_02.bundle
```

### 5. 配置文件示例

Mod首次运行时会自动生成 `config.json`：

```json
{
  "enableDetailedLogging": false,
  "resourceMappings": {},
  "categories": {
    "cards": {
      "enabled": true,
      "path": "resources/cards",
      "description": "手牌卡图替换",
      "resourceMappings": {
        "mod_card_01.bundle": "game_card_001.bundle"
      }
    },
    "events": {
      "enabled": true,
      "path": "resources/events",
      "description": "事件卡图替换",
      "resourceMappings": {}
    }
  }
}
```

### 6. 打包脚本更新

更新 `package.bat` 确保分类目录被正确复制：

```batch
:: 创建分类目录
mkdir "Release\ModResources\%MOD_NAME%\resources\cards"
mkdir "Release\ModResources\%MOD_NAME%\resources\events"

:: 复制分类资源
xcopy "resources\cards\*" "Release\ModResources\%MOD_NAME%\resources\cards\" /s /e /i /y
xcopy "resources\events\*" "Release\ModResources\%MOD_NAME%\resources\events\" /s /e /i /y
```

## Mod管理器集成

### 获取分类配置

Mod管理器可以通过 `GetCategories()` 方法读取Mod的分类配置：

```csharp
// 获取Mod实例
var mod = GetModInstance();

// 获取所有分类配置
Dictionary<string, ResourceCategory> categories = mod.GetCategories();

// 遍历分类
foreach (var kvp in categories)
{
    string categoryName = kvp.Key;
    ResourceCategory category = kvp.Value;
    
    Console.WriteLine($"分类: {categoryName}");
    Console.WriteLine($"  启用: {category.Enabled}");
    Console.WriteLine($"  描述: {category.Description}");
    Console.WriteLine($"  路径: {category.Path}");
}
```

### 修改分类启用状态

Mod管理器可以通过 `SetCategoryEnabled()` 方法修改分类的启用状态：

```csharp
// 禁用cards分类
bool result = mod.SetCategoryEnabled("cards", false);

if (result)
{
    Console.WriteLine("cards分类已禁用");
    // 配置会自动保存到config.json
}
else
{
    Console.WriteLine("分类不存在");
}
```

### 动态添加分类

Mod管理器可以通过 `SetCategory()` 方法动态添加新分类：

```csharp
// 创建新分类
var newCategory = new ResourceCategory
{
    Path = "resources/new_category",
    Description = "新分类",
    Enabled = true
};

// 添加到Mod配置
mod.SetCategory("new_category", newCategory);
```

## 完整示例

参考 `mods/YuGiOhCardMod/` 目录下的完整实现：

- [`YuGiOhCardMod.cs`](../mods/YuGiOhCardMod/YuGiOhCardMod.cs) - Mod实现
- [`package.bat`](../mods/YuGiOhCardMod/package.bat) - 打包脚本

## 向后兼容性

分类资源管理功能完全向后兼容：

1. **不启用分类管理的Mod**：设置 `UseCategoricalResources = false`（默认值），Mod将使用传统的 `ResourceDirectories` 方式工作

2. **混合模式**：如果 `UseCategoricalResources` 为 `true` 但 `Config.Categories` 为空，Mod会自动将 `ResourceDirectories` 转换为默认分类

3. **配置文件兼容**：旧版配置文件没有 `categories` 字段时，Mod会自动创建默认分类配置

## 最佳实践

1. **分类命名**：使用简洁、有意义的分类名称，如 `cards`、`events`、`characters`、`ui`

2. **路径设置**：分类路径建议使用 `resources/分类名` 的结构，保持统一

3. **描述清晰**：分类描述应该清楚地说明该分类包含什么类型的资源

4. **默认启用**：新分类默认应该启用（`Enabled = true`），让用户自己选择禁用

5. **资源映射**：如果Mod文件名与游戏内资源名不一致，使用 `ResourceMappings` 进行映射

6. **文档说明**：在Mod的README中说明可用的分类及其功能

## 常见问题

### Q: 启用分类资源管理后，原有的ResourceDirectories还有用吗？

A: 有用。当 `Config.Categories` 为空时，Mod会自动将 `ResourceDirectories` 转换为默认分类。这保证了向后兼容性。

### Q: 分类的Enabled状态改变后需要重启游戏吗？

A: 不需要。调用 `SetCategoryEnabled()` 后，配置会自动保存。下次热重载（按F10/F11）或重新加载资源时，新的启用状态会生效。

### Q: 可以为同一个游戏资源设置多个分类的映射吗？

A: 可以，但不建议。如果多个分类都映射到同一个游戏资源，后加载的分类会覆盖先加载的。建议每个游戏资源只在一个分类中映射。

### Q: 分类路径可以是绝对路径吗？

A: 不建议。分类路径应该相对于Mod目录，以保证Mod的可移植性。

## API参考

### ModConfigBase 新增方法

| 方法 | 说明 |
|------|------|
| `GetCategory(string)` | 获取指定分类配置 |
| `SetCategoryEnabled(string, bool)` | 设置分类启用状态 |
| `IsCategoryEnabled(string)` | 检查分类是否启用 |
| `SetCategory(string, ResourceCategory)` | 添加/更新分类 |
| `RemoveCategory(string)` | 移除分类 |
| `GetEnabledCategoryNames()` | 获取所有启用的分类名称 |
| `GetAllCategoryNames()` | 获取所有分类名称 |

### CoreMod 新增方法

| 方法 | 说明 |
|------|------|
| `GetCategories()` | 获取所有分类配置（供Mod管理器使用） |
| `SetCategoryEnabled(string, bool)` | 设置分类启用状态并保存（供Mod管理器使用） |
| `SetCategory(string, ResourceCategory)` | 添加/更新分类并保存（供Mod管理器使用） |

### CoreMod 新增属性

| 属性 | 说明 |
|------|------|
| `UseCategoricalResources` | 是否启用分类资源管理 |

### CoreMod 新增可重写方法

| 方法 | 说明 |
|------|------|
| `InitializeDefaultCategories()` | 初始化默认分类配置 |
| `ScanCategoricalResources()` | 分类资源扫描（高级用法） |
| `ScanCategory(string, ResourceCategory, string)` | 扫描指定分类（高级用法） |
