@echo off
REM ========================================
REM AstralParty Mod 一键构建脚本
REM 架构：每个Mod直接被MelonLoader加载，外置桌面管理器提前管理
REM 作用：编译核心库和所有Mod，输出到out目录
REM ========================================

echo ========================================
echo  AstralParty Mod 构建脚本
echo ========================================
echo.

REM 创建输出目录
if not exist out mkdir out
echo [1/4] 清理输出目录...
del /Q out\*.dll 2>nul
del /Q out\*.pdb 2>nul

echo.
echo [2/4] 正在编译核心库...
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

echo [3/4] 正在编译 SpeedHack 加速Mod...
dotnet build ".\mods\SpeedHack\SpeedHack.csproj" --configuration Release
if %errorlevel% neq 0 (
    echo 错误：SpeedHack编译失败！
    pause
    exit /b 1
)
copy ".\mods\SpeedHack\bin\Release\net6.0\SpeedHack.dll" out\ /y
copy ".\mods\SpeedHack\bin\Release\net6.0\SpeedHack.pdb" out\ /y
echo SpeedHack编译完成 ✓
echo.

echo [4/4] 正在编译 YuGiOhCardMod 卡图Mod...
dotnet build ".\mods\YuGiOhCardMod\YuGiOhCardMod.csproj" --configuration Release
if %errorlevel% neq 0 (
    echo 错误：YuGiOhCardMod编译失败！
    pause
    exit /b 1
)
copy ".\mods\YuGiOhCardMod\bin\Release\net6.0\YuGiOhCardMod.dll" out\ /y
copy ".\mods\YuGiOhCardMod\bin\Release\net6.0\YuGiOhCardMod.pdb" out\ /y
echo YuGiOhCardMod编译完成 ✓
echo.

echo ========================================
echo  构建完成！所有输出文件都在 out 目录：
echo ========================================
dir out\*.dll /B
echo.
echo  使用方法：
echo  - 将 out/AstralPartyMod.Core.dll 复制到游戏 Mods 目录
echo  - 将 out/SpeedHack.dll 复制到游戏 Mods 目录 （如需加速功能）
echo  - 将 out/YuGiOhCardMod.dll 复制到游戏 Mods 目录 （如需游戏王卡图）
echo  - 使用桌面 ModManager.exe 在启动前管理启用禁用
echo.
pause
