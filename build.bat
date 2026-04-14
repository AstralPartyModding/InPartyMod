@echo off
REM ========================================
REM AstralParty Mod 一键构建脚本
REM 架构：每个Mod直接被MelonLoader加载，外置桌面管理器提前管理
REM 作用：编译核心库和所有已存在的Mod，输出到out目录
REM ========================================

echo ========================================
echo  AstralParty Mod 构建脚本
echo ========================================
echo.

REM 创建输出目录和mods目录
if not exist out mkdir out
if not exist mods mkdir mods
echo [1/...] 清理输出目录...
del /Q out\*.dll 2>nul
del /Q out\*.pdb 2>nul

echo.
echo [2/...] 正在编译核心库...
echo 所有Mod都依赖这个核心库...
dotnet build ".\src\Core\AstralPartyMod.Core.csproj" --configuration Release
if %errorlevel% neq 0 (
    echo 错误：核心库编译失败！
    pause
    exit /b 1
)
copy ".\src\Core\bin\Release\net6.0\AstralPartyMod.Core.dll" out\ /y
copy ".\src\Core\bin\Release\net6.0\AstralPartyMod.Core.pdb" out\ /y
echo 核心库编译完成 ✓
echo.

REM 遍历mods目录下所有.csproj文件编译
set MOD_COUNT=0
for /r mods %%f in (*.csproj) do (
    set /a MOD_COUNT+=1
    echo.
    echo [+] 正在编译 %%~nf...
    dotnet build "%%f" --configuration Release
    if errorlevel 1 (
        echo 警告：%%~nf 编译失败，跳过...
    ) else (
        copy "%%~dpfbin\Release\net6.0\%%~nf.dll" out\ /y
        copy "%%~dpfbin\Release\net6.0\%%~nf.pdb" out\ /y 2>nul
        echo %%~nf 编译完成 ✓
    )
)

echo.
echo [+] 正在编译 AstralPartyModManager (MelonLoader插件)...
dotnet build ".\@ModManager\src\AstralPartyModManager.MelonLoader.csproj" --configuration Release
if %errorlevel% equ 0 (
    copy ".\@ModManager\bin\Release\net6.0\AstralPartyModManager.dll" out\ /y
    copy ".\@ModManager\bin\Release\net6.0\AstralPartyModManager.pdb" out\ /y 2>nul
    echo AstralPartyModManager 编译完成 ✓
) else (
    echo 警告：AstralPartyModManager 编译失败，跳过（@ModManager需要作为子模块检出）
)

echo.
echo ========================================
echo  构建完成！所有输出文件都在 out 目录：
echo ========================================
dir out\*.dll /B
echo.
echo  使用方法：
echo  - 将 out/AstralPartyMod.Core.dll 复制到游戏 Mods 目录
echo  - 将 out/AstralPartyModManager.dll 复制到游戏 Mods 目录（Mod管理器）
echo  - 将 out/*.dll 其他Mod复制到游戏 Mods 目录
echo.
pause
