@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: Mod信息配置
set MOD_NAME=TemplateMod
set MOD_VERSION=0.1.0

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

:: 步骤2: 创建临时目录
echo [2/4] 创建打包目录...
if exist "Temp" rmdir /s /q "Temp"
mkdir "Temp\Mods"
mkdir "Temp\ModResources\%MOD_NAME%"
echo [OK] 目录创建完成
echo.

:: 步骤3: 复制文件
echo [3/4] 复制Mod文件...

:: 复制DLL
copy "bin\Release\%MOD_NAME%.dll" "Temp\Mods\" >nul 2>&1
if errorlevel 1 (
    echo [错误] 找不到 %MOD_NAME%.dll
    pause
    exit /b 1
)
echo   - %MOD_NAME%.dll

:: 复制资源（如果有）
if exist "Resources" (
    xcopy "Resources\*" "Temp\ModResources\%MOD_NAME%\" /s /e /i /y >nul 2>&1
    echo   - Resources 目录
)

echo [OK] 文件复制完成
echo.

:: 步骤4: 打包
echo [4/4] 打包成ZIP...
set ZIP_NAME=%MOD_NAME%-v%MOD_VERSION%-for-Player.zip
if exist "%ZIP_NAME%" del "%ZIP_NAME%"

powershell -Command "Compress-Archive -Path 'Temp\*' -DestinationPath '%ZIP_NAME%' -Force" >nul 2>&1
if errorlevel 1 (
    echo [错误] 打包失败！
    pause
    exit /b 1
)

:: 计算文件大小
for %%F in ("%ZIP_NAME%") do set SIZE=%%~zF
if !SIZE! GTR 1048576 (
    set /a SIZE_MB=!SIZE! / 1048576
    set SIZE_STR=!SIZE_MB! MB
) else (
    set /a SIZE_KB=!SIZE! / 1024
    set SIZE_STR=!SIZE_KB! KB
)

:: 清理临时目录
rmdir /s /q "Temp"

echo [OK] 打包成功！
echo.
echo  ========================================
echo   打包完成！
echo  ========================================
echo   文件名: %ZIP_NAME%
echo   大小: %SIZE_STR%
echo.
echo   【使用方法】
echo   将 %ZIP_NAME% 解压到游戏根目录
echo   （与游戏.exe同级目录）
echo.
echo   解压后结构:
echo   Mods/          - Mod DLL文件
echo   ModResources/  - Mod资源文件
echo  ========================================
echo.

pause
