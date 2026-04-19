# Mod开发模板

基于 AstralPartyMod.Core 框架的Mod开发模板。

## 使用方法

1. 复制此目录到 `mods/YourModName/`
2. 修改 `TemplateMod.csproj` 中的项目名称
3. 修改 `TemplateMod.cs` 中的Mod信息
4. 编译后放入游戏 `Mods` 目录

## 核心代码示例

```csharp
using MelonLoader;
using AstralPartyMod.Core;

namespace YourMod
{
    public class YourMod : CoreMod
    {
        protected override string ModName => "YourMod";
        protected override string ModVersion => "1.0.0";
        protected override string ModAuthor => "YourName";
        protected override string[] ResourceDirectories => new[] { "cards", "events" };

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("Mod已加载！");
        }
    }
}
```

## 继承 CoreMod 获得的功能

- 自动资源管理和替换
- 配置文件读写
- 事件系统集成
- Harmony补丁支持
- 模块注册系统

## 更多信息

- [API参考手册](../../docs/API_Reference.md)
- [入门教程](../../docs/Getting_Started.md)
