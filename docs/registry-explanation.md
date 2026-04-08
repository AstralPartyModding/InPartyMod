# Registry (Mod注册中心) 说明

## 什么是Registry？

**Registry** 是 Mod 注册中心，类似一个"Mod黄页"或"应用商店索引"。

## 作用

1. **聚合所有Mod** - 官方Mod + 社区Mod的统一列表
2. **Mod发现** - 玩家可以在这里找到所有可用Mod
3. **版本管理** - 记录Mod版本、更新信息
4. **下载链接** - 指向各Mod的Release下载地址

## 核心文件：registry.json

```json
{
  "version": "1.0.0",
  "lastUpdated": "2024-01-15",
  "mods": [
    {
      "id": "CardReplacement",
      "name": "星引擎卡面替换",
      "version": "1.0.0",
      "author": "官方",
      "type": "official",
      "repository": "https://github.com/AstralPartyModding/CardReplacement",
      "downloadUrl": "https://github.com/AstralPartyModding/CardReplacement/releases/download/v1.0.0/CardReplacement.dll",
      "description": "通用卡面替换Mod",
      "tags": ["card", "replacement"],
      "dependencies": ["AstralPartyMod.Core>=1.0.0"],
      "gameVersion": "*",
      "lastUpdated": "2024-01-15"
    },
    {
      "id": "GenshinMod",
      "name": "原神角色替换",
      "version": "1.2.0",
      "author": "开发者A",
      "type": "community",
      "repository": "https://github.com/开发者A/GenshinMod",
      "downloadUrl": "https://github.com/开发者A/GenshinMod/releases/download/v1.2.0/GenshinMod.dll",
      "description": "将角色替换为原神角色",
      "tags": ["genshin", "character"],
      "dependencies": ["AstralPartyMod.Core>=1.0.0"],
      "gameVersion": ">=1.0.0",
      "lastUpdated": "2024-01-10"
    }
  ]
}
```

## 工作流程

```
开发者开发Mod → 发布Release → 提交PR到Registry → 审核合并 → 玩家可以看到
```

## 类比

| 平台 | 类似功能 |
|------|---------|
| Minecraft | CurseForge |
| Beat Saber | ModAssistant列表 |
| VS Code | Extension Marketplace |
| 手机 | 应用商店 |

## 简单总结

**Registry = Mod的"黄页"**

- 不存储Mod文件本身（在各自仓库）
- 只存储Mod信息（名称、版本、下载链接）
- 让玩家和ModManager知道有哪些Mod可用

## 在架构中的位置

```
AstralPartyModding/
├── InPartyMod/           ← 主项目
├── ModManager/           ← 管理器（从Registry获取Mod列表）
├── Template/             ← 模板
├── Docs/                 ← 文档
├── Registry/             ← Mod注册中心（本文件）
└── 官方Mods/
```

## 谁维护Registry？

- **官方Mod**：自动添加
- **社区Mod**：开发者提交PR，审核后合并
