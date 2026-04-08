# 星引擎Mod构建脚本
# 使用方法:
#   .\build.ps1                    # 构建所有项目
#   .\build.ps1 -Mod CardReplacement # 构建指定Mod
#   .\build.ps1 -CopyToGame        # 构建并复制到游戏目录

param(
    [string]$Mod = "",
    [switch]$CopyToGame,
    [string]$GamePath = "F:\dowmload\steamapps\common\Astral Party\8vJXn6CN"
)

$ErrorActionPreference = "Stop"

# 颜色定义
$Green = "Green"
$Yellow = "Yellow"
$Red = "Red"
$Cyan = "Cyan"

Write-Host "========================================" -ForegroundColor $Cyan
Write-Host "  星引擎MelonLoader Mod构建脚本" -ForegroundColor $Cyan
Write-Host "========================================" -ForegroundColor $Cyan
Write-Host ""

# 获取脚本所在目录
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

# 构建Core库
function Build-Core {
    Write-Host "[1/3] 构建 Core 库..." -ForegroundColor $Yellow
    
    $CorePath = Join-Path $RootDir "src\Core"
    Set-Location $CorePath
    
    try {
        dotnet build -c Release
        if ($LASTEXITCODE -ne 0) {
            throw "Core库构建失败"
        }
        Write-Host "✓ Core库构建成功" -ForegroundColor $Green
    }
    catch {
        Write-Host "✗ Core库构建失败: $_" -ForegroundColor $Red
        exit 1
    }
    finally {
        Set-Location $RootDir
    }
}

# 构建指定Mod
function Build-Mod {
    param([string]$ModName)
    
    Write-Host "[2/3] 构建 Mod: $ModName..." -ForegroundColor $Yellow
    
    $ModPath = Join-Path $RootDir "mods\$ModName"
    
    if (-not (Test-Path $ModPath)) {
        Write-Host "✗ Mod目录不存在: $ModPath" -ForegroundColor $Red
        return $false
    }
    
    Set-Location $ModPath
    
    try {
        dotnet build -c Release
        if ($LASTEXITCODE -ne 0) {
            throw "Mod构建失败: $ModName"
        }
        Write-Host "✓ $ModName 构建成功" -ForegroundColor $Green
        return $true
    }
    catch {
        Write-Host "✗ $ModName 构建失败: $_" -ForegroundColor $Red
        return $false
    }
    finally {
        Set-Location $RootDir
    }
}

# 构建所有Mod
function Build-AllMods {
    Write-Host "[2/3] 构建所有 Mod..." -ForegroundColor $Yellow
    
    $ModsDir = Join-Path $RootDir "mods"
    $ModDirs = Get-ChildItem -Path $ModsDir -Directory | Where-Object { $_.Name -ne "_Template" }
    
    $SuccessCount = 0
    $FailCount = 0
    
    foreach ($ModDir in $ModDirs) {
        $ModName = $ModDir.Name
        if (Build-Mod -ModName $ModName) {
            $SuccessCount++
        }
        else {
            $FailCount++
        }
    }
    
    Write-Host ""
    Write-Host "构建结果: 成功 $SuccessCount, 失败 $FailCount" -ForegroundColor $Cyan
}

# 复制到游戏目录
function Copy-ToGame {
    param([string]$ModName)
    
    Write-Host "[3/3] 复制到游戏目录..." -ForegroundColor $Yellow
    
    if (-not (Test-Path $GamePath)) {
        Write-Host "✗ 游戏目录不存在: $GamePath" -ForegroundColor $Red
        Write-Host "请修改脚本中的 GamePath 或使用 -GamePath 参数指定" -ForegroundColor $Yellow
        return
    }
    
    $ModsPath = Join-Path $GamePath "Mods"
    if (-not (Test-Path $ModsPath)) {
        New-Item -ItemType Directory -Path $ModsPath -Force | Out-Null
    }
    
    if ($ModName) {
        # 复制指定Mod
        $DllPath = Join-Path $RootDir "mods\$ModName\bin\Release\net6.0\$ModName.dll"
        if (Test-Path $DllPath) {
            Copy-Item $DllPath $ModsPath -Force
            Write-Host "✓ 已复制 $ModName.dll 到游戏目录" -ForegroundColor $Green
        }
    }
    else {
        # 复制所有Mod
        $ModsDir = Join-Path $RootDir "mods"
        $ModDirs = Get-ChildItem -Path $ModsDir -Directory | Where-Object { $_.Name -ne "_Template" }
        
        foreach ($ModDir in $ModDirs) {
            $ModName = $ModDir.Name
            $DllPath = Join-Path $ModDir.FullName "bin\Release\net6.0\$ModName.dll"
            if (Test-Path $DllPath) {
                Copy-Item $DllPath $ModsPath -Force
                Write-Host "✓ 已复制 $ModName.dll" -ForegroundColor $Green
            }
        }
    }
}

# 主流程
Build-Core

if ($Mod) {
    Build-Mod -ModName $Mod
    if ($CopyToGame) {
        Copy-ToGame -ModName $Mod
    }
}
else {
    Build-AllMods
    if ($CopyToGame) {
        Copy-ToGame
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor $Cyan
Write-Host "  构建完成!" -ForegroundColor $Green
Write-Host "========================================" -ForegroundColor $Cyan
