using MelonLoader;

#if UNITYEXPLORER_AVAILABLE
using UnityExplorer;
using UniverseLib;
#endif

namespace AstralPartyMod.Core.Debugging
{
    /// <summary>
    /// UnityExplorer调试工具初始化
    /// </summary>
    public static class UnityExplorerInit
    {
        private static bool _initialized = false;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => _initialized;

#if UNITYEXPLORER_AVAILABLE
        /// <summary>
        /// 初始化UnityExplorer
        /// </summary>
        public static bool Initialize(bool autoStart = false)
        {
            if (_initialized)
            {
                MelonLogger.Warning("[UnityExplorerInit] 已经初始化过了");
                return true;
            }

            try
            {
                // 设置配置
                UniverseLib.Config.Settings.Disable_UnityExplorer_DefaultHotkey = false;
                
                // 初始化UnityExplorer
                ExplorerStandalone.CreateInstance();
                
                _initialized = true;
                MelonLogger.Msg("[UnityExplorerInit] UnityExplorer 初始化成功");
                MelonLogger.Msg("[UnityExplorerInit] 默认快捷键 F7 打开调试面板");
                
                if (autoStart)
                {
                    ExplorerStandalone.Instance?.Show();
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[UnityExplorerInit] 初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 显示调试面板
        /// </summary>
        public static void Show()
        {
            if (!_initialized || ExplorerStandalone.Instance == null)
            {
                return;
            }

            ExplorerStandalone.Instance.Show();
        }

        /// <summary>
        /// 隐藏调试面板
        /// </summary>
        public static void Hide()
        {
            if (!_initialized || ExplorerStandalone.Instance == null)
            {
                return;
            }

            ExplorerStandalone.Instance.Hide();
        }

        /// <summary>
        /// 切换显示状态
        /// </summary>
        public static void Toggle()
        {
            if (!_initialized || ExplorerStandalone.Instance == null)
            {
                return;
            }

            if (ExplorerStandalone.Instance.Visible)
            {
                ExplorerStandalone.Instance.Hide();
            }
            else
            {
                ExplorerStandalone.Instance.Show();
            }
        }
#else
        /// <summary>
        /// 初始化UnityExplorer (UnityExplorer不可用)
        /// </summary>
        public static bool Initialize(bool autoStart = false)
        {
            MelonLogger.Warning("[UnityExplorerInit] UnityExplorer 未启用，请在编译时定义UNITYEXPLORER_AVAILABLE");
            return false;
        }

        /// <summary>
        /// 显示调试面板 (UnityExplorer不可用)
        /// </summary>
        public static void Show() { }

        /// <summary>
        /// 隐藏调试面板 (UnityExplorer不可用)
        /// </summary>
        public static void Hide() { }

        /// <summary>
        /// 切换显示状态 (UnityExplorer不可用)
        /// </summary>
        public static void Toggle() { }
#endif
    }
}
