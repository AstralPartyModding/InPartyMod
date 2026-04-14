# Mod开发模板

基于 [InPartyMod.Core](https://github.com/AstralPartyModding/InPartyMod) 的Mod开发模板。

## 快速开始

```bash
# 1. Fork本仓库并改名
git clone https://github.com/AstralPartyModding/Template.git MyMod

# 2. 修改3个属性
cd MyMod
# - TemplateMod.cs: ModName, ModVersion, ModAuthor
# - TemplateMod.csproj: AssemblyName, RootNamespace

# 3. 编译
dotnet build

# 4. 测试
# 将生成的DLL放入游戏Mods目录
```

## 目录结构

```
MyMod/
├── MyMod.csproj      # 项目文件
├── MyMod.cs          # Mod代码（继承CoreMod）
└── README.md         # 说明文档
```

游戏内目录：
```
Mods/
├── MyMod.dll         # Mod文件
└── MyMod/            # 数据目录
    ├── config.json
    └── Resources/    # 资源文件
```

## 更多信息

- [开发文档](https://github.com/AstralPartyModding/Docs)
- [API参考](https://github.com/AstralPartyModding/InPartyMod/tree/main/src/Core)
