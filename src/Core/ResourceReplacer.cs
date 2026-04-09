using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;

namespace AstralPartyMod.Core
{
    public class ResourceReplacer
    {
        private readonly Dictionary<string, string> _replacements = new(StringComparer.OrdinalIgnoreCase);

        public event Action? OnResourceReplaced;
        public int Count => _replacements.Count;
        public int ReplacedCount { get; private set; }

        public void AddReplacement(string fileName, string modPath)
        {
            _replacements[fileName] = modPath;
            MelonLogger.Msg($"[ResourceReplacer] 注册替换: {fileName} -> {modPath}");
        }

        public bool TryGetReplacement(string fileName, out string? modPath)
        {
            bool found = _replacements.TryGetValue(fileName, out modPath);
            if (found)
                MelonLogger.Msg($"[ResourceReplacer] 找到替换: {fileName} -> {modPath}");
            return found;
        }

        public bool HasReplacement(string fileName) => _replacements.ContainsKey(fileName);

        public IReadOnlyDictionary<string, string> GetAllReplacements() => _replacements;

        public void Clear() => _replacements.Clear();

        public bool RemoveReplacement(string fileName) => _replacements.Remove(fileName);

        internal void NotifyResourceReplaced()
        {
            ReplacedCount++;
            OnResourceReplaced?.Invoke();
        }

        public void DebugListAllReplacements()
        {
            MelonLogger.Msg($"[ResourceReplacer] 当前共有 {_replacements.Count} 个资源替换:");
            foreach (var kvp in _replacements)
                MelonLogger.Msg($"  - {kvp.Key} -> {kvp.Value}");
        }
    }
}
