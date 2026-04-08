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
    /// <summary>
    /// Mod基类 - 所有星引擎Mod的基类
    /// </summary>
    public abstract class CoreMod : MelonMod
    {
        #region 抽象属性 - 子类必须实现
        
        /// <summary>
        /// Mod名称
        /// </summary>
        protected abstract string ModName { get; }
        
        /// <summary>
        /// Mod版本
        /// </summary>
        protected abstract string ModVersion { get; }
        
        /// <summary>
        /// Mod作者
        /// </summary>
        protected abstract string ModAuthor { get; }
        
        /// <summary>
        /// 资源目录列表（相对于Mod目录）
        /// </summary>
        protected abstract string[] ResourceDirectories { get; }
        
        #endregion
        
        #region 可选重写属性
        
        /// <summary>
        /// 热重载按键（默认F10）
        /// </summary>
        protected virtual KeyCode ReloadKey => KeyCode.F10;
        
        /// <summary>
        /// 是否启用统计功能
        /// </summary>
        protected virtual bool EnableStatistics => false;
        
        /// <summary>
        /// 统计信息显示按键（仅在EnableStatistics为true时有效）
        /// </summary>
        protected virtual KeyCode StatisticsKey => KeyCode.F12;
        
        /// <summary>
        /// 是否启用详细日志
        /// </summary>
        protected virtual bool EnableDetailedLogging => false;
        
        #endregion
        
        #region 核心组件
        
        /// <summary>
        /// 资源替换器
        /// </summary>
        public ResourceReplacer ResourceReplacer { get; private set; } = null!;
        
        /// <summary>
        /// 配置实例
        /// </summary>
        protected ModConfigBase Config { get; private set; } = new ModConfigBase();
        
        #endregion
        
        #region 统计信息
        
        private int _replacedCount = 0;
        private int _totalResources = 0;
        
        /// <summary>
        /// 获取替换次数
        /// </summary>
        public int ReplacedCount => _replacedCount;
        
        /// <summary>
        /// 获取总资源数
        /// </summary>
        public int TotalResources => _totalResources;
        
        #endregion
        
        #region 生命周期方法
        
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("========================================");
            MelonLogger.Msg($"{ModName} v{ModVersion}");
            MelonLogger.Msg($"作者: {ModAuthor}");
            MelonLogger.Msg("========================================");
            
            try
            {
                // 基类MelonMod已经初始化了HarmonyInstance
                // 加载配置
                LoadConfig();
                
                // 初始化资源替换器
                ResourceReplacer = new ResourceReplacer();
                ResourceReplacer.OnResourceReplaced += () => _replacedCount++;
                
                // 扫描资源
                ScanResources();
                
                // 应用Harmony补丁
                ApplyPatches();
                
                // 注册到全局替换器
                AssetBundlePatches.RegisterMod(this);
                
                MelonLogger.Msg($"已加载 {_totalResources} 个资源，可替换 {ResourceReplacer.Count} 个");
                MelonLogger.Msg($"按 {ReloadKey} 重新加载资源");
                if (EnableStatistics)
                {
                    MelonLogger.Msg($"按 {StatisticsKey} 显示统计信息");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Mod初始化失败: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }
        
        public override void OnUpdate()
        {
            // 热重载
            if (Input.GetKeyDown(ReloadKey))
            {
                ReloadResources();
            }
            
            // 显示统计
            if (EnableStatistics && Input.GetKeyDown(StatisticsKey))
            {
                ShowStatistics();
            }
        }
        
        public override void OnDeinitializeMelon()
        {
            HarmonyInstance?.UnpatchSelf();
            AssetBundlePatches.UnregisterMod(this);
            MelonLogger.Msg($"{ModName}已卸载，本次共替换了 {_replacedCount} 次资源");
        }
        
        #endregion
        
        #region 资源管理
        
        /// <summary>
        /// 扫描资源目录
        /// </summary>
        protected virtual void ScanResources()
        {
            string modDir = GetModDirectory();
            _totalResources = 0;
            
            foreach (var dirName in ResourceDirectories)
            {
                string fullPath = Path.Combine(modDir, dirName);
                if (Directory.Exists(fullPath))
                {
                    var files = Directory.GetFiles(fullPath, "*.bundle", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        _totalResources++;
                        
                        // 检查是否有自定义映射
                        if (Config.ResourceMappings.TryGetValue(fileName, out string? targetName) && !string.IsNullOrEmpty(targetName))
                        {
                            ResourceReplacer.AddReplacement(targetName, file);
                            if (EnableDetailedLogging)
                            {
                                MelonLogger.Msg($"[{dirName}] {fileName} -> 替换 {targetName}");
                            }
                        }
                        else
                        {
                            ResourceReplacer.AddReplacement(fileName, file);
                            if (EnableDetailedLogging)
                            {
                                MelonLogger.Msg($"[{dirName}] {fileName}");
                            }
                        }
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
        
        /// <summary>
        /// 重新加载资源
        /// </summary>
        protected virtual void ReloadResources()
        {
            MelonLogger.Msg("重新加载Mod资源...");
            ResourceReplacer.Clear();
            _replacedCount = 0;
            ScanResources();
            MelonLogger.Msg($"已重新加载 {_totalResources} 个资源");
        }
        
        /// <summary>
        /// 显示统计信息
        /// </summary>
        protected virtual void ShowStatistics()
        {
            MelonLogger.Msg("========================================");
            MelonLogger.Msg($"{ModName} 统计信息");
            MelonLogger.Msg($"总资源数: {_totalResources}");
            MelonLogger.Msg($"已替换: {_replacedCount}");
            MelonLogger.Msg($"替换率: {(TotalResources > 0 ? (ReplacedCount * 100 / TotalResources) : 0)}%");
            MelonLogger.Msg("========================================");
        }
        
        #endregion
        
        #region 配置管理
        
        /// <summary>
        /// 加载配置
        /// </summary>
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
                SaveConfig();
                MelonLogger.Msg("已创建默认配置文件");
            }
        }
        
        /// <summary>
        /// 保存配置
        /// </summary>
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
        
        #endregion
        
        #region Harmony补丁
        
        /// <summary>
        /// 应用Harmony补丁
        /// </summary>
        protected virtual void ApplyPatches()
        {
            try
            {
                // 获取AssetBundle.LoadFromFile方法
                var loadFromFileMethod = typeof(AssetBundle).GetMethod("LoadFromFile", 
                    new[] { typeof(string) });
                
                if (loadFromFileMethod != null)
                {
                    HarmonyInstance?.Patch(
                        loadFromFileMethod,
                        prefix: new HarmonyMethod(typeof(AssetBundlePatches), nameof(AssetBundlePatches.LoadFromFile_Prefix))
                    );
                    MelonLogger.Msg("已补丁 AssetBundle.LoadFromFile");
                }
                
                // 获取AssetBundle.LoadFromFileAsync方法
                var loadFromFileAsyncMethod = typeof(AssetBundle).GetMethod("LoadFromFileAsync", 
                    new[] { typeof(string) });
                
                if (loadFromFileAsyncMethod != null)
                {
                    HarmonyInstance?.Patch(
                        loadFromFileAsyncMethod,
                        prefix: new HarmonyMethod(typeof(AssetBundlePatches), nameof(AssetBundlePatches.LoadFromFileAsync_Prefix))
                    );
                    MelonLogger.Msg("已补丁 AssetBundle.LoadFromFileAsync");
                }
                
                MelonLogger.Msg("Harmony补丁应用成功");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"应用补丁失败: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }
        
        #endregion
        
        #region 工具方法
        
        /// <summary>
        /// 获取Mod目录路径
        /// </summary>
        public static string GetModDirectory()
        {
            string? assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? modDir = Path.GetDirectoryName(assemblyLocation);
            
            if (string.IsNullOrEmpty(modDir) || !Directory.Exists(modDir))
            {
                modDir = Directory.GetCurrentDirectory();
            }
            
            return modDir;
        }
        
        /// <summary>
        /// 尝试获取资源替换路径
        /// </summary>
        public bool TryGetReplacement(string fileName, out string? modPath)
        {
            return ResourceReplacer.TryGetReplacement(fileName, out modPath);
        }
        
        #endregion
    }
}
