using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace AstralPartyMod.Core.Assets
{
    /// <summary>
    /// 备份完整性校验结果
    /// </summary>
    public enum BackupValidationResult
    {
        Valid,
        InvalidHash,
        FileNotFound,
        SizeMismatch,
        GameVersionMismatch,
        Unknown
    }

    /// <summary>
    /// 备份校验详情
    /// </summary>
    public class BackupValidationDetail
    {
        public string FileName { get; set; } = string.Empty;
        public BackupValidationResult Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ExpectedHash { get; set; }
        public string? ActualHash { get; set; }
        public long? ExpectedSize { get; set; }
        public long? ActualSize { get; set; }
    }

    /// <summary>
    /// 预替换执行结果
    /// </summary>
    public class PreloadReplacementResult
    {
        public bool Success { get; set; }
        public int ReplacedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedFiles { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 预替换恢复结果
    /// </summary>
    public class RestoreResult
    {
        public bool Success { get; set; }
        public int RestoredCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedFiles { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }

    public class PreloadReplacementManager
    {
        private readonly string _gameAssetPath;
        private readonly string _backupPath;
        private readonly string _backupRecordFile;
        private readonly List<string> _replacedFiles = new();
        private BackupInfo? _backupInfo;
        private bool _enableHashValidation = true;
        private bool _enableAutoRollback = true;

        public static string GetGameAssetPath()
        {
            string gameRoot = MelonLoader.Utils.MelonEnvironment.GameRootDirectory;
            return Path.Combine(gameRoot, "AstralParty_CN_Data", "StreamingAssets", "aa", "StandaloneWindows64");
        }

        public PreloadReplacementManager()
        {
            _gameAssetPath = GetGameAssetPath();
            _backupPath = Path.Combine(_gameAssetPath, ".backup");
            _backupRecordFile = Path.Combine(_backupPath, "backup_info.json");
        }

        /// <summary>
        /// 是否启用哈希校验
        /// </summary>
        public bool EnableHashValidation
        {
            get => _enableHashValidation;
            set => _enableHashValidation = value;
        }

        /// <summary>
        /// 是否启用自动回滚
        /// </summary>
        public bool EnableAutoRollback
        {
            get => _enableAutoRollback;
            set => _enableAutoRollback = value;
        }

        /// <summary>
        /// 执行预替换
        /// </summary>
        public PreloadReplacementResult ExecutePreloadReplacement(Dictionary<string, string> modResources)
        {
            var result = new PreloadReplacementResult();

            try
            {
                MelonLogger.Msg("[预替换] 开始执行预替换...");
                Directory.CreateDirectory(_backupPath);
                LoadBackupInfo();

                // 游戏版本检查
                string currentGameVersion = GetGameVersion();
                if (_backupInfo != null && _backupInfo.GameVersion != currentGameVersion)
                {
                    MelonLogger.Warning($"[预替换] 游戏版本已变更: {_backupInfo.GameVersion} -> {currentGameVersion}");
                    MelonLogger.Warning("[预替换] 备份文件可能与新版本不兼容，将重新备份");

                    // 清除旧备份
                    ClearOldBackups();
                    _backupInfo = new BackupInfo
                    {
                        GameVersion = currentGameVersion,
                        BackupTime = DateTime.Now,
                        BackedUpFiles = new List<BackedUpFileInfo>()
                    };
                }

                foreach (var kvp in modResources)
                {
                    string fileName = kvp.Key;
                    string modPath = kvp.Value;
                    string targetPath = Path.Combine(_gameAssetPath, fileName);

                    if (!File.Exists(targetPath) || !File.Exists(modPath))
                    {
                        MelonLogger.Warning($"[预替换] 跳过: {fileName} (源文件或目标文件不存在)");
                        continue;
                    }

                    try
                    {
                        // 备份前校验现有备份
                        if (ShouldBackup(fileName, targetPath))
                        {
                            BackupOriginalFile(targetPath, fileName);
                        }

                        // 执行替换
                        File.Copy(modPath, targetPath, overwrite: true);
                        _replacedFiles.Add(fileName);
                        result.ReplacedCount++;
                        MelonLogger.Msg($"[预替换] 成功替换: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.FailedFiles.Add(fileName);
                        MelonLogger.Error($"[预替换] 替换失败 {fileName}: {ex.Message}");

                        // 自动回滚
                        if (_enableAutoRollback)
                        {
                            TryRollback(fileName);
                        }
                    }
                }

                SaveBackupInfo();
                result.Success = result.FailedCount == 0;
                MelonLogger.Msg($"[预替换] 完成！共替换 {result.ReplacedCount} 个资源，失败 {result.FailedCount} 个");

                if (result.FailedCount > 0)
                {
                    MelonLogger.Error($"[预替换] 失败文件: {string.Join(", ", result.FailedFiles)}");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                MelonLogger.Error($"[预替换] 执行失败: {ex.Message}");

                // 尝试自动回滚所有文件
                if (_enableAutoRollback)
                {
                    TryRollbackAll();
                }

                return result;
            }
        }

        /// <summary>
        /// 恢复原始资源
        /// </summary>
        public RestoreResult RestoreOriginalAssets()
        {
            var result = new RestoreResult();

            try
            {
                MelonLogger.Msg("[预替换] 开始恢复原始资源...");
                LoadBackupInfo();

                // 恢复前先校验备份完整性
                var validationResult = ValidateAllBackups();
                if (validationResult.Count > 0)
                {
                    int invalidCount = validationResult.Count(v => v.Result != BackupValidationResult.Valid);
                    if (invalidCount > 0)
                    {
                        MelonLogger.Warning($"[预替换] 发现 {invalidCount} 个备份文件校验失败");
                        foreach (var validation in validationResult.Where(v => v.Result != BackupValidationResult.Valid))
                        {
                            MelonLogger.Warning($"  - {validation.FileName}: {validation.Message}");
                        }
                    }
                }

                if (_backupInfo?.BackedUpFiles.Count == 0)
                    return result;

                foreach (var backedUpFile in _backupInfo!.BackedUpFiles)
                {
                    string fileName = backedUpFile.FileName;
                    string backupPath = Path.Combine(_backupPath, fileName);
                    string targetPath = Path.Combine(_gameAssetPath, fileName);

                    try
                    {
                        if (File.Exists(backupPath))
                        {
                            File.Copy(backupPath, targetPath, overwrite: true);
                            result.RestoredCount++;
                            MelonLogger.Msg($"[预替换] 已恢复: {fileName}");
                        }
                        else
                        {
                            result.FailedCount++;
                            result.FailedFiles.Add(fileName);
                            MelonLogger.Error($"[预替换] 备份文件不存在: {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.FailedFiles.Add(fileName);
                        MelonLogger.Error($"[预替换] 恢复失败 {fileName}: {ex.Message}");
                    }
                }

                result.Success = result.FailedCount == 0;
                MelonLogger.Msg($"[预替换] 恢复完成！共恢复 {result.RestoredCount} 个资源，失败 {result.FailedCount} 个");

                if (result.FailedCount > 0)
                {
                    result.ErrorMessage = $"失败文件: {string.Join(", ", result.FailedFiles)}";
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                MelonLogger.Error($"[预替换] 恢复失败: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// 校验单个备份文件
        /// </summary>
        public BackupValidationDetail ValidateBackup(string fileName)
        {
            var detail = new BackupValidationDetail { FileName = fileName };

            try
            {
                LoadBackupInfo();

                var backedUpFile = _backupInfo?.BackedUpFiles.FirstOrDefault(f =>
                    f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

                if (backedUpFile == null)
                {
                    detail.Result = BackupValidationResult.FileNotFound;
                    detail.Message = "备份记录不存在";
                    return detail;
                }

                string backupPath = Path.Combine(_backupPath, fileName);
                string targetPath = Path.Combine(_gameAssetPath, fileName);

                if (!File.Exists(backupPath))
                {
                    detail.Result = BackupValidationResult.FileNotFound;
                    detail.Message = "备份文件不存在";
                    return detail;
                }

                // 检查文件大小
                var fileInfo = new FileInfo(backupPath);
                if (fileInfo.Length != backedUpFile.OriginalSize)
                {
                    detail.Result = BackupValidationResult.SizeMismatch;
                    detail.Message = $"文件大小不匹配: 预期 {backedUpFile.OriginalSize}, 实际 {fileInfo.Length}";
                    detail.ExpectedSize = backedUpFile.OriginalSize;
                    detail.ActualSize = fileInfo.Length;
                    return detail;
                }

                // 计算哈希
                if (_enableHashValidation)
                {
                    string hash = ComputeFileHash(backupPath);
                    if (!string.IsNullOrEmpty(backedUpFile.OriginalHash) &&
                        hash != backedUpFile.OriginalHash)
                    {
                        detail.Result = BackupValidationResult.InvalidHash;
                        detail.Message = "文件哈希校验失败";
                        detail.ExpectedHash = backedUpFile.OriginalHash;
                        detail.ActualHash = hash;
                        return detail;
                    }
                    detail.ActualHash = hash;
                }

                detail.Result = BackupValidationResult.Valid;
                detail.Message = "校验通过";
                detail.ExpectedHash = backedUpFile.OriginalHash;
                detail.ExpectedSize = backedUpFile.OriginalSize;
                detail.ActualSize = fileInfo.Length;
                return detail;
            }
            catch (Exception ex)
            {
                detail.Result = BackupValidationResult.Unknown;
                detail.Message = $"校验异常: {ex.Message}";
                return detail;
            }
        }

        /// <summary>
        /// 校验所有备份文件
        /// </summary>
        public List<BackupValidationDetail> ValidateAllBackups()
        {
            var results = new List<BackupValidationDetail>();

            LoadBackupInfo();

            if (_backupInfo?.BackedUpFiles == null || _backupInfo.BackedUpFiles.Count == 0)
                return results;

            foreach (var backedUpFile in _backupInfo.BackedUpFiles)
            {
                results.Add(ValidateBackup(backedUpFile.FileName));
            }

            return results;
        }

        /// <summary>
        /// 计算文件SHA256哈希
        /// </summary>
        public static string ComputeFileHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 是否需要备份
        /// </summary>
        private bool ShouldBackup(string fileName, string targetPath)
        {
            LoadBackupInfo();

            // 如果已备份，跳过
            if (_backupInfo?.BackedUpFiles.Any(f =>
                f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)) == true)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 备份原始文件
        /// </summary>
        private void BackupOriginalFile(string originalPath, string fileName)
        {
            if (_backupInfo?.BackedUpFiles.Any(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)) == true)
                return;

            string backupFilePath = Path.Combine(_backupPath, fileName);
            File.Copy(originalPath, backupFilePath, overwrite: true);

            var fileInfo = new FileInfo(originalPath);
            var backupInfo = new BackedUpFileInfo
            {
                FileName = fileName,
                OriginalSize = fileInfo.Length,
                OriginalHash = _enableHashValidation ? ComputeFileHash(originalPath) : string.Empty,
                BackupTime = DateTime.Now
            };

            _backupInfo ??= new BackupInfo
            {
                GameVersion = GetGameVersion(),
                BackupTime = DateTime.Now,
                BackedUpFiles = new List<BackedUpFileInfo>()
            };

            _backupInfo.BackedUpFiles.Add(backupInfo);
        }

        /// <summary>
        /// 尝试回滚单个文件
        /// </summary>
        private void TryRollback(string fileName)
        {
            try
            {
                string backupPath = Path.Combine(_backupPath, fileName);
                string targetPath = Path.Combine(_gameAssetPath, fileName);

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, targetPath, overwrite: true);
                    MelonLogger.Msg($"[预替换] 已回滚: {fileName}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[预替换] 回滚失败 {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// 尝试回滚所有文件
        /// </summary>
        private void TryRollbackAll()
        {
            MelonLogger.Warning("[预替换] 正在尝试回滚所有文件...");
            LoadBackupInfo();

            if (_backupInfo?.BackedUpFiles == null)
                return;

            foreach (var backedUpFile in _backupInfo.BackedUpFiles)
            {
                TryRollback(backedUpFile.FileName);
            }
        }

        /// <summary>
        /// 清除旧备份
        /// </summary>
        private void ClearOldBackups()
        {
            try
            {
                if (Directory.Exists(_backupPath))
                {
                    var files = Directory.GetFiles(_backupPath, "*", SearchOption.TopDirectoryOnly)
                        .Where(f => !f.EndsWith(".json"));
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void LoadBackupInfo()
        {
            try
            {
                if (File.Exists(_backupRecordFile))
                {
                    string json = File.ReadAllText(_backupRecordFile);
                    _backupInfo = JsonSerializer.Deserialize<BackupInfo>(json);
                }
                else
                {
                    _backupInfo = new BackupInfo
                    {
                        GameVersion = GetGameVersion(),
                        BackupTime = DateTime.Now,
                        BackedUpFiles = new List<BackedUpFileInfo>()
                    };
                }
            }
            catch
            {
                _backupInfo = new BackupInfo
                {
                    GameVersion = GetGameVersion(),
                    BackupTime = DateTime.Now,
                    BackedUpFiles = new List<BackedUpFileInfo>()
                };
            }
        }

        private void SaveBackupInfo()
        {
            try
            {
                if (_backupInfo != null)
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_backupInfo, options);
                    File.WriteAllText(_backupRecordFile, json);
                }
            }
            catch { }
        }

        private string GetGameVersion()
        {
            try
            {
                return MelonLoader.InternalUtils.UnityInformationHandler.GameVersion ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public IReadOnlyList<string> GetReplacedFiles() => _replacedFiles.AsReadOnly();
        public bool HasBackups() => _backupInfo?.BackedUpFiles.Count > 0;
        public int GetBackupCount() => _backupInfo?.BackedUpFiles.Count ?? 0;
    }

    public class BackupInfo
    {
        public string GameVersion { get; set; } = "Unknown";
        public DateTime BackupTime { get; set; }
        public List<BackedUpFileInfo> BackedUpFiles { get; set; } = new();
    }

    public class BackedUpFileInfo
    {
        public string FileName { get; set; } = "";
        public long OriginalSize { get; set; }
        public string OriginalHash { get; set; } = "";
        public DateTime BackupTime { get; set; }
    }
}
