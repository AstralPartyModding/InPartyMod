# InPartyMod - 星引擎Mod开发框架

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![MelonLoader](https://img.shields.io/badge/MelonLoader-0.6+-orange.svg)](https://melonloader.com)

> 基于MelonLoader的星引擎（Astral Party）Mod开发框架，让资源替换Mod开发更简单。

## 快速开始

### 玩家使用

1. 安装 [MelonLoader](https://melonloader.com) 到星引擎游戏
2. 下载 [最新Release](https://github.com/AstralPartyModding/InPartyMod/releases)
3. 将DLL文件放入游戏 `Mods` 目录
4. 将Mod资源放入对应文件夹

### 开发者开发

```bash
# 1. Fork模板仓库
git clone https://github.com/AstralPartyModding/Template.git MyMod

# 2. 修改3个属性（Mod名称、版本、作者）
# 3. 编译并测试
dotnet build

# 4. 发布！
```

详细开发指南：[查看文档](https://github.com/AstralPartyModding/InPartyDocs)

## 项目结构

```
InPartyMod/
├── src/Core/           # 核心库（Mod基类、资源替换、Harmony补丁）
├── @ModManager/        # 游戏内Mod管理器（Submodule）
├── mods/               # 官方Mod示例
│   ├── CardReplacement/# 卡面替换Mod
│   └── YuGiOhCardMod/  # 游戏王卡面Mod
└── docs/               # 文档和Mod索引
```

## 核心特性

- ✅ **零侵入** - 不修改游戏文件，安全无副作用
- ✅ **热重载** - F10键重新加载资源，无需重启游戏
- ✅ **多Mod共存** - 支持多个Mod同时加载
- ✅ **极简开发** - 继承CoreMod基类，只需配置3个属性

## 相关仓库

| 仓库 | 说明 |
|------|------|
| [InPartyMod](https://github.com/AstralPartyModding/InPartyMod) | 主项目（本仓库）- 包含完整Core代码 |
| [@ModManager](https://github.com/AstralPartyModding/InPartyModManager) | 游戏内Mod管理器 |
| [Template](https://github.com/AstralPartyModding/Template) | Mod开发模板 |
| [InPartyDocs](https://github.com/AstralPartyModding/InPartyDocs) | 文档和Mod索引 |

## 社区Mod

查看 [Mod索引](https://github.com/AstralPartyModding/InPartyDocs/blob/main/registry/registry.json) 发现更多社区Mod。

## 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件
