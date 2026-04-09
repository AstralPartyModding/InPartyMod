using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AstralPartyMod.Core
{
    /// <summary>
    /// 资源分类配置
    /// 用于定义Mod资源的分类，每个分类可以独立启用/禁用
    /// </summary>
    [Serializable]
    public class ResourceCategory
    {
        /// <summary>
        /// 分类是否启用
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 分类目录路径（相对于Mod目录）
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 分类描述
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 分类特定的资源映射表
        /// 用于该分类下的资源文件名到游戏内资源名的映射
        /// </summary>
        [JsonPropertyName("resourceMappings")]
        public Dictionary<string, string> ResourceMappings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ResourceCategory() { }

        /// <summary>
        /// 创建分类配置
        /// </summary>
        /// <param name="path">分类目录路径</param>
        /// <param name="description">分类描述</param>
        /// <param name="enabled">是否启用</param>
        public ResourceCategory(string path, string description, bool enabled = true)
        {
            Path = path;
            Description = description;
            Enabled = enabled;
        }
    }

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
        /// 资源分类配置
        /// 键为分类名称，值为分类配置
        /// 支持按分类管理资源，每个分类可以独立启用/禁用
        /// </summary>
        [JsonPropertyName("categories")]
        public Dictionary<string, ResourceCategory> Categories { get; set; } = new Dictionary<string, ResourceCategory>();

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

        /// <summary>
        /// 获取分类配置
        /// </summary>
        /// <param name="categoryName">分类名称</param>
        /// <returns>分类配置，如果不存在则返回null</returns>
        public ResourceCategory? GetCategory(string categoryName)
        {
            if (Categories.TryGetValue(categoryName, out var category))
            {
                return category;
            }
            return null;
        }

        /// <summary>
        /// 设置分类启用状态
        /// </summary>
        /// <param name="categoryName">分类名称</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>是否成功设置（分类存在时返回true）</returns>
        public bool SetCategoryEnabled(string categoryName, bool enabled)
        {
            if (Categories.TryGetValue(categoryName, out var category))
            {
                category.Enabled = enabled;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查分类是否启用
        /// </summary>
        /// <param name="categoryName">分类名称</param>
        /// <returns>如果分类不存在或禁用返回false，否则返回true</returns>
        public bool IsCategoryEnabled(string categoryName)
        {
            if (Categories.TryGetValue(categoryName, out var category))
            {
                return category.Enabled;
            }
            return false;
        }

        /// <summary>
        /// 添加或更新分类配置
        /// </summary>
        /// <param name="categoryName">分类名称</param>
        /// <param name="category">分类配置</param>
        public void SetCategory(string categoryName, ResourceCategory category)
        {
            Categories[categoryName] = category;
        }

        /// <summary>
        /// 移除分类配置
        /// </summary>
        /// <param name="categoryName">分类名称</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveCategory(string categoryName)
        {
            return Categories.Remove(categoryName);
        }

        /// <summary>
        /// 获取所有启用的分类名称
        /// </summary>
        /// <returns>启用的分类名称列表</returns>
        public List<string> GetEnabledCategoryNames()
        {
            var enabled = new List<string>();
            foreach (var kvp in Categories)
            {
                if (kvp.Value.Enabled)
                {
                    enabled.Add(kvp.Key);
                }
            }
            return enabled;
        }

        /// <summary>
        /// 获取所有分类名称
        /// </summary>
        /// <returns>所有分类名称列表</returns>
        public List<string> GetAllCategoryNames()
        {
            return new List<string>(Categories.Keys);
        }
    }
}
