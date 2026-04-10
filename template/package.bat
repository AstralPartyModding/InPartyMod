@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: ========== Mod信息配置（修改这里）==========
set MOD_NAME=TemplateMod
set MOD_VERSION=1.0.0
:: ============================================

echo.
echo  ========================================
echo   %MOD_NAME% v%MOD_VERSION% 一键打包
echo  ========================================
echo.

:: 步骤1: 编译
echo [1/4] 正在编译项目...
dotnet build -c Release >nul 2>&1
if errorlevel 1 (
    echo [错误] 编译失败！请检查代码错误
    pause
    exit /b 1
)
echo [OK] 编译成功
echo.

:: 查找DLL文件路径
set "DLL_PATH="
set "CORE_DLL_PATH="

for /f "delims=" %%F in ('dir /s /b bin\Release\%MOD_NAME%.dll 2^>nul') do (
    set "DLL_PATH=%%F"
)

for /f "delims=" %%F in ('dir /s /b bin\Release\AstralPartyMod.Core.dll 2^>nul') do (
    set "CORE_DLL_PATH=%%F"
)

:: 步骤2: 创建临时目录
echo [2/4] 创建打包目录...
if exist "Temp" rmdir /s /q "Temp"
mkdir "Temp\Mods"
mkdir "Temp\ModResources\%MOD_NAME%\cards"
mkdir "Temp\ModResources\%MOD_NAME%\events"
echo [OK] 目录创建完成
echo.

:: 步骤3: 复制文件
echo [3/4] 复制Mod文件...

:: 复制Mod DLL
if "!DLL_PATH!"=="" (
    echo [错误] 找不到 %MOD_NAME%.dll
    pause
    exit /b 1
)
copy "!DLL_PATH!" "Temp\Mods\" >nul 2>&1
echo   - %MOD_NAME%.dll

:: 复制Core DLL
copy "!CORE_DLL_PATH!" "Temp\Mods\" >nul 2>&1
echo   - AstralPartyMod.Core.dll

:: 复制cards资源
if exist "resources\cards" (
    xcopy "resources\cards\*" "Temp\ModResources\%MOD_NAME%\cards\" /s /e /i /y >nul 2>&1
    for /f %%A in ('dir /s /b "resources\cards\*" 2^>nul ^| find /c /v ""') do set CARDS_COUNT=%%A
    if "!CARDS_COUNT!"=="" set CARDS_COUNT=0
    echo   - cards 资源 (!CARDS_COUNT! 个文件)
)

:: 复制events资源
if exist "resources\events" (
    xcopy "resources\events\*" "Temp\ModResources\%MOD_NAME%\events\" /s /e /i /y >nul 2>&1
    for /f %%A in ('dir /s /b "resources\events\*" 2^>nul ^| find /c /v ""') do set EVENTS_COUNT=%%A
    if "!EVENTS_COUNT!"=="" set EVENTS_COUNT=0
    echo   - events 资源 (!EVENTS_COUNT! 个文件)
)

:: 生成config.json
echo {> "Temp\ModResources\%MOD_NAME%\config.json"
echo   "enableDetailedLogging": false,>> "Temp\ModResources\%MOD_NAME%\config.json"
echo   "resourceMappings": {},>> "Temp\ModResources\%MOD_NAME%\config.json"
echo   "categories": {>> "Temp\ModResources\%MOD_NAME%\config.json"
echo     "cards": {>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "enabled": true,>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "path": "resources/cards",>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "description": "手牌卡图替换",>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "resourceMappings": {}>> "Temp\ModResources\%MOD_NAME%\config.json"
echo     },>> "Temp\ModResources\%MOD_NAME%\config.json"
echo     "events": {>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "enabled": true,>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "path": "resources/events",>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "description": "事件卡图替换",>> "Temp\ModResources\%MOD_NAME%\config.json"
echo       "resourceMappings": {}>> "Temp\ModResources\%MOD_NAME%\config.json"
echo     }>> "Temp\ModResources\%MOD_NAME%\config.json"
echo   }>> "Temp\ModResources\%MOD_NAME%\config.json"
echo }>> "Temp\ModResources\%MOD_NAME%\config.json"
echo   - config.json

echo [OK] 文件复制完成
echo.

:: 步骤4: 打包
echo [4/4] 打包成ZIP...
if exist "%MOD_NAME%-v%MOD_VERSION%-for-Player.zip" del "%MOD_NAME%-v%MOD_VERSION%-for-Player.zip"

cd Temp
powershell -Command "Compress-Archive -Path * -DestinationPath ..\%MOD_NAME%-v%MOD_VERSION%-for-Player.zip -Force" >nul 2>&1
cd ..

if not exist "%MOD_NAME%-v%MOD_VERSION%-for-Player.zip" (
    echo [错误] 打包失败
    pause
    exit /b 1
)

:: 清理临时目录
rmdir /s /q "Temp" >nul 2>&1

echo [OK] 打包成功！
echo.
echo  ========================================
echo   打包完成！
echo  ========================================
echo   文件名: %MOD_NAME%-v%MOD_VERSION%-for-Player.zip
echo.
echo   【使用方法】
echo   将ZIP解压到游戏根目录（与游戏.exe同级）
echo.
echo   解压后结构:
echo   Mods/          - Mod DLL文件
echo   ModResources/  - Mod资源文件
echo     └── %MOD_NAME%/
echo         ├── cards/     - 手牌卡图
echo         ├── events/    - 事件卡图
echo         └── config.json
echo  ========================================
echo.
pause
