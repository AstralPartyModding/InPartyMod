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
xcopy mods\_Template mods\MyMod /E /I
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
├── LICENSE                   # MIT许可证
├── src/Core/                 # AstralPartyMod.Core 核心框架
├── mods/_Template/           # Mod开发模板
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

## 手动测试

### 环境准备

1. 安装 [MelonLoader 0.6+](https://melonloader.com) 到星引擎游戏目录
2. 设置环境变量 `GAME_DIR` 指向你的游戏目录：
```batch
set GAME_DIR=F:\dowmload\steamapps\common\Astral Party\8vJXn6CN
```

### 编译框架

```batch
:: 清理并编译
dotnet clean src/Core/AstralPartyMod.Core.csproj
dotnet build src/Core/AstralPartyMod.Core.csproj --configuration Release
```

**预期结果**:
- 编译成功，输出 `0 错误`
- 可能有 `4 个警告`（Mono.Cecil版本冲突），不影响使用

### 安装测试

1. 复制编译输出 `bin\Release\net6.0\AstralPartyMod.Core.dll` 到游戏 `Mods` 目录
2. 启动游戏，查看MelonLoader控制台输出

**预期输出**:
```
[AstralPartyMod] OnInitializeMelon
[HarmonyPatcher] 初始化完成，ID: AstralPartyMod.Core
[ConfigManager] 创建配置目录: ...\UserData\Config
[ConfigManager] 初始化完成
[HarmonyPatcher] 成功应用 X 个补丁
...
```

### 检查功能

1. **Harmony补丁**: 框架会自动扫描并应用所有补丁，控制台会显示补丁数量
2. **配置系统**: 启动后会在 `UserData/Config/` 目录创建配置文件夹
3. **UnityExplorer**: 如果已安装UnityExplorer，按 `F7` 可打开调试面板
4. **事件总线**: 游戏生命周期事件会自动广播，Mod可以订阅这些事件

### 故障排除

**编译错误: 找不到MelonLoader.dll**:
- 确认 `GAME_DIR` 环境变量设置正确
- 确认游戏目录下 `MelonLoader/net6/MelonLoader.dll` 存在

**编译警告: Mono.Cecil版本冲突**:
- 这是正常的，HarmonyX和MelonLoader使用不同版本的Mono.Cecil
- 不影响运行，可以忽略

**游戏启动失败: 文件找不到**:
- 确认 `AstralPartyMod.Core.dll` 已复制到 `Mods` 目录
- 确认游戏版本兼容 (.NET 6.0，MelonLoader 0.6+)

## 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件
