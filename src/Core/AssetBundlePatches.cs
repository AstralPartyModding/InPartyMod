using MelonLoader;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AstralPartyMod.Core
{
    /// <summary>
    /// AssetBundle加载补丁 - 拦截并替换资源加载
    /// </summary>
    public static class AssetBundlePatches
    {
        // 已注册的Mod列表
        private static readonly List<CoreMod> _registeredMods = new();
        
        /// <summary>
        /// 注册Mod到替换系统
        /// </summary>
        public static void RegisterMod(CoreMod mod)
        {
            if (!_registeredMods.Contains(mod))
            {
                _registeredMods.Add(mod);
            }
        }
        
        /// <summary>
        /// 从替换系统注销Mod
        /// </summary>
        public static void UnregisterMod(CoreMod mod)
        {
            _registeredMods.Remove(mod);
        }
        
        /// <summary>
        /// 拦截AssetBundle.LoadFromFile
        /// </summary>
        public static bool LoadFromFile_Prefix(string path, ref AssetBundle __result)
        {
            try
            {
                string fileName = Path.GetFileName(path);
                
                // 遍历所有已注册的Mod，寻找替换
                foreach (var mod in _registeredMods)
                {
                    if (mod.TryGetReplacement(fileName, out string? modPath) && !string.IsNullOrEmpty(modPath))
                    {
                        if (File.Exists(modPath))
                        {
                            __result = AssetBundle.LoadFromFile(modPath);
                            mod.ResourceReplacer.NotifyResourceReplaced();
                            return false; // 跳过原始方法
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[AssetBundlePatches] 资源替换失败: {ex.Message}");
            }
            
            return true; // 继续执行原始方法
        }
        
        /// <summary>
        /// 拦截AssetBundle.LoadFromFileAsync
        /// </summary>
        public static bool LoadFromFileAsync_Prefix(string path, ref AssetBundleCreateRequest __result)
        {
            try
            {
                string fileName = Path.GetFileName(path);
                
                // 遍历所有已注册的Mod，寻找替换
                foreach (var mod in _registeredMods)
                {
                    if (mod.TryGetReplacement(fileName, out string? modPath) && !string.IsNullOrEmpty(modPath))
                    {
                        if (File.Exists(modPath))
                        {
                            __result = AssetBundle.LoadFromFileAsync(modPath);
                            mod.ResourceReplacer.NotifyResourceReplaced();
                            return false; // 跳过原始方法
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[AssetBundlePatches] 异步资源替换失败: {ex.Message}");
            }
            
            return true; // 继续执行原始方法
        }
    }
}
