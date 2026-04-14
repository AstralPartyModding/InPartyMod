# 变更日志

## v0.1.0 (2026-04-14) - 第一阶段基础设施完成

### 新增功能

- **Harmony补丁管理**: 创建了 `HarmonyPatcher.cs` - 统一管理所有Harmony补丁，支持补丁加载/卸载
- **配置管理系统**: 
  - 创建了 `ConfigManager.cs` - JSON配置文件读写支持
  - 保留已有 `ModConfigBase.cs` - 配置基类
  - 支持自动创建默认配置和热重载
- **全局事件总线**:
  - 创建了 `EventBus.cs` - 泛型事件总线，支持订阅/取消订阅/广播
  - 创建了 `GameLifecycleEvents.cs` - 定义游戏生命周期事件
    - GameStartEvent
    - GameExitEvent  
    - RoundStartEvent
    - RoundEndEvent
    - CardUsedEvent
    - SceneLoadedEvent
- **模块注册中心**:
  - 创建了 `IModModule.cs` - 模块基础接口
  - 创建了 `ModuleRegistry.cs` - 模块注册中心，支持依赖解析和拓扑排序
- **调试工具集成**:
  - 创建了 `UnityExplorerInit.cs` - UnityExplorer调试工具集成
  - 使用条件编译，在没有UnityExplorer环境下也能正常编译
- **目录结构重构**:
  - 按功能模块重新组织代码结构，更清晰易扩展

### 代码结构

```
src/Core/
├── AstralPartyMod.Core.csproj    # 项目文件
├── CoreMod.cs                    # MelonLoader入口抽象基类
├── Harmony/
│   └── HarmonyPatcher.cs         # Harmony补丁管理器
├── Configuration/
│   ├── ModConfigBase.cs          # 配置基类
│   └── ConfigManager.cs          # 配置管理器
├── Events/
│   ├── EventBus.cs               # 全局事件总线
│   └── GameLifecycleEvents.cs    # 游戏生命周期事件
├── Modules/
│   ├── IModModule.cs             # 模块接口
│   └── ModuleRegistry.cs         # 模块注册中心
├── Debugging/
│   └── UnityExplorerInit.cs      # UnityExplorer初始化
└── Assets/
    ├── AssetBundlePatches.cs
    ├── PreloadReplacementManager.cs
    └── ResourceReplacer.cs
```

### 项目配置

- 目标框架: .NET 6.0
- 平台: x64
- 依赖:
  - HarmonyX 2.12.0 (NuGet)
  - MelonLoader (从游戏目录引用)
  - UniverseLib (从游戏目录引用，可选)
  - UnityExplorer (从游戏目录引用，可选)

### 备注

- 编译成功，只有Mono.Cecil版本冲突警告（不影响使用）
- 准备发布v0.1.0开发预览版
