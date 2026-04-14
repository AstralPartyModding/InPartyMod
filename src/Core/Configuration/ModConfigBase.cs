using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AstralPartyMod.Core.Configuration
{
    [Serializable]
    public class ResourceCategory
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("resourceMappings")]
        public Dictionary<string, string> ResourceMappings { get; set; } = new Dictionary<string, string>();

        public ResourceCategory() { }

        public ResourceCategory(string path, string description, bool enabled = true)
        {
            Path = path;
            Description = description;
            Enabled = enabled;
        }
    }

    [Serializable]
    public class ModConfigBase
    {
        [JsonPropertyName("enableDetailedLogging")]
        public bool EnableDetailedLogging { get; set; } = false;

        [JsonPropertyName("resourceMappings")]
        public Dictionary<string, string> ResourceMappings { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("categories")]
        public Dictionary<string, ResourceCategory> Categories { get; set; } = new Dictionary<string, ResourceCategory>();

        public T GetValue<T>(string key, T defaultValue)
        {
            if (ResourceMappings.TryGetValue(key, out string? value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public void SetValue(string key, string value)
        {
            ResourceMappings[key] = value;
        }

        public ResourceCategory? GetCategory(string categoryName)
        {
            if (Categories.TryGetValue(categoryName, out var category))
            {
                return category;
            }
            return null;
        }

        public bool SetCategoryEnabled(string categoryName, bool enabled)
        {
            if (Categories.TryGetValue(categoryName, out var category))
            {
                category.Enabled = enabled;
                return true;
            }
            return false;
        }

        public bool IsCategoryEnabled(string categoryName)
        {
            if (Categories.TryGetValue(categoryName, out var category))
            {
                return category.Enabled;
            }
            return false;
        }

        public void AddOrUpdateCategory(string categoryName, ResourceCategory category)
        {
            Categories[categoryName] = category;
        }

        public void SetCategory(string categoryName, ResourceCategory category)
        {
            AddOrUpdateCategory(categoryName, category);
        }

        public void SetCategory(string categoryName, string path, string description, bool enabled = true)
        {
            AddOrUpdateCategory(categoryName, new ResourceCategory(path, description, enabled));
        }

        public void InitializeDefaultCategories()
        {
        }
    }
}
