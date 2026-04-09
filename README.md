# InPartyMod - 星引擎Mod开发框架

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![MelonLoader](https://img.shields.io/badge/MelonLoader-0.6+-orange.svg)](https://melonloader.com)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-blue.svg)]()

> 基于MelonLoader的星引擎（Astral Party）Mod开发框架，支持预加载资源替换，让Mod开发更简单。

## 快速开始

### 玩家使用

1. 安装 [MelonLoader](https://melonloader.com) 到星引擎游戏
2. 下载 [最新Release](https://github.com/AstralPartyModding/InPartyMod/releases)
3. 将ZIP解压到游戏根目录（与游戏.exe同级）
4. 启动游戏，Mod自动生效

### 开发者开发

```bash
# 1. Fork模板仓库
git clone https://github.com/AstralPartyModding/Template.git MyMod

# 2. 修改Mod属性（名称、版本、作者、资源路径）
# 3. 编译并测试
dotnet build

# 4. 运行打包脚本生成发布包
package.bat
```

详细开发指南：[查看文档](docs/DEVELOPER_PACKAGING_GUIDE.md)

## 项目结构

```
InPartyMod/
├── src/Core/                    # 核心库
│   ├── CoreMod.cs              # Mod基类（继承MelonMod）
│   ├── PreloadReplacementManager.cs  # 预替换管理器
│   ├── ResourceReplacer.cs     # 资源映射管理
│   ├── AssetBundlePatches.cs   # Harmony补丁（备用）
│   └── ModConfigBase.cs        # 配置基类
├── InPartyModManager/           # 游戏内Mod管理器（MelonLoader插件）
├── @ModManager/                # 桌面Mod管理器（WinForms）
├── mods/                       # 官方Mod示例
│   └── YuGiOhCardMod/          # 游戏王卡面Mod
└── docs/                       # 文档
    ├── DEVELOPER_PACKAGING_GUIDE.md
    ├── USER_GUIDE.md
    └── CATEGORICAL_RESOURCES.md
```

## 核心特性

- ✅ **预加载替换** - 游戏启动前自动替换资源文件，兼容Addressables系统
- ✅ **自动备份** - 首次替换时自动备份原始文件，退出时自动恢复
- ✅ **分类管理** - 支持按分类组织资源（如cards/events），可独立启用/禁用
- ✅ **多Mod共存** - 支持多个Mod同时加载，资源冲突自动处理
- ✅ **极简开发** - 继承CoreMod基类，只需配置几个属性即可

## 技术架构

### 预替换系统

不同于传统的运行时拦截（Harmony补丁），本框架采用**预加载替换**架构：

1. **启动时**：Mod在`OnInitializeMelon()`中执行预替换
2. **备份**：原始资源自动备份到`.backup/`目录，附带JSON元数据
3. **替换**：将Mod资源复制到游戏资源目录
4. **退出时**：`OnDeinitializeMelon()`自动恢复原始文件

这种架构的优势：
- 兼容Unity Addressables资源系统
- 无需复杂的运行时补丁
- 游戏更新后自动识别新资源

### 资源目录结构

```
ModResources/
└── {ModName}/
    ├── cards/              # 手牌卡图
    ├── events/             # 事件卡图
    └── config.json         # 分类配置
```

## 相关仓库

| 仓库 | 说明 |
|------|------|
| [InPartyMod](https://github.com/AstralPartyModding/InPartyMod) | 主项目（本仓库）- 包含Core库和示例Mod |
| [InPartyModManager](https://github.com/AstralPartyModding/InPartyModManager) | 游戏内Mod管理器（MelonLoader插件） |
| [@ModManager](https://github.com/AstralPartyModding/ModManager) | 桌面Mod管理器（WinForms应用） |
| [Template](https://github.com/AstralPartyModding/Template) | Mod开发模板 |

## 系统要求

- **操作系统**: Windows 10/11 (x64)
- **游戏**: 星引擎 Party (Astral Party)
- **依赖**: MelonLoader 0.6+

## 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件
