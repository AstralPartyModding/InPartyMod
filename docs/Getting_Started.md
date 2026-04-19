# 入门教程 - 5分钟创建你的第一个Mod

本教程将带你创建一个简单的Mod，实现修改游戏动画速度的功能。

## 前置要求

- 安装了 MelonLoader 的星引擎游戏
- .NET 6.0 SDK
- 代码编辑器（Visual Studio / VS Code）

## 第一步：创建Mod项目

1. 克隆框架仓库：
```batch
git clone https://github.com/AstralPartyModding/astparty.git
cd astparty
```

2. 复制模板创建新Mod：
```batch
xcopy mods\_Template mods\SpeedMod /E /I
cd mods\SpeedMod
```

3. 修改 `TemplateMod.csproj` 中的项目名称：
```xml
<AssemblyName>SpeedMod</AssemblyName>
```

## 第二步：编写Mod代码

打开 `TemplateMod.cs`，修改为以下内容：

```csharp
using MelonLoader;
using UnityEngine;
using AstralPartyMod.Core;

namespace SpeedMod
{
    // 继承CoreMod基类
    public class SpeedMod : CoreMod
    {
        // Mod基本信息
        protected override string ModName => "SpeedMod";
        protected override string ModVersion => "1.0.0";
        protected override string ModAuthor => "YourName";
        protected override string[] ResourceDirectories => new[] { "cards", "events" };

        // 动画速度倍率
        private float _speedMultiplier = 2.0f;

        // 快捷键设置
        protected override KeyCode ReloadKey => KeyCode.F10;

        // 初始化
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("SpeedMod 已加载！按 +/- 调整速度");
        }

        // 每帧更新
        public override void OnUpdate()
        {
            base.OnUpdate();

            // 按 + 增加速度
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                _speedMultiplier = Mathf.Min(_speedMultiplier + 0.5f, 5.0f);
                MelonLogger.Msg($"速度: {_speedMultiplier}x");
            }

            // 按 - 减少速度
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                _speedMultiplier = Mathf.Max(_speedMultiplier - 0.5f, 0.5f);
                MelonLogger.Msg($"速度: {_speedMultiplier}x");
            }
        }
    }
}
```

## 第三步：编译

返回项目根目录：
```batch
cd ..\..
build.bat
```

编译成功后会生成 `out/SpeedMod.dll`。

## 第四步：安装测试

1. 复制 `out/SpeedMod.dll` 到游戏的 `Mods` 目录
2. 启动游戏
3. 查看MelonLoader日志，确认 "SpeedMod 已加载！"
4. 按 +/- 调整速度

## 进阶：使用事件系统

如果另一个Mod需要知道速度变化，可以发布事件：

```csharp
// 在SpeedMod中发布事件
public class SpeedChangedEvent
{
    public float NewSpeed { get; }
    public SpeedChangedEvent(float newSpeed) => NewSpeed = newSpeed;
}

// 发布
EventBus.Publish(new SpeedChangedEvent(_speedMultiplier));
```

其他Mod可以订阅：
```csharp
EventBus.Subscribe<SpeedChangedEvent>(OnSpeedChanged);

private void OnSpeedChanged(SpeedChangedEvent evt)
{
    MelonLogger.Msg($"速度被改为 {evt.NewSpeed}x！");
}
```

## 进阶：创建功能模块

创建可复用的模块：

```csharp
public class SpeedModule : IModModule
{
    public string Name => "SpeedControl";
    public string Description => "速度控制模块";
    public string Version => "1.0.0";
    public IReadOnlyList<string> Dependencies => Array.Empty<string>();
    public bool IsEnabled { get; private set; }

    private float _speed = 1.0f;

    public bool Initialize() => true;
    public bool Enable() { IsEnabled = true; return true; }
    public bool Disable() { IsEnabled = false; return true; }
    public void Shutdown() { }
}
```

注册模块：
```csharp
ModuleRegistry.RegisterModule(new SpeedModule());
```

## 常见问题

### 编译错误：找不到MelonLoader.dll
设置环境变量 `GAME_DIR` 指向游戏目录：
```batch
set GAME_DIR=F:\steamapps\common\Astral Party\8vJXn6CN
```

### 编译警告：Mono.Cecil版本冲突
这是正常的，不影响运行。

### 游戏启动失败
确认游戏版本兼容 MelonLoader 0.6+ 和 .NET 6.0。

## 下一步

- 查看 [API 参考手册](API_Reference.md) 了解更多功能
- 查看 [示例Mod](../mods/_Template) 了解完整项目结构
- 参与社区讨论，提出问题和建议
