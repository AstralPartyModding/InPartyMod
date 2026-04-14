# AstralParty Mod - 星引擎Mod开发框架

**版本: v0.1.0**

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![MelonLoader](https://img.shields.io/badge/MelonLoader-0.6+-orange.svg)](https://melonloader.com)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-blue.svg)]()
[![Version](https://img.shields.io/badge/Version-0.1.0-green.svg)]()

> 基于MelonLoader的星引擎（Astral Party）Mod开发框架，支持预加载资源替换，让Mod开发更简单。

## 快速开始

### 玩家使用

1. 安装 [MelonLoader](https://melonloader.com) 到星引擎游戏
2. 下载 [最新Release](https://github.com/AstralPartyModding/astparty/releases)
3. 将DLL文件放入游戏 `Mods` 目录
4. 启动游戏，Mod自动生效

### 开发者开发

```batch
:: 1. Clone本仓库
git clone https://github.com/AstralPartyModding/astparty.git
cd astparty

:: 2. 使用模板创建新Mod
xcopy template mods\MyMod /E /I
cd mods\MyMod

:: 3. 修改Mod属性（名称、版本、作者）
:: 4. 回到根目录编译
cd ..\..
build.bat

:: 编译完成后所有输出在 out 目录
```

## 项目结构

```
astparty/
├── Directory.Build.props     # 全局构建配置（含绑定重定向、MelonLoader路径）
├── build.bat                 # 一键构建脚本（自动扫描mods目录）
├── src/Core/                 # AstralPartyMod.Core 核心框架
├── template/                 # Mod开发模板
├── mods/                     # 本地Mod开发（.gitkeep保留目录，不包含实际Mod，不上传GitHub）
├── @ModManager/              # Mod管理器（可选，Git子模块，不上传GitHub）
└── out/                      # 构建输出目录（编译后的dll在这里）
```

## 核心特性

- ✅ **预加载替换** - 游戏启动前自动替换资源文件，兼容Addressables系统
- ✅ **自动备份** - 首次替换时自动备份原始文件，退出时自动恢复
- ✅ **分类管理** - 支持按分类组织资源（如cards/events），可独立启用/禁用
- ✅ **多Mod共存** - 支持多个Mod同时加载，资源冲突自动处理
- ✅ **极简开发** - 继承CoreMod基类，只需配置几个属性即可

## 预替换架构

不同于传统的运行时拦截（Harmony补丁），本框架采用**预加载替换**架构：

1. **启动时**：Mod在初始化中执行预替换
2. **备份**：原始资源自动备份到 `.backup/` 目录
3. **替换**：将Mod资源复制到游戏资源目录
4. **退出时**：自动恢复原始文件

优势：
- 兼容Unity Addressables资源系统
- 无需复杂的运行时补丁
- 游戏更新后自动识别新资源

## 系统要求

- **操作系统**: Windows 10/11 (x64)
- **游戏**: 星引擎 Party (Astral Party)
- **依赖**: MelonLoader 0.6+ 且 .NET 6.0

## 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件
