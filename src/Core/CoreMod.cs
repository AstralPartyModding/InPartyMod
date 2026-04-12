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
            string? currentDir = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(currentDir))
                currentDir = Directory.GetCurrentDirectory();
            
            string modName = GetType().Name;
            
            // 尝试从当前目录向上查找，找到包含 AstralParty.exe 或 Astral Party.exe 的目录作为游戏根目录
            string? gameRoot = FindGameRoot(currentDir);
            if (string.IsNullOrEmpty(gameRoot))
                gameRoot = currentDir;
            
            // 优先检查游戏根目录下是否有 ModResources
            string candidatePath = Path.Combine(gameRoot, "ModResources", modName);
            if (Directory.Exists(candidatePath))
            {
                // 只有当这个目录内确实有cards 或者 events 子目录时才返回，
                // 避免因为空目录存在就错误返回，而实际资源在其他位置
                string checkCards = Path.Combine(candidatePath, "cards");
                string checkEvents = Path.Combine(candidatePath, "events");
                if (Directory.Exists(checkCards) || Directory.Exists(checkEvents))
                {
                    return candidatePath;
                }
            }
            
            // 检查游戏根目录下是否直接有 ModResources/cards 或 ModResources/events
            // 处理打包格式：ModResources\cards 而不是 ModResources\{modName}\cards
            string rootModResCardsCheck = Path.Combine(gameRoot, "ModResources", "cards");
            string rootModResEventsCheck = Path.Combine(gameRoot, "ModResources", "events");
            if (Directory.Exists(rootModResCardsCheck) || Directory.Exists(rootModResEventsCheck))
            {
                // 找到了，返回 ModResources 目录
                string modResDir = Path.Combine(gameRoot, "ModResources");
                return modResDir;
            }
            
            // 检查是否从父级的 ModResources 目录加载
            string foundParentPath = string.Empty;
            if (DoesDirectoryHaveParentModResources(currentDir, modName, out foundParentPath))
            {
                return foundParentPath;
            }
            
            // 直接检查：如果当前目录下就有cards或者events目录，直接返回当前目录
            // 这解决了绝大多数社区mod的打包布局问题
            string cardsCheck = Path.Combine(currentDir, "cards");
            string eventsCheck = Path.Combine(currentDir, "events");
            if (Directory.Exists(cardsCheck) || Directory.Exists(eventsCheck))
            {
                // 找到了常见分类目录，直接使用当前目录
                return currentDir;
            }

            // 检查：同级的 ModResources 目录下是否有 cards 或 events
            string modResCardsCheck = Path.Combine(currentDir, "ModResources", "cards");
            string modResEventsCheck = Path.Combine(currentDir, "ModResources", "events");
            if (Directory.Exists(modResCardsCheck) || Directory.Exists(modResEventsCheck))
            {
                // 找到了，返回 ModResources 目录
                string modResDir = Path.Combine(currentDir, "ModResources");
                return modResDir;
            }
            
            // 检查：当前目录下有同名子目录，子目录里面有 ModResources/cards
            // 处理：Mods/{modName}.dll + Mods/{modName}/ModResources/cards
            string subDirModResCards = Path.Combine(currentDir, modName, "ModResources", "cards");
            string subDirModResEvents = Path.Combine(currentDir, modName, "ModResources", "events");
            if (Directory.Exists(subDirModResCards) || Directory.Exists(subDirModResEvents))
            {
                // 找到了，返回 ModResources 目录
                string modResDir = Path.Combine(currentDir, modName, "ModResources");
                return modResDir;
            }

            // 向上查找：在任何上级目录中找 ModResources/cards 或 ModResources/events
            string? checkParent = currentDir;
            while (!string.IsNullOrEmpty(checkParent))
            {
                string parentCards = Path.Combine(checkParent, "ModResources", "cards");
                string parentEvents = Path.Combine(checkParent, "ModResources", "events");
                if (Directory.Exists(parentCards) || Directory.Exists(parentEvents))
                {
                    string modResDir = Path.Combine(checkParent, "ModResources");
                    return modResDir;
                }
                var dirInfo = Directory.GetParent(checkParent);
                if (dirInfo == null)
                    break;
                checkParent = dirInfo.FullName;
            }

             // 如果当前目录下直接包含cards或者events目录，说明资源就在这里
             // 这是最常见的社区mod打包方式: Mods/{modName}/dll + Mods/{modName}/cards...
             // 这里必须确认确实有cards或events目录，而不只是随便有子目录就返回
             string currentCards = Path.Combine(currentDir, "cards");
             string currentEvents = Path.Combine(currentDir, "events");
             if (Directory.Exists(currentCards) || Directory.Exists(currentEvents))
             {
                 // 当前mod目录确实包含分类目录，直接使用当前目录
                 return currentDir;
             }
             
             // 最后检查：当前mod目录下是否有 ModResources 包含 cards 或 events
             // 处理: Mods/{modName}/dll + Mods/{modName}/ModResources/cards
             string finalCardsCheck = Path.Combine(currentDir, "ModResources", "cards");
             string finalEventsCheck = Path.Combine(currentDir, "ModResources", "events");
             if (Directory.Exists(finalCardsCheck) || Directory.Exists(finalEventsCheck))
             {
                 // 找到了，返回 ModResources 目录
                 string modResDir = Path.Combine(currentDir, "ModResources");
                 return modResDir;
             }
             
             // 如果当前目录不包含分类目录，再走 fallback 逻辑
            
            // fallback 1: 检查当前目录是否已经在 ModResources 结构中
            // 这种情况是 Mod dll 放在 ModResources/{ModName}/ 目录下
            if (currentDir.IndexOf(Path.Combine("ModResources", modName), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string combined = Path.Combine(currentDir);
                if (Directory.Exists(combined))
                    return combined;
            }
            
            // fallback 2: 使用原来的算法（兼容旧布局）
            string? fallbackParent = Directory.GetParent(currentDir)?.FullName;
            if (string.IsNullOrEmpty(fallbackParent))
                fallbackParent = currentDir;
            return Path.Combine(fallbackParent, "ModResources", modName);
        }
        
        /// <summary>
        /// 从当前目录向上查找游戏根目录（包含exe文件）
        /// </summary>
        protected virtual string? FindGameRoot(string startDir)
        {
            string? current = startDir;
            while (!string.IsNullOrEmpty(current))
            {
                // 检查是否包含游戏可执行文件
                if (File.Exists(Path.Combine(current, "Astral Party.exe")) ||
                    File.Exists(Path.Combine(current, "AstralParty.exe")))
                {
                    return current;
                }
                
                var dirInfo = Directory.GetParent(current);
                if (dirInfo == null)
                    break;
                current = dirInfo.FullName;
            }
            
            // 如果没找到exe，返回null让后续逻辑处理
            return null;
        }
        
        /// <summary>
        /// 从当前目录向上查找是否存在 ModResources/{modName}
        /// 处理从随机目录加载程序集的情况（Mod管理器）
        /// </summary>
        protected virtual bool DoesDirectoryHaveParentModResources(string startDir, string modName, out string foundPath)
        {
            foundPath = string.Empty;
            string? current = startDir;
            while (!string.IsNullOrEmpty(current))
            {
                string testPath = Path.Combine(current, "ModResources", modName);
                if (Directory.Exists(testPath))
                {
                    // 只有当这个目录内确实有cards 或者 events 子目录时才返回，
                    // 避免因为空目录存在就错误返回，而实际资源在其他位置
                    string checkCards = Path.Combine(testPath, "cards");
                    string checkEvents = Path.Combine(testPath, "events");
                    if (Directory.Exists(checkCards) || Directory.Exists(checkEvents))
                    {
                        foundPath = testPath;
                        return true;
                    }
                }
                
                var dirInfo = Directory.GetParent(current);
                if (dirInfo == null)
                    break;
                current = dirInfo.FullName;
            }
            return false;
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
