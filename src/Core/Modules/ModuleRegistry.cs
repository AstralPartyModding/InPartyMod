using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AstralPartyMod.Core.Modules
{
    /// <summary>
    /// 模块注册中心 - 管理所有功能模块的注册和生命周期
    /// </summary>
    public static class ModuleRegistry
    {
        private static readonly Dictionary<string, IModModule> _modules = new Dictionary<string, IModModule>();
        private static readonly List<string> _loadOrder = new List<string>();

        /// <summary>
        /// 获取所有已注册模块
        /// </summary>
        public static IReadOnlyDictionary<string, IModModule> Modules => _modules;

        /// <summary>
        /// 获取已注册模块数量
        /// </summary>
        public static int ModuleCount => _modules.Count;

        /// <summary>
        /// 注册模块
        /// </summary>
        public static bool RegisterModule(IModModule module)
        {
            if (module == null)
            {
                MelonLogger.Error("[ModuleRegistry] 无法注册空模块");
                return false;
            }

            if (_modules.ContainsKey(module.Name))
            {
                MelonLogger.Warning($"[ModuleRegistry] 模块 {module.Name} 已经注册过了");
                return false;
            }

            _modules[module.Name] = module;
            MelonLogger.Msg($"[ModuleRegistry] 注册模块: {module.Name} v{module.Version}");
            return true;
        }

        /// <summary>
        /// 注销模块
        /// </summary>
        public static bool UnregisterModule(string moduleName)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
            {
                MelonLogger.Warning($"[ModuleRegistry] 模块 {moduleName} 未找到");
                return false;
            }

            if (module.IsEnabled)
            {
                module.Disable();
                module.Shutdown();
            }

            _modules.Remove(moduleName);
            _loadOrder.Remove(moduleName);
            MelonLogger.Msg($"[ModuleRegistry] 注销模块: {moduleName}");
            return true;
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        public static IModModule? GetModule(string moduleName)
        {
            _modules.TryGetValue(moduleName, out var module);
            return module;
        }

        /// <summary>
        /// 检查模块是否已注册
        /// </summary>
        public static bool IsRegistered(string moduleName) => _modules.ContainsKey(moduleName);

        /// <summary>
        /// 初始化所有模块（按依赖顺序）
        /// </summary>
        public static bool InitializeAll()
        {
            MelonLogger.Msg($"[ModuleRegistry] 开始初始化 {_modules.Count} 个模块...");
            
            // 拓扑排序处理依赖
            if (!ResolveLoadOrder())
            {
                MelonLogger.Error("[ModuleRegistry] 依赖解析失败，可能存在循环依赖");
                return false;
            }

            bool allSuccess = true;
            foreach (var moduleName in _loadOrder)
            {
                var module = _modules[moduleName];
                MelonLogger.Msg($"[ModuleRegistry] 初始化模块: {moduleName}");
                
                try
                {
                    if (!module.Initialize())
                    {
                        MelonLogger.Error($"[ModuleRegistry] 模块 {moduleName} 初始化失败");
                        allSuccess = false;
                        continue;
                    }

                    if (!module.Enable())
                    {
                        MelonLogger.Warning($"[ModuleRegistry] 模块 {moduleName} 启用失败");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[ModuleRegistry] 模块 {moduleName} 初始化异常: {ex.Message}");
                    allSuccess = false;
                }
            }

            MelonLogger.Msg($"[ModuleRegistry] 初始化完成，成功: {_loadOrder.Count(m => GetModule(m)?.IsEnabled == true)} / {_modules.Count}");
            return allSuccess;
        }

        /// <summary>
        /// 关闭所有模块
        /// </summary>
        public static void ShutdownAll()
        {
            MelonLogger.Msg("[ModuleRegistry] 关闭所有模块...");
            
            // 逆序关闭
            for (int i = _loadOrder.Count - 1; i >= 0; i--)
            {
                var moduleName = _loadOrder[i];
                if (_modules.TryGetValue(moduleName, out var module))
                {
                    try
                    {
                        if (module.IsEnabled)
                        {
                            module.Disable();
                        }
                        module.Shutdown();
                        MelonLogger.Msg($"[ModuleRegistry] 关闭模块: {moduleName}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[ModuleRegistry] 关闭模块 {moduleName} 异常: {ex.Message}");
                    }
                }
            }

            _loadOrder.Clear();
            MelonLogger.Msg("[ModuleRegistry] 所有模块已关闭");
        }

        /// <summary>
        /// 解析加载顺序
        /// </summary>
        private static bool ResolveLoadOrder()
        {
            _loadOrder.Clear();
            var visited = new HashSet<string>();
            var processing = new HashSet<string>();

            foreach (var moduleName in _modules.Keys)
            {
                if (!Visit(moduleName, visited, processing))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 深度优先访问处理依赖
        /// </summary>
        private static bool Visit(string moduleName, HashSet<string> visited, HashSet<string> processing)
        {
            if (visited.Contains(moduleName))
            {
                return true;
            }

            if (processing.Contains(moduleName))
            {
                // 循环依赖
                return false;
            }

            if (!_modules.TryGetValue(moduleName, out var module))
            {
                MelonLogger.Error($"[ModuleRegistry] 找不到依赖模块: {moduleName}");
                return false;
            }

            processing.Add(moduleName);

            // 先处理所有依赖
            foreach (var dependency in module.Dependencies)
            {
                if (!IsRegistered(dependency))
                {
                    MelonLogger.Error($"[ModuleRegistry] 模块 {moduleName} 依赖的模块 {dependency} 未注册");
                    return false;
                }

                if (!Visit(dependency, visited, processing))
                {
                    return false;
                }
            }

            processing.Remove(moduleName);
            visited.Add(moduleName);
            _loadOrder.Add(moduleName);
            return true;
        }

        /// <summary>
        /// 清除所有模块
        /// </summary>
        public static void Clear()
        {
            ShutdownAll();
            _modules.Clear();
            _loadOrder.Clear();
        }
    }
}
