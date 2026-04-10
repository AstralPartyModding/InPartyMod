# Mod开发者打包指南

> 本文档指导Mod开发者如何正确打包和发布自己的Mod。

---

## 一、推荐打包结构

一个标准的Mod发布包（ZIP文件）应包含以下内容：

```
MyMod-v1.0.0-for-Player.zip
├── Mods/
│   ├── MyMod.dll              # Mod主程序（必需）
│   └── AstralPartyMod.Core.dll # 核心库（必需）
└── ModResources/
    └── MyMod/
        ├── cards/             # 手牌卡图资源
        ├── events/            # 事件卡图资源
        └── config.json        # 分类配置
```

### 目录说明

| 路径 | 说明 |
|------|------|
| `Mods/MyMod.dll` | Mod主程序文件，编译后的DLL |
| `Mods/AstralPartyMod.Core.dll` | 核心库，提供预替换功能 |
| `ModResources/MyMod/` | Mod资源目录，包含替换资源 |
| `config.json` | 分类配置文件 |

### 使用package.bat自动打包

每个Mod目录下都有 `package.bat` 脚本，双击运行即可自动生成标准ZIP包：

```bash
cd mods/MyMod
package.bat
# 生成: MyMod-v1.0.0-for-Player.zip
```

---

## 二、必须包含的文件清单

### 必需文件

| 文件 | 类型 | 说明 |
|------|------|------|
| `MyMod.dll` | DLL | Mod主程序，MelonLoader加载的入口 |

### 依赖检查

发布前请确认：

- [ ] DLL文件已正确编译（Release模式）
- [ ] DLL文件名与Mod名称一致（建议）
- [ ] 资源文件路径正确（区分大小写）
- [ ] 已测试在游戏中的运行情况

---

## 三、可选文件

### 推荐包含

| 文件 | 类型 | 说明 |
|------|------|------|
| `README.txt` | 文本 | Mod说明、安装方法、更新日志 |
| `Resources/` | 文件夹 | 图片、音频等替换资源 |
| `LICENSE` | 文本 | 许可证文件（如使用开源协议）

### 特殊情况

| 文件 | 类型 | 何时需要 |
|------|------|----------|
| `AstralPartyMod.Core.dll` | DLL | 用户未安装InPartyMod主包时 |
| `MelonLoader.dll` | DLL | 通常不需要，由MelonLoader提供 |

---

## 四、README.txt模板

以下是推荐的README.txt模板，请根据实际情况修改：

```text
====================================
  [Mod名称] v[版本号]
  作者：[你的名字]
====================================

【简介】
简要描述这个Mod的功能和特点。

【安装方法】
1. 确保已安装 MelonLoader 到星引擎游戏
2. 将 MyMod.dll 放入游戏目录的 Mods 文件夹
3. 将 Resources 文件夹放入游戏目录（如有资源文件）
4. 启动游戏即可生效

【使用方法】
- 按 F10 键可热重载资源（无需重启游戏）
- 详细说明Mod的使用方式

【更新日志】
v1.0.0 (2024-XX-XX)
- 初始版本发布
- 功能1
- 功能2

【依赖说明】
- 需要安装 InPartyMod 主包（如已包含Core.dll则不需要）
- MelonLoader 0.6.0 或更高版本

【注意事项】
- 本Mod仅供学习交流使用
- 请勿用于商业用途
- 使用本Mod产生的任何问题由用户自行承担

【联系方式】
- GitHub: https://github.com/你的用户名
- 问题反馈: https://github.com/你的用户名/你的仓库/issues

====================================
  感谢使用！
====================================
```

---

## 五、发布方式建议

### 推荐：GitHub Release

使用GitHub Release发布Mod是最推荐的方式：

#### 步骤

1. **创建Release**
   - 进入GitHub仓库页面
   - 点击右侧 "Releases" → "Create a new release"

2. **填写版本信息**
   ```
   Tag version: v1.0.0
   Release title: MyMod v1.0.0
   ```

3. **编写Release说明**
   ```markdown
   ## 更新内容
   - 新增功能X
   - 修复问题Y

   ## 安装方法
   1. 下载 MyMod_v1.0.0.zip
   2. 解压到游戏目录
   3. 启动游戏

   ## 依赖
   - InPartyMod Core v1.0+
   - MelonLoader 0.6+
   ```

4. **上传文件**
   - 将打包好的ZIP文件拖入上传区域
   - 点击 "Publish release"

#### 版本号规范

遵循 [语义化版本](https://semver.org/lang/zh-CN/)：

```
主版本号.次版本号.修订号

示例：
v1.0.0    # 初始版本
v1.1.0    # 新增功能
v1.1.1    # 修复bug
v2.0.0    # 重大更新（可能不兼容）
```

### 其他发布渠道

| 渠道 | 说明 |
|------|------|
| GitHub Release | ⭐ 推荐，支持版本管理 |
| 社区论坛 | 可在星引擎相关社区发布 |
| QQ群/Discord | 适合快速分享测试版本 |

---

## 六、手动打包步骤（Windows命令）

### 方法1：使用PowerShell

```powershell
# 1. 进入编译输出目录
cd bin/Release/net6.0

# 2. 创建打包目录
New-Item -ItemType Directory -Force -Path "Package/MyMod"

# 3. 复制DLL文件
Copy-Item "MyMod.dll" "Package/MyMod/"

# 4. 复制资源文件（如有）
Copy-Item -Recurse "Resources" "Package/MyMod/" -ErrorAction SilentlyContinue

# 5. 复制README
Copy-Item "README.txt" "Package/MyMod/" -ErrorAction SilentlyContinue

# 6. 打包为ZIP
Compress-Archive -Path "Package/MyMod/*" -DestinationPath "MyMod_v1.0.0.zip" -Force

# 7. 清理临时目录
Remove-Item -Recurse -Force "Package"
```

### 方法2：使用CMD批处理

创建 `package.bat` 文件：

```batch
@echo off
set MOD_NAME=MyMod
set VERSION=1.0.0

echo 正在打包 %MOD_NAME% v%VERSION%...

:: 创建临时目录
mkdir "Package\%MOD_NAME%"

:: 复制文件
copy "bin\Release\net6.0\%MOD_NAME%.dll" "Package\%MOD_NAME%\"
xcopy /E /I /Y "Resources" "Package\%MOD_NAME%\Resources\" 2>nul
copy "README.txt" "Package\%MOD_NAME%\" 2>nul

:: 打包
cd Package
powershell -Command "Compress-Archive -Path '%MOD_NAME%\*' -DestinationPath '..\%MOD_NAME%_v%VERSION%.zip' -Force"
cd ..

:: 清理
rmdir /S /Q "Package"

echo 打包完成: %MOD_NAME%_v%VERSION%.zip
pause
```

### 方法3：使用7-Zip（如已安装）

```batch
:: 使用7-Zip命令行打包
"C:\Program Files\7-Zip\7z.exe" a -tzip MyMod_v1.0.0.zip ".\bin\Release\net6.0\MyMod.dll"
"C:\Program Files\7-Zip\7z.exe" a -tzip MyMod_v1.0.0.zip ".\Resources\" -r
"C:\Program Files\7-Zip\7z.exe" a -tzip MyMod_v1.0.0.zip ".\README.txt"
```

---

## 七、重要提醒

### ⚠️ 版权与法律

1. **游戏版权**
   - 本Mod框架仅供学习交流使用
   - 请勿将Mod用于商业用途
   - 尊重星引擎游戏的知识产权

2. **资源版权**
   - 使用他人创作的资源（图片、音频等）需获得授权
   - 建议在README中注明资源来源
   - 避免使用受版权保护的商业素材

3. **免责声明**
   - 建议在README中包含免责声明
   - 明确Mod作者不对使用后果负责

### ⚠️ Core依赖说明

| 情况 | 处理方式 |
|------|----------|
| 用户已安装InPartyMod | 只需发布Mod DLL |
| 用户未安装InPartyMod | 需同时包含 `AstralPartyMod.Core.dll` |

**建议做法**：
- 在README中明确说明依赖关系
- 推荐用户先安装InPartyMod主包
- 如包含Core.dll，请确保版本匹配

### ⚠️ 发布前检查清单

- [ ] 使用Release模式编译（非Debug）
- [ ] 删除不必要的.pdb文件（可选）
- [ ] 测试Mod在干净环境中的运行情况
- [ ] 检查资源文件路径是否正确
- [ ] 确认版本号已更新
- [ ] 编写完整的README.txt
- [ ] 检查依赖关系说明

### ⚠️ 版本兼容性

| InPartyMod版本 | 兼容的Core版本 |
|----------------|----------------|
| v1.0.x | Core v1.0.x |
| v1.1.x | Core v1.1.x |

**提示**：Core版本不匹配可能导致Mod无法加载，请在发布前测试。

---

## 八、常见问题

### Q: 用户反馈Mod无法加载？

**可能原因**：
1. 未安装MelonLoader
2. 未安装InPartyMod主包（且Mod未包含Core.dll）
3. Core版本不匹配
4. DLL编译目标框架不正确

**解决方案**：
- 在README中详细说明安装步骤
- 提供依赖检查清单

### Q: 资源文件没有生效？

**可能原因**：
1. 资源路径大小写不匹配（Linux/Mac敏感）
2. 资源文件格式不支持
3. 资源目录结构不正确

**解决方案**：
- 使用统一的资源目录结构
- 在Windows和Linux环境下都进行测试

### Q: 如何更新已发布的Mod？

**步骤**：
1. 更新代码中的版本号
2. 重新编译
3. 在GitHub创建新的Release
4. 使用新的版本标签（如v1.0.1）
5. 上传新的ZIP文件

---

## 相关链接

| 资源 | 链接 |
|------|------|
| 主项目 | https://github.com/AstralPartyModding/InPartyMod |
| 开发模板 | https://github.com/AstralPartyModding/Template |
| 文档/Mod索引 | https://github.com/AstralPartyModding/InPartyDocs |
| 语义化版本规范 | https://semver.org/lang/zh-CN/ |

---

*最后更新：2024年*
