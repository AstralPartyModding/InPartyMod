using MelonLoader;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AstralPartyMod.Core
{
    public abstract class CoreMod : MelonMod
    {
        protected abstract string ModName { get; }
        protected abstract string ModVersion { get; }
        protected abstract string ModAuthor { get; }
        protected abstract string[] ResourceDirectories { get; }

        protected virtual KeyCode ReloadKey => KeyCode.F10;
        protected virtual bool EnableStatistics => false;
        protected virtual bool EnableDetailedLogging => false;
        protected virtual bool UseCategoricalResources => false;

        public ResourceReplacer ResourceReplacer { get; private set; } = null!;
        protected PreloadReplacementManager? PreloadManager { get; private set; }
        protected ModConfigBase Config { get; private set; } = new ModConfigBase();

        private int _replacedCount = 0;
        private int _totalResources = 0;

        public int ReplacedCount => _replacedCount;
        public int TotalResources => _totalResources;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("========================================");
            MelonLogger.Msg($"{ModName} v{ModVersion}");
            MelonLogger.Msg($"作者: {ModAuthor}");
            MelonLogger.Msg("========================================");

            try
            {
                LoadConfig();
                ResourceReplacer = new ResourceReplacer();
                ResourceReplacer.OnResourceReplaced += () => _replacedCount++;
                ScanResources();
                ExecutePreloadReplacement();
                ApplyPatches();
                AssetBundlePatches.RegisterMod(this);
                MelonLogger.Msg($"已加载 {_totalResources} 个资源，可替换 {ResourceReplacer.Count} 个");
                MelonLogger.Msg($"按 {ReloadKey} 重新加载资源");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Mod初始化失败: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(ReloadKey))
                ReloadResources();
        }

        public override void OnDeinitializeMelon()
        {
            RestoreOriginalAssets();
            HarmonyInstance?.UnpatchSelf();
            AssetBundlePatches.UnregisterMod(this);
            MelonLogger.Msg($"{ModName}已卸载，本次共替换了 {_replacedCount} 次资源");
        }

        protected virtual void ScanResources()
        {
            if (UseCategoricalResources && Config.Categories.Count > 0)
                ScanCategoricalResources();
            else
                ScanTraditionalResources();
        }

        protected virtual void ScanTraditionalResources()
        {
            string resourcesDir = GetModResourcesDirectory();
            _totalResources = 0;
            foreach (var dirName in ResourceDirectories)
            {
                string fullPath = Path.Combine(resourcesDir, dirName);
                if (Directory.Exists(fullPath))
                {
                    var files = Directory.GetFiles(fullPath, "*.bundle", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        _totalResources++;
                        if (Config.ResourceMappings.TryGetValue(fileName, out string? targetName) && !string.IsNullOrEmpty(targetName))
                            ResourceReplacer.AddReplacement(targetName, file);
                        else
                            ResourceReplacer.AddReplacement(fileName, file);
                    }
                    MelonLogger.Msg($"{dirName}目录: {files.Length} 个文件");
                }
                else
                {
                    Directory.CreateDirectory(fullPath);
                    MelonLogger.Warning($"创建资源目录: {fullPath}");
                }
            }
        }

        protected virtual void ScanCategoricalResources()
        {
            string resourcesDir = GetModResourcesDirectory();
            _totalResources = 0;
            foreach (var kvp in Config.Categories)
            {
                string categoryName = kvp.Key;
                var category = kvp.Value;
                if (!category.Enabled)
                {
                    MelonLogger.Msg($"[分类] {categoryName} 已禁用，跳过扫描");
                    continue;
                }
                string dirPath = string.IsNullOrEmpty(category.Path) ? categoryName : category.Path;
                string fullPath = Path.Combine(resourcesDir, dirPath);
                if (Directory.Exists(fullPath))
                {
                    int categoryResourceCount = ScanCategory(categoryName, category, fullPath);
                    MelonLogger.Msg($"[分类] {categoryName}: {categoryResourceCount} 个资源 ({category.Description})");
                }
                else
                {
                    MelonLogger.Warning($"[分类] {categoryName} 目录不存在: {fullPath}");
                }
            }
        }

        protected virtual int ScanCategory(string categoryName, ResourceCategory category, string fullPath)
        {
            int count = 0;
            var files = Directory.GetFiles(fullPath, "*.bundle", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                _totalResources++;
                count++;
                string? targetName = null;
                bool hasCategoryMapping = category.ResourceMappings.TryGetValue(fileName, out targetName) && !string.IsNullOrEmpty(targetName);
                if (!hasCategoryMapping)
                    Config.ResourceMappings.TryGetValue(fileName, out targetName);
                if (!string.IsNullOrEmpty(targetName))
                    ResourceReplacer.AddReplacement(targetName, file);
                else
                    ResourceReplacer.AddReplacement(fileName, file);
            }
            return count;
        }

        protected virtual void ReloadResources()
        {
            MelonLogger.Msg("重新加载Mod资源...");
            ResourceReplacer.Clear();
            _replacedCount = 0;
            if (UseCategoricalResources)
                LoadConfig();
            ScanResources();
            MelonLogger.Msg($"已重新加载 {_totalResources} 个资源");
        }

        protected virtual void LoadConfig()
        {
            string configPath = Path.Combine(GetModDirectory(), "config.json");
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    Config = JsonSerializer.Deserialize<ModConfigBase>(json) ?? new ModConfigBase();
                    MelonLogger.Msg("配置文件加载成功");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"加载配置文件失败: {ex.Message}，使用默认配置");
                    Config = new ModConfigBase();
                }
            }
            else
            {
                Config = new ModConfigBase();
                MelonLogger.Msg("已创建默认配置文件");
            }
            if (UseCategoricalResources)
            {
                InitializeDefaultCategories();
                SaveConfig();
            }
        }

        protected virtual void SaveConfig()
        {
            string configPath = Path.Combine(GetModDirectory(), "config.json");
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Config, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"保存配置文件失败: {ex.Message}");
            }
        }

        protected virtual void InitializeDefaultCategories()
        {
            foreach (var dir in ResourceDirectories)
            {
                if (!Config.Categories.ContainsKey(dir))
                {
                    Config.Categories[dir] = new ResourceCategory
                    {
                        Path = dir,
                        Description = $"{dir}资源",
                        Enabled = true
                    };
                }
            }
        }

        public Dictionary<string, ResourceCategory> GetCategories()
        {
            return new Dictionary<string, ResourceCategory>(Config.Categories);
        }

        public bool SetCategoryEnabled(string category, bool enabled)
        {
            bool result = Config.SetCategoryEnabled(category, enabled);
            if (result)
            {
                SaveConfig();
                MelonLogger.Msg($"分类 '{category}' 已{(enabled ? "启用" : "禁用")}");
            }
            return result;
        }

        public void SetCategory(string categoryName, ResourceCategory category)
        {
            Config.SetCategory(categoryName, category);
            SaveConfig();
        }

        protected virtual void ApplyPatches()
        {
            try
            {
                var loadFromFileMethod = typeof(AssetBundle).GetMethod("LoadFromFile", new[] { typeof(string) });
                if (loadFromFileMethod != null)
                {
                    HarmonyInstance?.Patch(loadFromFileMethod,
                        prefix: new HarmonyMethod(typeof(AssetBundlePatches), nameof(AssetBundlePatches.LoadFromFile_Prefix)));
                    MelonLogger.Msg("已补丁 AssetBundle.LoadFromFile");
                }
                var loadFromFileAsyncMethod = typeof(AssetBundle).GetMethod("LoadFromFileAsync", new[] { typeof(string) });
                if (loadFromFileAsyncMethod != null)
                {
                    HarmonyInstance?.Patch(loadFromFileAsyncMethod,
                        prefix: new HarmonyMethod(typeof(AssetBundlePatches), nameof(AssetBundlePatches.LoadFromFileAsync_Prefix)));
                    MelonLogger.Msg("已补丁 AssetBundle.LoadFromFileAsync");
                }
                AssetBundlePatches.TryPatchAddressables(HarmonyInstance!);
                AssetBundlePatches.TryPatchResources(HarmonyInstance!);
                MelonLogger.Msg("Harmony补丁应用成功");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"应用补丁失败: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

        public static string GetModDirectory()
        {
            string? assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? modDir = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(modDir) || !Directory.Exists(modDir))
                modDir = Directory.GetCurrentDirectory();
            return modDir;
        }

        protected virtual string GetModResourcesDirectory()
        {
            string? assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? modDir = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(modDir))
                modDir = Directory.GetCurrentDirectory();
            string? gameRoot = Directory.GetParent(modDir)?.FullName;
            if (string.IsNullOrEmpty(gameRoot))
                gameRoot = modDir;
            string modName = GetType().Name;
            return Path.Combine(gameRoot, "ModResources", modName);
        }

        public bool TryGetReplacement(string fileName, out string? modPath)
        {
            return ResourceReplacer.TryGetReplacement(fileName, out modPath);
        }

        protected virtual void ExecutePreloadReplacement()
        {
            try
            {
                if (ResourceReplacer.Count == 0)
                {
                    MelonLogger.Msg("[预替换] 没有需要替换的资源");
                    return;
                }
                PreloadManager = new PreloadReplacementManager();
                var replacements = ResourceReplacer.GetAllReplacements();
                int replacedCount = PreloadManager.ExecutePreloadReplacement(replacements.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                _replacedCount = replacedCount;
                MelonLogger.Msg($"[预替换] 启动时已完成 {replacedCount} 个资源的预替换");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[预替换] 执行失败: {ex.Message}");
            }
        }

        protected virtual void RestoreOriginalAssets()
        {
            try
            {
                if (PreloadManager == null)
                    return;
                MelonLogger.Msg("[预替换] 游戏退出，开始恢复原始资源...");
                int restoredCount = PreloadManager.RestoreOriginalAssets();
                MelonLogger.Msg($"[预替换] 已恢复 {restoredCount} 个原始资源");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[预替换] 恢复失败: {ex.Message}");
            }
        }
    }
}
