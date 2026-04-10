# InPartyMod 使用指南

## 一、玩家使用（安装Mod）

### 步骤1：安装MelonLoader

1. 下载 [MelonLoader](https://melonloader.com)
2. 运行安装程序，选择星引擎游戏目录
3. 等待安装完成

### 步骤2：安装Mod

1. 从 [InPartyMod Releases](https://github.com/AstralPartyModding/InPartyMod/releases) 下载Mod DLL文件
2. 将DLL文件放入游戏目录的 `Mods` 文件夹
3. 将Mod资源文件（图片/音频等）放入对应Mod的资源目录

### 步骤3：运行游戏

- 启动游戏，Mod会自动加载
- 按 **F10** 键可热重载资源（无需重启游戏）

---

## 二、开发者开发（创建新Mod）

### 方法1：使用模板（推荐）

```bash
# 1. Fork InPartyMod仓库
git clone https://github.com/AstralPartyModding/InPartyMod.git

# 2. 复制模板创建新Mod
cp -r template mods/MyMod
cd mods/MyMod

# 3. 修改3个属性（TemplateMod.cs）
#    - ModName: Mod名称
#    - ModVersion: 版本号
#    - ModAuthor: 作者名

# 4. 编译
dotnet build

# 5. 打包 - 运行package.bat生成发布包
```

### 方法2：手动创建

```csharp
using MelonLoader;
using AstralPartyMod.Core;

[assembly: MelonInfo(typeof(MyMod), "我的Mod", "1.0.0", "你的名字")]

public class MyMod : CoreMod
{
    protected override string ModName => "我的Mod";
    protected override string ModVersion => "1.0.0";
    protected override string ModAuthor => "你的名字";
    protected override string[] ResourceDirectories => new[] { "Resources" };
}
```

---

## 三、Mod管理器（游戏内管理）

`@ModManager` 子模块提供游戏内Mod管理功能：

- **F1** 键打开管理器界面
- 查看已加载的Mod列表
- 启用/禁用Mod
- 实时查看资源替换统计

---

## 四、相关链接

| 资源 | 链接 |
|------|------|
| 主项目 | https://github.com/AstralPartyModding/InPartyMod |
| 开发模板 | [template/](../template/) 目录（已集成在主项目） |
| 文档/Mod索引 | https://github.com/AstralPartyModding/InPartyDocs |
| Mod管理器 | https://github.com/AstralPartyModding/InPartyModManager |
