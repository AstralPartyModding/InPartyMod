# AstralPartyMod.Core API 参考手册

## 命名空间

- `AstralPartyMod.Core` - 核心类和Mod基类
- `AstralPartyMod.Core.Assets` - 资源管理
- `AstralPartyMod.Core.Configuration` - 配置管理
- `AstralPartyMod.Core.Events` - 事件系统
- `AstralPartyMod.Core.Harmony` - Harmony补丁管理
- `AstralPartyMod.Core.Modules` - 模块系统

---

## CoreMod 抽象类

所有Mod必须继承此类。

### 必需属性

```csharp
// Mod名称
protected abstract string ModName { get; }

// Mod版本
protected abstract string ModVersion { get; }

// Mod作者
protected abstract string ModAuthor { get; }

// 资源目录名称数组
protected abstract string[] ResourceDirectories { get; }
```

### 可选属性

```csharp
// 重新加载资源的快捷键（默认: F10）
protected virtual KeyCode ReloadKey => KeyCode.F10;

// 是否启用统计（默认: false）
protected virtual bool EnableStatistics => false;

// 是否启用详细日志（默认: false）
protected virtual bool EnableDetailedLogging => false;

// 是否使用分类资源（默认: false）
protected virtual bool UseCategoricalResources => false;
```

### 公共属性

```csharp
// 资源替换器
public ResourceReplacer ResourceReplacer { get; }

// Mod配置
public ModConfigBase Config { get; }

// 已替换的资源数量
public int ReplacedCount { get; }

// 总资源数量
public int TotalResources { get; }
```

### 生命周期方法

```csharp
// Mod初始化时调用
public override void OnInitializeMelon()

// 每帧调用
public override void OnUpdate()

// Mod卸载时调用
public override void OnDeinitializeMelon()
```

---

## EventBus 事件总线

提供Mod间通信和事件广播。

### 订阅事件

```csharp
EventBus.Subscribe<MyEvent>(OnMyEventHandler);

private void OnMyEventHandler(MyEvent evt)
{
    // 处理事件
}
```

### 发布事件

```csharp
EventBus.Publish(new MyEvent());
```

### 取消订阅

```csharp
EventBus.Unsubscribe<MyEvent>(OnMyEventHandler);
```

### 辅助方法

```csharp
// 获取订阅者数量
int count = EventBus.GetSubscriberCount<MyEvent>();

// 清除所有订阅
EventBus.ClearAll();

// 清除特定事件订阅
EventBus.Clear<MyEvent>();
```

---

## 预定义事件

### GameStartEvent

游戏启动时发布。

```csharp
GameStartEvent.Publish();
```

### GameExitEvent

游戏退出时发布。

```csharp
GameExitEvent.Publish();
```

### RoundStartEvent

回合开始时发布。

```csharp
RoundStartEvent.Publish(int roundNumber);
```

### RoundEndEvent

回合结束时发布。

```csharp
RoundEndEvent.Publish(int roundNumber);
```

### CardUsedEvent

卡牌使用时发布。

```csharp
CardUsedEvent.Publish(string cardId, int playerIndex);
```

### SceneLoadedEvent

场景加载完成时发布。

```csharp
SceneLoadedEvent.Publish(string sceneName);
```

---

## ModuleRegistry 模块注册中心

管理所有功能模块的注册和生命周期。

### 注册模块

```csharp
ModuleRegistry.RegisterModule(myModule);
```

### 注销模块

```csharp
ModuleRegistry.UnregisterModule("ModuleName");
```

### 获取模块

```csharp
var module = ModuleRegistry.GetModule("ModuleName");
```

### 检查模块

```csharp
bool exists = ModuleRegistry.IsRegistered("ModuleName");
```

### 获取模块数量

```csharp
int count = ModuleRegistry.ModuleCount;
```

### 获取所有模块

```csharp
IReadOnlyDictionary<string, IModModule> modules = ModuleRegistry.Modules;
```

### 初始化所有模块

```csharp
ModuleRegistry.InitializeAll();
```

### 关闭所有模块

```csharp
ModuleRegistry.ShutdownAll();
```

---

## IModModule 接口

所有模块必须实现此接口。

### 属性

```csharp
string Name { get; }
string Description { get; }
string Version { get; }
IReadOnlyList<string> Dependencies { get; }
bool IsEnabled { get; }
```

### 方法

```csharp
bool Initialize();
bool Enable();
bool Disable();
void Shutdown();
```

---

## HarmonyPatcher Harmony补丁管理

统一管理Harmony补丁。

### 初始化

```csharp
HarmonyPatcher.Initialize("MyMod");
```

### 应用所有补丁

```csharp
HarmonyPatcher.PatchAll();
```

### 应用单个补丁

```csharp
HarmonyPatcher.Patch(typeof(MyPatchClass));
```

### 卸载所有补丁

```csharp
HarmonyPatcher.UnpatchAll();
```

### 属性

```csharp
HarmonyLib.Harmony? HarmonyInstance { get; }
bool IsPatched { get; }
```

---

## ConfigManager 配置管理器

JSON配置文件读写。

### 初始化

```csharp
ConfigManager.Initialize("Config");
```

### 加载配置

```csharp
var config = ConfigManager.LoadConfig<MyConfig>("config_name", defaultConfig);
```

### 保存配置

```csharp
bool success = ConfigManager.SaveConfig("config_name", config);
```

### 重新加载配置

```csharp
var config = ConfigManager.ReloadConfig<MyConfig>("config_name", defaultConfig);
```

### 检查配置是否存在

```csharp
bool exists = ConfigManager.ConfigExists("config_name");
```

### 删除配置

```csharp
bool success = ConfigManager.DeleteConfig("config_name");
```

---

## ModConfigBase 配置基类

### 添加配置项

```csharp
Config.SetValue("key", "value");
```

### 获取配置项

```csharp
string value = Config.GetValue("key", "default");
```

### 分类管理

```csharp
// 设置分类
Config.SetCategory("category_name", path, description, enabled);

// 获取分类
var category = Config.GetCategory("category_name");

// 设置分类启用状态
Config.SetCategoryEnabled("category_name", true);

// 检查分类是否启用
bool enabled = Config.IsCategoryEnabled("category_name");
```

---

## UnityExplorerInit UnityExplorer集成

调试工具初始化（条件编译）。

```csharp
#if UNITYEXPLORER_AVAILABLE
UnityExplorerInit.Initialize(autoStart: false);
UnityExplorerInit.Show();
UnityExplorerInit.Hide();
UnityExplorerInit.Toggle();
#endif
```
