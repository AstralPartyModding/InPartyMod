using MelonLoader;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AstralPartyMod.Core
{
    public static class AssetBundlePatches
    {
        private static readonly List<CoreMod> _registeredMods = new();
        public static bool DebugMode { get; set; } = false;

        public static void RegisterMod(CoreMod mod)
        {
            if (!_registeredMods.Contains(mod))
                _registeredMods.Add(mod);
        }

        public static void UnregisterMod(CoreMod mod)
        {
            _registeredMods.Remove(mod);
        }

        private static bool TryGetModReplacement(string fileName, out string? modPath, out CoreMod? sourceMod)
        {
            modPath = null;
            sourceMod = null;

            foreach (var mod in _registeredMods)
            {
                if (mod.TryGetReplacement(fileName, out modPath) && !string.IsNullOrEmpty(modPath))
                {
                    sourceMod = mod;
                    return true;
                }
            }
            return false;
        }

        public static bool LoadFromFile_Prefix(string path, ref AssetBundle __result)
        {
            try
            {
                string fileName = Path.GetFileName(path);

                if (TryGetModReplacement(fileName, out string? modPath, out CoreMod? sourceMod) &&
                    !string.IsNullOrEmpty(modPath) && sourceMod != null)
                {
                    if (File.Exists(modPath))
                    {
                        __result = AssetBundle.LoadFromFile(modPath);
                        sourceMod.ResourceReplacer.NotifyResourceReplaced();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[AssetBundlePatches] 资源替换失败: {ex.Message}");
            }
            return true;
        }

        public static bool LoadFromFileAsync_Prefix(string path, ref AssetBundleCreateRequest __result)
        {
            try
            {
                string fileName = Path.GetFileName(path);

                if (TryGetModReplacement(fileName, out string? modPath, out CoreMod? sourceMod) &&
                    !string.IsNullOrEmpty(modPath) && sourceMod != null)
                {
                    if (File.Exists(modPath))
                    {
                        __result = AssetBundle.LoadFromFileAsync(modPath);
                        sourceMod.ResourceReplacer.NotifyResourceReplaced();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[AssetBundlePatches] 异步资源替换失败: {ex.Message}");
            }
            return true;
        }

        public static void TryPatchAddressables(HarmonyLib.Harmony harmony)
        {
            try
            {
                var addressablesAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a?.GetName().Name?.Contains("Addressables") == true);

                if (addressablesAssembly == null)
                    return;

                var addressablesType = addressablesAssembly.GetTypes()
                    .FirstOrDefault(t => t.Name == "Addressables");

                if (addressablesType != null)
                {
                    MelonLogger.Msg("[Addressables] 检测到 Addressables 系统");
                }
            }
            catch { }
        }

        public static void TryPatchResources(HarmonyLib.Harmony harmony)
        {
            try
            {
                var resourcesType = typeof(Resources);
                var loadGenericMethod = resourcesType.GetMethods()
                    .FirstOrDefault(m => m.Name == "Load" && m.IsGenericMethod && m.GetParameters().Length == 1);

                if (loadGenericMethod != null)
                    MelonLogger.Msg("[Resources] 检测到 Resources.Load<T>");
            }
            catch { }
        }

        public static void DebugListAllReplacements()
        {
            MelonLogger.Msg("========== 已注册的资源替换列表 ==========");
            foreach (var mod in _registeredMods)
            {
                MelonLogger.Msg($"Mod: {mod.Info.Name}");
                var replacements = mod.ResourceReplacer.GetAllReplacements();
                foreach (var kvp in replacements)
                    MelonLogger.Msg($"  - {kvp.Key} -> {kvp.Value}");
            }
            MelonLogger.Msg("==========================================");
        }

        public static (int modCount, int totalReplacements, int totalReplaced) GetStatistics()
        {
            int modCount = _registeredMods.Count;
            int totalReplacements = _registeredMods.Sum(m => m.ResourceReplacer.GetAllReplacements().Count);
            int totalReplaced = _registeredMods.Sum(m => m.ResourceReplacer.ReplacedCount);
            return (modCount, totalReplacements, totalReplaced);
        }
    }
}
