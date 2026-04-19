using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AstralPartyMod.Core.Assets
{
    /// <summary>
    /// 资源冲突信息
    /// </summary>
    public class ResourceConflict
    {
        /// <summary>
        /// 冲突的资源文件名
        /// </summary>
        public string ResourceName { get; set; } = string.Empty;

        /// <summary>
        /// 竞争的Mod列表
        /// </summary>
        public List<ConflictModInfo> CompetingMods { get; set; } = new List<ConflictModInfo>();

        /// <summary>
        /// 最终被选中的Mod
        /// </summary>
        public ConflictModInfo? Winner { get; set; }

        /// <summary>
        /// 冲突数量
        /// </summary>
        public int ConflictCount => CompetingMods.Count;
    }

    /// <summary>
    /// 冲突的Mod信息
    /// </summary>
    public class ConflictModInfo
    {
        /// <summary>
        /// Mod名称
        /// </summary>
        public string ModName { get; set; } = string.Empty;

        /// <summary>
        /// 资源路径
        /// </summary>
        public string ResourcePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 加载顺序（数字越小优先级越高）
        /// </summary>
        public int LoadOrder { get; set; }
    }

    /// <summary>
    /// 资源冲突检测器 - 检测多个Mod替换同一资源时的冲突
    /// </summary>
    public static class ResourceConflictDetector
    {
        /// <summary>
        /// 加载优先级（数字越小优先级越高）
        /// </summary>
        private static readonly Dictionary<string, int> _modLoadPriority = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static int _nextPriority = 0;

        /// <summary>
        /// 检测到的所有冲突
        /// </summary>
        private static readonly List<ResourceConflict> _detectedConflicts = new List<ResourceConflict>();

        /// <summary>
        /// 注册Mod的资源替换冲突检测
        /// </summary>
        /// <param name="modName">Mod名称</param>
        /// <param name="replacements">资源替换映射表</param>
        /// <returns>冲突列表</returns>
        public static List<ResourceConflict> RegisterModResources(string modName, IReadOnlyDictionary<string, string> replacements)
        {
            var newConflicts = new List<ResourceConflict>();

            foreach (var kvp in replacements)
            {
                string resourceName = kvp.Key;
                string resourcePath = kvp.Value;

                // 检查是否已存在该资源的冲突记录
                var existingConflict = _detectedConflicts.FirstOrDefault(c =>
                    c.ResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase));

                if (existingConflict != null)
                {
                    // 已存在冲突，添加此Mod到竞争列表
                    if (!existingConflict.CompetingMods.Any(m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var modInfo = new ConflictModInfo
                        {
                            ModName = modName,
                            ResourcePath = resourcePath,
                            FileSize = GetFileSize(resourcePath),
                            LoadOrder = GetOrCreateModPriority(modName)
                        };
                        existingConflict.CompetingMods.Add(modInfo);
                        existingConflict.CompetingMods = existingConflict.CompetingMods
                            .OrderBy(m => m.LoadOrder)
                            .ToList();
                        LogConflict(existingConflict);
                    }
                }
                else
                {
                    // 检查是否与其他Mod的资源冲突
                    // 这里我们检查是否有其他Mod已经注册了相同的资源名
                    var conflictingMods = FindConflictingMods(resourceName, modName);

                    if (conflictingMods.Count > 0)
                    {
                        var conflict = new ResourceConflict
                        {
                            ResourceName = resourceName,
                            CompetingMods = conflictingMods
                        };

                        // 添加当前Mod
                        var currentModInfo = new ConflictModInfo
                        {
                            ModName = modName,
                            ResourcePath = resourcePath,
                            FileSize = GetFileSize(resourcePath),
                            LoadOrder = GetOrCreateModPriority(modName)
                        };
                        conflict.CompetingMods.Add(currentModInfo);

                        // 按加载顺序排序
                        conflict.CompetingMods = conflict.CompetingMods
                            .OrderBy(m => m.LoadOrder)
                            .ToList();

                        // 确定胜者（加载顺序最高的Mod获胜）
                        conflict.Winner = conflict.CompetingMods.First();

                        _detectedConflicts.Add(conflict);
                        newConflicts.Add(conflict);
                        LogConflict(conflict);
                    }
                }
            }

            return newConflicts;
        }

        /// <summary>
        /// 获取或创建Mod的优先级
        /// </summary>
        private static int GetOrCreateModPriority(string modName)
        {
            if (!_modLoadPriority.TryGetValue(modName, out int priority))
            {
                priority = _nextPriority++;
                _modLoadPriority[modName] = priority;
            }
            return priority;
        }

        /// <summary>
        /// 查找冲突的其他Mod
        /// </summary>
        private static List<ConflictModInfo> FindConflictingMods(string resourceName, string excludeModName)
        {
            var conflictingMods = new List<ConflictModInfo>();

            // 遍历已检测到的冲突
            foreach (var conflict in _detectedConflicts)
            {
                if (conflict.ResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var mod in conflict.CompetingMods)
                    {
                        if (!mod.ModName.Equals(excludeModName, StringComparison.OrdinalIgnoreCase))
                        {
                            conflictingMods.Add(mod);
                        }
                    }
                }
            }

            return conflictingMods;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        private static long GetFileSize(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    return new System.IO.FileInfo(path).Length;
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// 记录冲突信息
        /// </summary>
        private static void LogConflict(ResourceConflict conflict)
        {
            MelonLogger.Warning($"[ResourceConflict] 检测到资源冲突: {conflict.ResourceName}");
            foreach (var mod in conflict.CompetingMods)
            {
                MelonLogger.Warning($"  - {mod.ModName}: {mod.ResourcePath} (大小: {mod.FileSize} bytes, 优先级: {mod.LoadOrder})");
            }
            if (conflict.Winner != null)
            {
                MelonLogger.Msg($"[ResourceConflict] 胜出者: {conflict.Winner.ModName}");
            }
        }

        /// <summary>
        /// 获取所有检测到的冲突
        /// </summary>
        public static IReadOnlyList<ResourceConflict> GetAllConflicts() => _detectedConflicts.AsReadOnly();

        /// <summary>
        /// 获取冲突数量
        /// </summary>
        public static int ConflictCount => _detectedConflicts.Count;

        /// <summary>
        /// 检查是否存在冲突
        /// </summary>
        public static bool HasConflicts => _detectedConflicts.Count > 0;

        /// <summary>
        /// 获取特定资源的冲突信息
        /// </summary>
        public static ResourceConflict? GetConflict(string resourceName)
        {
            return _detectedConflicts.FirstOrDefault(c =>
                c.ResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 获取胜出的Mod信息
        /// </summary>
        public static ConflictModInfo? GetWinner(string resourceName)
        {
            var conflict = GetConflict(resourceName);
            return conflict?.Winner;
        }

        /// <summary>
        /// 清除所有冲突记录
        /// </summary>
        public static void Clear()
        {
            _detectedConflicts.Clear();
            _modLoadPriority.Clear();
            _nextPriority = 0;
            MelonLogger.Msg("[ResourceConflictDetector] 已清除所有冲突记录");
        }

        /// <summary>
        /// 生成冲突报告
        /// </summary>
        public static string GenerateConflictReport()
        {
            if (_detectedConflicts.Count == 0)
            {
                return "未检测到资源冲突。";
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== 资源冲突报告 ===");
            sb.AppendLine($"总计冲突数: {_detectedConflicts.Count}");
            sb.AppendLine();

            foreach (var conflict in _detectedConflicts)
            {
                sb.AppendLine($"资源: {conflict.ResourceName}");
                sb.AppendLine($"竞争Mod数: {conflict.CompetingMods.Count}");
                foreach (var mod in conflict.CompetingMods)
                {
                    sb.AppendLine($"  - {mod.ModName} (优先级: {mod.LoadOrder}, 大小: {mod.FileSize} bytes)");
                }
                if (conflict.Winner != null)
                {
                    sb.AppendLine($"胜出: {conflict.Winner.ModName}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 打印冲突报告到日志
        /// </summary>
        public static void PrintConflictReport()
        {
            if (_detectedConflicts.Count == 0)
            {
                MelonLogger.Msg("[ResourceConflictDetector] 未检测到资源冲突。");
                return;
            }

            MelonLogger.Warning("=== 资源冲突报告 ===");
            MelonLogger.Warning($"总计冲突数: {_detectedConflicts.Count}");

            foreach (var conflict in _detectedConflicts)
            {
                MelonLogger.Warning($"资源: {conflict.ResourceName}");
                foreach (var mod in conflict.CompetingMods)
                {
                    MelonLogger.Warning($"  - {mod.ModName} (优先级: {mod.LoadOrder})");
                }
                if (conflict.Winner != null)
                {
                    MelonLogger.Warning($"  胜出: {conflict.Winner.ModName}");
                }
            }
        }
    }
}
