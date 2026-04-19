using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AstralPartyMod.Core.Configuration
{
    /// <summary>
    /// 配置验证结果
    /// </summary>
    public class ConfigValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误消息列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 警告消息列表
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// 添加错误
        /// </summary>
        public void AddError(string message)
        {
            IsValid = false;
            Errors.Add(message);
        }

        /// <summary>
        /// 添加警告
        /// </summary>
        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }

        /// <summary>
        /// 获取摘要
        /// </summary>
        public string GetSummary()
        {
            var summary = IsValid ? "配置有效" : "配置无效";
            if (Errors.Count > 0)
            {
                summary += $"\n错误 ({Errors.Count}):\n";
                foreach (var error in Errors)
                {
                    summary += $"  - {error}\n";
                }
            }
            if (Warnings.Count > 0)
            {
                summary += $"\n警告 ({Warnings.Count}):\n";
                foreach (var warning in Warnings)
                {
                    summary += $"  - {warning}\n";
                }
            }
            return summary.TrimEnd();
        }
    }

    /// <summary>
    /// 配置验证选项
    /// </summary>
    public class ConfigValidationOptions
    {
        /// <summary>
        /// 是否严格模式（严格模式下警告也算错误）
        /// </summary>
        public bool StrictMode { get; set; } = false;

        /// <summary>
        /// 是否验证资源映射路径
        /// </summary>
        public bool ValidateResourcePaths { get; set; } = true;

        /// <summary>
        /// 是否检查重复的分类名称
        /// </summary>
        public bool CheckDuplicateCategories { get; set; } = true;

        /// <summary>
        /// 是否检查空的分类
        /// </summary>
        public bool CheckEmptyCategories { get; set; } = true;

        /// <summary>
        /// 最大允许的分类数量
        /// </summary>
        public int MaxCategories { get; set; } = 100;

        /// <summary>
        /// 最大允许的单个分类中的资源数量
        /// </summary>
        public int MaxResourcesPerCategory { get; set; } = 10000;
    }

    /// <summary>
    /// 配置验证器 - 启动时校验config.json格式
    /// </summary>
    public static class ConfigValidator
    {
        /// <summary>
        /// 验证配置
        /// </summary>
        public static ConfigValidationResult Validate(ModConfigBase config, ConfigValidationOptions? options = null)
        {
            options ??= new ConfigValidationOptions();
            var result = new ConfigValidationResult { IsValid = true };

            // 验证基本属性
            ValidateBasicProperties(config, result, options);

            // 验证分类
            ValidateCategories(config, result, options);

            // 验证资源映射
            ValidateResourceMappings(config, result, options);

            // 严格模式下，警告也算错误
            if (options.StrictMode && result.Warnings.Count > 0)
            {
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// 验证基本属性
        /// </summary>
        private static void ValidateBasicProperties(ModConfigBase config, ConfigValidationResult result, ConfigValidationOptions options)
        {
            // EnableDetailedLogging 应该有合理的默认值
            // 当前没有明确的约束条件，仅作占位
        }

        /// <summary>
        /// 验证分类
        /// </summary>
        private static void ValidateCategories(ModConfigBase config, ConfigValidationResult result, ConfigValidationOptions options)
        {
            if (config.Categories == null)
            {
                result.AddError("分类字典为空");
                return;
            }

            // 检查分类数量
            if (config.Categories.Count > options.MaxCategories)
            {
                result.AddError($"分类数量超过限制: {config.Categories.Count} > {options.MaxCategories}");
            }

            // 检查空分类名称
            var emptyCategoryNames = new List<string>();
            var duplicateCategoryNames = new HashSet<string>();

            foreach (var kvp in config.Categories)
            {
                string categoryName = kvp.Key;

                // 检查空名称
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    emptyCategoryNames.Add(categoryName);
                }

                // 检查重复（通过比较字符串）
                if (!duplicateCategoryNames.Contains(categoryName))
                {
                    duplicateCategoryNames.Add(categoryName);
                }
                else
                {
                    result.AddWarning($"发现重复的分类名称: {categoryName}");
                }

                // 验证分类内容
                ValidateCategory(categoryName, kvp.Value, result, options);
            }

            if (emptyCategoryNames.Count > 0)
            {
                result.AddError($"发现 {emptyCategoryNames.Count} 个空分类名称");
            }

            // 检查空分类
            if (options.CheckEmptyCategories)
            {
                var emptyCategories = new List<string>();
                foreach (var kvp in config.Categories)
                {
                    if (kvp.Value == null)
                    {
                        emptyCategories.Add(kvp.Key);
                    }
                }

                if (emptyCategories.Count > 0)
                {
                    result.AddWarning($"发现 {emptyCategories.Count} 个空分类: {string.Join(", ", emptyCategories)}");
                }
            }
        }

        /// <summary>
        /// 验证单个分类
        /// </summary>
        private static void ValidateCategory(string categoryName, ResourceCategory? category, ConfigValidationResult result, ConfigValidationOptions options)
        {
            if (category == null)
            {
                return;
            }

            // 验证资源映射数量
            if (category.ResourceMappings != null && category.ResourceMappings.Count > options.MaxResourcesPerCategory)
            {
                result.AddError($"分类 '{categoryName}' 的资源映射数量超过限制: {category.ResourceMappings.Count} > {options.MaxResourcesPerCategory}");
            }

            // 验证描述长度
            if (!string.IsNullOrEmpty(category.Description) && category.Description.Length > 500)
            {
                result.AddWarning($"分类 '{categoryName}' 的描述过长: {category.Description.Length} > 500");
            }
        }

        /// <summary>
        /// 验证资源映射
        /// </summary>
        private static void ValidateResourceMappings(ModConfigBase config, ConfigValidationResult result, ConfigValidationOptions options)
        {
            if (config.ResourceMappings == null)
            {
                return;
            }

            // 检查空键
            var emptyKeys = new List<string>();
            foreach (var kvp in config.ResourceMappings)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    emptyKeys.Add(kvp.Key ?? "(null)");
                }
            }

            if (emptyKeys.Count > 0)
            {
                result.AddError($"发现 {emptyKeys.Count} 个空资源映射键");
            }

            // 检查空值
            var emptyValues = new List<string>();
            foreach (var kvp in config.ResourceMappings)
            {
                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    emptyValues.Add(kvp.Key);
                }
            }

            if (emptyValues.Count > 0)
            {
                result.AddWarning($"发现 {emptyValues.Count} 个空资源映射值: {string.Join(", ", emptyValues)}");
            }

            // 检查资源路径
            if (options.ValidateResourcePaths)
            {
                var invalidPaths = new List<string>();
                foreach (var kvp in config.ResourceMappings)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Value) && !System.IO.File.Exists(kvp.Value))
                    {
                        invalidPaths.Add(kvp.Key);
                    }
                }

                if (invalidPaths.Count > 0)
                {
                    result.AddWarning($"发现 {invalidPaths.Count} 个不存在的资源路径: {string.Join(", ", invalidPaths)}");
                }
            }
        }

        /// <summary>
        /// 验证JSON字符串
        /// </summary>
        public static ConfigValidationResult ValidateJson(string json, ConfigValidationOptions? options = null)
        {
            var result = new ConfigValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(json))
            {
                result.AddError("JSON字符串为空");
                return result;
            }

            try
            {
                var config = JsonSerializer.Deserialize<ModConfigBase>(json);
                if (config == null)
                {
                    result.AddError("JSON反序列化失败，返回空对象");
                    return result;
                }

                // 合并验证结果
                var configResult = Validate(config, options);
                result.IsValid = configResult.IsValid;
                result.Errors.AddRange(configResult.Errors);
                result.Warnings.AddRange(configResult.Warnings);

                return result;
            }
            catch (JsonException ex)
            {
                result.AddError($"JSON解析错误: {ex.Message}");
                return result;
            }
            catch (Exception ex)
            {
                result.AddError($"未知错误: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static ModConfigBase CreateDefault()
        {
            return new ModConfigBase
            {
                EnableDetailedLogging = false,
                ResourceMappings = new Dictionary<string, string>(),
                Categories = new Dictionary<string, ResourceCategory>()
            };
        }
    }
}
