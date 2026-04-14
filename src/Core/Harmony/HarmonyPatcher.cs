using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AstralPartyMod.Core.Harmony
{
    /// <summary>
    /// 统一管理所有Harmony补丁
    /// </summary>
    public static class HarmonyPatcher
    {
        private static HarmonyLib.Harmony? _harmony;
        private static bool _isPatched = false;
        private static readonly HashSet<Type> _patchedTypes = new HashSet<Type>();

        /// <summary>
        /// 获取Harmony实例
        /// </summary>
        public static HarmonyLib.Harmony? HarmonyInstance => _harmony;

        /// <summary>
        /// 是否已应用补丁
        /// </summary>
        public static bool IsPatched => _isPatched;

        /// <summary>
        /// 初始化Harmony实例
        /// </summary>
        public static void Initialize(string harmonyId)
        {
            if (_harmony != null)
            {
                MelonLogger.Warning("[HarmonyPatcher] Harmony已经初始化过了");
                return;
            }

            _harmony = new HarmonyLib.Harmony(harmonyId);
            MelonLogger.Msg($"[HarmonyPatcher] 初始化完成，ID: {harmonyId}");
        }

        /// <summary>
        /// 应用所有补丁在当前程序集
        /// </summary>
        public static bool PatchAll()
        {
            if (_harmony == null)
            {
                MelonLogger.Error("[HarmonyPatcher] Harmony未初始化，无法应用补丁");
                return false;
            }

            if (_isPatched)
            {
                MelonLogger.Warning("[HarmonyPatcher] 补丁已经应用过了");
                return true;
            }

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                _harmony.PatchAll(assembly);
                
                // 记录已打补丁的类型
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<HarmonyPatch>() != null)
                    {
                        _patchedTypes.Add(type);
                    }
                }

                _isPatched = true;
                MelonLogger.Msg($"[HarmonyPatcher] 成功应用 {_patchedTypes.Count} 个补丁");
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[HarmonyPatcher] 应用补丁失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 应用单个类型的补丁
        /// </summary>
        public static bool Patch(Type patchType)
        {
            if (_harmony == null)
            {
                MelonLogger.Error("[HarmonyPatcher] Harmony未初始化，无法应用补丁");
                return false;
            }

            if (_patchedTypes.Contains(patchType))
            {
                MelonLogger.Warning($"[HarmonyPatcher] 补丁 {patchType.Name} 已经应用过了");
                return true;
            }

            try
            {
                _harmony.PatchAll(patchType.Assembly);
                _patchedTypes.Add(patchType);
                MelonLogger.Msg($"[HarmonyPatcher] 应用补丁: {patchType.Name}");
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[HarmonyPatcher] 应用补丁 {patchType.Name} 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 卸载所有补丁
        /// </summary>
        public static void UnpatchAll()
        {
            if (!_isPatched || _harmony == null)
            {
                return;
            }

            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
            _patchedTypes.Clear();
            _isPatched = false;
            MelonLogger.Msg("[HarmonyPatcher] 所有补丁已卸载");
        }

        /// <summary>
        /// 获取已打补丁的数量
        /// </summary>
        public static int GetPatchedCount() => _patchedTypes.Count;
    }
}
