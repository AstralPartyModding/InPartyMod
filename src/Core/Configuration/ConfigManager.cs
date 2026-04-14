using MelonLoader;
using System;
using System.IO;
using System.Text.Json;
using AstralPartyMod.Core.Configuration;

namespace AstralPartyMod.Core.Configuration
{
    /// <summary>
    /// 配置管理器 - 负责JSON配置文件的读写
    /// </summary>
    public static class ConfigManager
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private static string _configDirectory = string.Empty;

        /// <summary>
        /// 初始化配置管理器
        /// </summary>
        public static void Initialize(string configDir = "Config")
        {
            _configDirectory = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, configDir);
            
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
                MelonLogger.Msg($"[ConfigManager] 创建配置目录: {_configDirectory}");
            }
            
            MelonLogger.Msg("[ConfigManager] 初始化完成");
        }

        /// <summary>
        /// 获取配置文件完整路径
        /// </summary>
        public static string GetConfigPath(string configName)
        {
            if (!configName.EndsWith(".json"))
            {
                configName += ".json";
            }
            
            return Path.Combine(_configDirectory, configName);
        }

        /// <summary>
        /// 加载配置，如果文件不存在则创建默认配置
        /// </summary>
        public static T LoadConfig<T>(string configName, T? defaultConfig = default) where T : new()
        {
            var path = GetConfigPath(configName);

            if (!File.Exists(path))
            {
                if (defaultConfig == null)
                {
                    defaultConfig = new T();
                }
                
                SaveConfig(configName, defaultConfig);
                MelonLogger.Msg($"[ConfigManager] 创建默认配置: {configName}");
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                MelonLogger.Msg($"[ConfigManager] 加载配置成功: {configName}");
                return config ?? new T();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ConfigManager] 加载配置失败 {configName}: {ex.Message}");
                MelonLogger.Warning($"[ConfigManager] 将使用默认配置");
                return defaultConfig ?? new T();
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public static bool SaveConfig<T>(string configName, T config)
        {
            try
            {
                var path = GetConfigPath(configName);
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ConfigManager] 保存配置失败 {configName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static T ReloadConfig<T>(string configName, T? defaultConfig = default) where T : new()
        {
            return LoadConfig<T>(configName, defaultConfig);
        }

        /// <summary>
        /// 检查配置文件是否存在
        /// </summary>
        public static bool ConfigExists(string configName)
        {
            return File.Exists(GetConfigPath(configName));
        }

        /// <summary>
        /// 删除配置文件
        /// </summary>
        public static bool DeleteConfig(string configName)
        {
            try
            {
                var path = GetConfigPath(configName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ConfigManager] 删除配置失败 {configName}: {ex.Message}");
                return false;
            }
        }
    }
}
