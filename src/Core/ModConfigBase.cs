using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AstralPartyMod.Core
{
    /// <summary>
    /// Mod配置基类
    /// </summary>
    [Serializable]
    public class ModConfigBase
    {
        /// <summary>
        /// 是否启用详细日志输出
        /// </summary>
        [JsonPropertyName("enableDetailedLogging")]
        public bool EnableDetailedLogging { get; set; } = false;
        
        /// <summary>
        /// 资源映射表：Mod文件名 -> 目标替换文件名
        /// 用于当Mod文件名与游戏内文件名不一致时
        /// </summary>
        [JsonPropertyName("resourceMappings")]
        public Dictionary<string, string> ResourceMappings { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// 获取配置值（带默认值）
        /// </summary>
        public T GetValue<T>(string key, T defaultValue)
        {
            if (ResourceMappings.TryGetValue(key, out string? value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 设置配置值
        /// </summary>
        public void SetValue(string key, string value)
        {
            ResourceMappings[key] = value;
        }
    }
}
