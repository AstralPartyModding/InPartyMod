using MelonLoader;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AstralPartyMod.Core.Debugging
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 最详细的日志级别，用于调试
        /// </summary>
        Trace = 0,

        /// <summary>
        /// 调试信息
        /// </summary>
        Debug = 1,

        /// <summary>
        /// 一般信息
        /// </summary>
        Info = 2,

        /// <summary>
        /// 警告信息
        /// </summary>
        Warn = 3,

        /// <summary>
        /// 错误信息
        /// </summary>
        Error = 4,

        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal = 5,

        /// <summary>
        /// 关闭日志
        /// </summary>
        Off = 6
    }

    /// <summary>
    /// ModLogger - 支持TRACE/DEBUG/INFO/WARN/ERROR五级日志控制
    /// </summary>
    public static class ModLogger
    {
        /// <summary>
        /// 当前日志级别
        /// </summary>
        public static LogLevel CurrentLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Mod名称（用于日志前缀）
        /// </summary>
        private static string _modName = "AstralPartyMod";

        /// <summary>
        /// 是否启用颜色输出（控制台）
        /// </summary>
        public static bool EnableColorOutput { get; set; } = true;

        /// <summary>
        /// 日志级别名称
        /// </summary>
        private static readonly Dictionary<LogLevel, string> _levelNames = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, "TRACE" },
            { LogLevel.Debug, "DEBUG" },
            { LogLevel.Info, "INFO" },
            { LogLevel.Warn, "WARN" },
            { LogLevel.Error, "ERROR" },
            { LogLevel.Fatal, "FATAL" },
            { LogLevel.Off, "OFF" }
        };

        /// <summary>
        /// 日志级别颜色（ANSI颜色码）
        /// </summary>
        private static readonly Dictionary<LogLevel, string> _levelColors = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, "\x1b[90m" },   // 灰色
            { LogLevel.Debug, "\x1b[36m" },   // 青色
            { LogLevel.Info, "\x1b[32m" },    // 绿色
            { LogLevel.Warn, "\x1b[33m" },    // 黄色
            { LogLevel.Error, "\x1b[31m" },   // 红色
            { LogLevel.Fatal, "\x1b[35m" },   // 紫色
            { LogLevel.Off, "\x1b[0m" }       // 重置
        };

        /// <summary>
        /// 重置颜色
        /// </summary>
        private const string ColorReset = "\x1b[0m";

        /// <summary>
        /// 设置Mod名称
        /// </summary>
        public static void SetModName(string modName)
        {
            _modName = modName;
        }

        /// <summary>
        /// 从环境变量或配置初始化日志级别
        /// </summary>
        public static void InitializeFromConfig(bool enableDetailedLogging)
        {
            CurrentLevel = enableDetailedLogging ? LogLevel.Debug : LogLevel.Info;
        }

        /// <summary>
        /// 记录TRACE级别日志
        /// </summary>
        public static void Trace(string message, [CallerMemberName] string? caller = null)
        {
            Log(LogLevel.Trace, message, caller);
        }

        /// <summary>
        /// 记录DEBUG级别日志
        /// </summary>
        public static void Debug(string message, [CallerMemberName] string? caller = null)
        {
            Log(LogLevel.Debug, message, caller);
        }

        /// <summary>
        /// 记录INFO级别日志
        /// </summary>
        public static void Info(string message, [CallerMemberName] string? caller = null)
        {
            Log(LogLevel.Info, message, caller);
        }

        /// <summary>
        /// 记录WARN级别日志
        /// </summary>
        public static void Warn(string message, [CallerMemberName] string? caller = null)
        {
            Log(LogLevel.Warn, message, caller);
        }

        /// <summary>
        /// 记录ERROR级别日志
        /// </summary>
        public static void Error(string message, [CallerMemberName] string? caller = null)
        {
            Log(LogLevel.Error, message, caller);
        }

        /// <summary>
        /// 记录ERROR级别日志（带异常）
        /// </summary>
        public static void Error(string message, Exception? ex, [CallerMemberName] string? caller = null)
        {
            if (ex != null)
            {
                Log(LogLevel.Error, $"{message}\n{ex}", caller);
            }
            else
            {
                Log(LogLevel.Error, message, caller);
            }
        }

        /// <summary>
        /// 记录FATAL级别日志
        /// </summary>
        public static void Fatal(string message, [CallerMemberName] string? caller = null)
        {
            Log(LogLevel.Fatal, message, caller);
        }

        /// <summary>
        /// 核心日志方法
        /// </summary>
        private static void Log(LogLevel level, string message, string? caller)
        {
            if (level < CurrentLevel)
            {
                return;
            }

            string levelName = _levelNames[level];
            string color = EnableColorOutput ? _levelColors[level] : "";
            string prefix = $"[{_modName}] [{levelName}]";
            
            if (!string.IsNullOrEmpty(caller))
            {
                prefix += $" [{caller}]";
            }

            string formattedMessage = $"{color}{prefix} {message}{ColorReset}";

            // 使用MelonLoader的日志系统
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    MelonLogger.Msg(formattedMessage);
                    break;
                case LogLevel.Warn:
                    MelonLogger.Warning(formattedMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    MelonLogger.Error(formattedMessage);
                    break;
            }
        }

        /// <summary>
        /// 格式化消息（便捷方法）
        /// </summary>
        public static string Format(string template, params object[] args)
        {
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template + " (格式化失败)";
            }
        }

        /// <summary>
        /// 获取日志级别的显示名称
        /// </summary>
        public static string GetLevelName(LogLevel level)
        {
            return _levelNames.TryGetValue(level, out var name) ? name : "UNKNOWN";
        }

        /// <summary>
        /// 解析字符串为日志级别
        /// </summary>
        public static LogLevel ParseLevel(string levelStr)
        {
            if (Enum.TryParse<LogLevel>(levelStr, true, out var level))
            {
                return level;
            }

            // 尝试一些别名
            return levelStr.ToLowerInvariant() switch
            {
                "verbose" or "vb" or "v" => LogLevel.Trace,
                "detailed" or "detailedlogging" => LogLevel.Debug,
                "information" or "normal" => LogLevel.Info,
                "warning" => LogLevel.Warn,
                "err" or "er" => LogLevel.Error,
                "critical" or "crit" => LogLevel.Fatal,
                _ => LogLevel.Info
            };
        }
    }

    /// <summary>
    /// 条件日志助手 - 当条件满足时才记录日志
    /// </summary>
    public static class ConditionalLogger
    {
        /// <summary>
        /// 如果条件满足，记录DEBUG日志
        /// </summary>
        public static void DebugIf(bool condition, string message, [CallerMemberName] string? caller = null)
        {
            if (condition)
            {
                ModLogger.Debug(message, caller);
            }
        }

        /// <summary>
        /// 如果条件满足，记录INFO日志
        /// </summary>
        public static void InfoIf(bool condition, string message, [CallerMemberName] string? caller = null)
        {
            if (condition)
            {
                ModLogger.Info(message, caller);
            }
        }

        /// <summary>
        /// 如果条件满足，记录WARN日志
        /// </summary>
        public static void WarnIf(bool condition, string message, [CallerMemberName] string? caller = null)
        {
            if (condition)
            {
                ModLogger.Warn(message, caller);
            }
        }

        /// <summary>
        /// 如果条件满足，记录ERROR日志
        /// </summary>
        public static void ErrorIf(bool condition, string message, [CallerMemberName] string? caller = null)
        {
            if (condition)
            {
                ModLogger.Error(message, caller);
            }
        }
    }
}
