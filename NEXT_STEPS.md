# 下一步操作指南

## 已完成 ✅
- [x] InPartyMod (主框架) 已推送到 GitHub
- [x] InPartyModManager 已链接为子模块 (@ModManager)

## 当前架构状态

```
AstralPartyModding/
├── InPartyMod/              ✅ 已完成
│   ├── src/Core/            ✅ Core库
│   ├── mods/                ✅ 官方Mod示例
│   └── @ModManager/          ✅ 子模块链接
├── InPartyModManager/       ✅ 独立仓库
├── Template/                ⏳ 待创建
└── InPartyDocs/             ⏳ 待创建
```

## 待完成

### 1. 推送 Template 到 Organization


### 2. 推送 Template 到 Organization

```bash
# 创建临时目录
cd e:/git/astparty
mkdir temp-template
cd temp-template

# 复制模板文件
cp -r mods/_Template/* .
cp plans/migration-scripts/Template/NuGet.config .

# 创建README
cat > README.md << 'EOF'
# InPartyMod Template

星引擎Mod开发模板，基于InPartyMod.Core。

## 快速开始

1. Fork本仓库
2. 修改TemplateMod.cs中的ModName/ModVersion/ModAuthor
3. 编译并测试
4. 发布Release

## 依赖

- [InPartyMod.Core](https://github.com/AstralPartyModding/InPartyMod)

## 文档

- [开发文档](https://github.com/AstralPartyModding/InPartyDocs)
EOF

# 初始化git
git init
git add .
git commit -m "Initial commit: Mod Template v0.1.0"

# 推送
git remote add origin https://github.com/AstralPartyModding/Template.git
git push -u origin main

# 清理
cd ..
rm -rf temp-template
```

### 3. 推送 Docs 到 Organization

```bash
# 创建临时目录
cd e:/git/astparty
mkdir temp-docs
cd temp-docs

# 创建目录结构
mkdir -p docs registry

# 复制文档
cp docs/registry-explanation.md docs/
cp plans/ARCHITECTURE.md docs/

# 创建 registry.json
cat > registry/registry.json << 'EOF'
{
  "version": "1.0.0",
  "lastUpdated": "2024-01-15",
  "mods": [
    {
      "id": "CardReplacement",
      "name": "星引擎卡面替换",
      "version": "0.1.0",
      "author": "AstralPartyModding",
      "type": "official",
      "repository": "https://github.com/AstralPartyModding/InPartyMod",
      "description": "通用卡面替换Mod",
      "tags": ["card", "replacement", "official"]
    },
    {
      "id": "YuGiOhCardMod",
      "name": "游戏王卡面替换",
      "version": "0.1.0",
      "author": "AstralPartyModding",
      "type": "official",
      "repository": "https://github.com/AstralPartyModding/InPartyMod",
      "description": "游戏王主题卡面替换",
      "tags": ["yugioh", "card", "official"]
    }
  ]
}
EOF

# 创建README
cat > README.md << 'EOF'
# InPartyDocs - 星引擎Mod文档中心

## 📚 文档

- [架构设计](docs/ARCHITECTURE.md)
- [Registry说明](docs/registry-explanation.md)

## 🎨 Mod索引

查看 [registry/registry.json](registry/registry.json) 获取完整Mod列表。

## 🔗 相关项目

- [InPartyMod](https://github.com/AstralPartyModding/InPartyMod) - 主项目
- [InPartyModManager](https://github.com/AstralPartyModding/InPartyModManager) - 管理器
- [Template](https://github.com/AstralPartyModding/Template) - 开发模板
EOF

# 初始化git
git init
git add .
git commit -m "Initial commit: Docs and Registry v0.1.0"

# 推送
git remote add origin https://github.com/AstralPartyModding/InPartyDocs.git
git push -u origin main

# 清理
cd ..
rm -rf temp-docs
```

### 4. 设置 GitHub Packages (可选)

在InPartyMod仓库中配置GitHub Actions自动发布Core NuGet包。

## 验证清单

- [ ] InPartyModManager 已链接为子模块
- [ ] Template 仓库已创建并推送
- [ ] InPartyDocs 仓库已创建并推送
- [ ] 所有仓库README清晰
- [ ] 可以正常clone和编译

## 最终架构

```
AstralPartyModding/
├── InPartyMod/              ← ✅ 已完成
│   ├── src/Core/
│   ├── mods/
│   └── @ModManager/          ← 待链接
├── InPartyModManager/       ← ✅ 已有
├── Template/                ← 待创建
└── InPartyDocs/             ← 待创建
```

执行完这些步骤后，整个架构就完成了！
