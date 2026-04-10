# 星引擎Mod模板

这是基于 AstralPartyMod.Core 框架的Mod开发模板。

## 快速开始

### 1. 复制模板

**在InPartyMod仓库内开发（推荐）：**
```bash
# 1. Fork本仓库
git clone https://github.com/AstralPartyModding/InPartyMod.git

# 2. 复制模板到mods目录
cp -r template mods/MyMod
cd mods/MyMod
```

### 2. 修改配置

编辑以下文件中的占位符：

**TemplateMod.cs** - 修改3个属性：
```csharp
protected override string ModName => "你的Mod名称";
protected override string ModVersion => "1.0.0";
protected override string ModAuthor => "你的名字";
```

**TemplateMod.cs** - 修改程序集信息：
```csharp
[assembly: MelonInfo(typeof(TemplateMod.TemplateMod), "你的Mod名称", "1.0.0", "你的名字")]
```

**package.bat** - 修改Mod信息：
```batch
set MOD_NAME=你的Mod名称
set MOD_VERSION=1.0.0
```

### 3. 添加资源

将资源文件放入对应目录：
- `resources/cards/` - 手牌卡图资源
- `resources/events/` - 事件卡图资源

### 4. 编译打包

```bash
dotnet build
package.bat
```

## 目录结构

```
MyMod/
├── TemplateMod.cs          # Mod主类
├── TemplateMod.csproj      # 项目文件
├── package.bat             # 打包脚本
├── README.md               # 本文件
└── resources/              # 资源目录
    ├── cards/              # 手牌卡图
    └── events/             # 事件卡图
```

## 核心概念

### 预替换系统

本框架采用预加载替换架构：
1. **启动时**：Mod自动扫描资源并执行预替换
2. **备份**：原始资源自动备份到 `.backup/` 目录
3. **替换**：Mod资源复制到游戏资源目录
4. **退出时**：自动恢复原始文件

### 分类管理

通过 `UseCategoricalResources` 启用分类管理：
- 每个分类可以独立启用/禁用
- 支持通过Mod管理器动态控制
- 分类配置保存在 `config.json`

## 开发指南

### 添加新的资源分类

在 `InitializeDefaultCategories()` 中添加：

```csharp
Config.SetCategory("新分类", new ResourceCategory
{
    Path = "新分类路径",
    Description = "分类描述",
    Enabled = true,
    ResourceMappings = new Dictionary<string, string>
    {
        { "mod文件名.bundle", "游戏资源名.bundle" }
    }
});
```

### 资源映射

资源映射定义了Mod文件与游戏资源的对应关系：
- **键**：Mod资源文件名（相对于分类路径）
- **值**：游戏内资源完整文件名

## 依赖

- [MelonLoader](https://melonloader.com) 0.6+
- [AstralPartyMod.Core](../../src/Core/)

## 参考

- [开发文档](../../docs/DEVELOPER_PACKAGING_GUIDE.md)
- [分类资源指南](../../docs/CATEGORICAL_RESOURCES.md)
