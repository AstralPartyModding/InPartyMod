using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AstralPartyMod.Core
{
    public class PreloadReplacementManager
    {
        private readonly string _gameAssetPath;
        private readonly string _backupPath;
        private readonly string _backupRecordFile;
        private readonly List<string> _replacedFiles = new();
        private BackupInfo? _backupInfo;

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

        public int ExecutePreloadReplacement(Dictionary<string, string> modResources)
        {
            int replacedCount = 0;

            try
            {
                MelonLogger.Msg("[预替换] 开始执行预替换...");
                Directory.CreateDirectory(_backupPath);
                LoadBackupInfo();

                foreach (var kvp in modResources)
                {
                    string fileName = kvp.Key;
                    string modPath = kvp.Value;
                    string targetPath = Path.Combine(_gameAssetPath, fileName);

                    if (!File.Exists(targetPath) || !File.Exists(modPath))
                        continue;

                    try
                    {
                        BackupOriginalFile(targetPath, fileName);
                        File.Copy(modPath, targetPath, overwrite: true);
                        _replacedFiles.Add(fileName);
                        replacedCount++;
                        MelonLogger.Msg($"[预替换] 成功替换: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[预替换] 替换失败 {fileName}: {ex.Message}");
                    }
                }

                SaveBackupInfo();
                MelonLogger.Msg($"[预替换] 完成！共替换 {replacedCount} 个资源");
                return replacedCount;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[预替换] 执行失败: {ex.Message}");
                return replacedCount;
            }
        }

        public int RestoreOriginalAssets()
        {
            int restoredCount = 0;

            try
            {
                MelonLogger.Msg("[预替换] 开始恢复原始资源...");
                LoadBackupInfo();

                if (_backupInfo?.BackedUpFiles.Count == 0)
                    return 0;

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
                            restoredCount++;
                            MelonLogger.Msg($"[预替换] 已恢复: {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[预替换] 恢复失败 {fileName}: {ex.Message}");
                    }
                }

                MelonLogger.Msg($"[预替换] 恢复完成！共恢复 {restoredCount} 个资源");
                return restoredCount;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[预替换] 恢复失败: {ex.Message}");
                return restoredCount;
            }
        }

        private void BackupOriginalFile(string originalPath, string fileName)
        {
            if (_backupInfo?.BackedUpFiles.Any(f => f.FileName == fileName) == true)
                return;

            string backupFilePath = Path.Combine(_backupPath, fileName);
            File.Copy(originalPath, backupFilePath, overwrite: true);

            var fileInfo = new FileInfo(originalPath);
            _backupInfo?.BackedUpFiles.Add(new BackedUpFileInfo
            {
                FileName = fileName,
                OriginalSize = fileInfo.Length,
                BackupTime = DateTime.Now
            });
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
        public DateTime BackupTime { get; set; }
    }
}
