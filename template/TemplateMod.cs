using MelonLoader;
using AstralPartyMod.Core;
using UnityEngine;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(TemplateMod.TemplateMod), "模板Mod", "1.0.0", "作者名")]
[assembly: MelonGame(null, null)]

namespace TemplateMod
{
    /// <summary>
    /// 星引擎Mod模板
    /// 基于AstralPartyMod.Core框架的资源替换Mod
    /// </summary>
    public class TemplateMod : CoreMod
    {
        // ========== 基础信息配置 ==========
        protected override string ModName => "模板Mod";
        protected override string ModVersion => "1.0.0";
        protected override string ModAuthor => "作者名";

        // ========== 资源目录配置 ==========
        /// <summary>
        /// 资源目录列表
        /// 相对于Mod根目录的路径，包含需要替换的资源文件
        /// </summary>
        protected override string[] ResourceDirectories => new[] { "resources/cards", "resources/events" };

        // ========== 功能开关配置 ==========
        protected override bool EnableStatistics => true;
        protected override bool EnableDetailedLogging => false;

        /// <summary>
        /// 启用分类资源管理
        /// 设为true时，可以按分类独立启用/禁用资源
        /// </summary>
        protected override bool UseCategoricalResources => true;

        // ========== Mod生命周期 ==========
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            // 初始化完成后的自定义逻辑
            MelonLogger.Msg($"{ModName} 初始化完成");
            MelonLogger.Msg("资源已加载，游戏启动时将自动替换");
        }

        /// <summary>
        /// 初始化默认分类配置
        /// 定义各分类的资源映射关系
        /// </summary>
        protected override void InitializeDefaultCategories()
        {
            // 手牌卡图分类
            Config.SetCategory("cards", new ResourceCategory
            {
                Path = "cards",
                Description = "手牌卡图替换",
                Enabled = true,
                ResourceMappings = new Dictionary<string, string>
                {
                    // 资源映射格式：{ "Mod文件名", "游戏内资源名" }
                    // 示例：{ "my_card_01.bundle", "handcard_001.bundle" },
                }
            });

            // 事件卡图分类
            Config.SetCategory("events", new ResourceCategory
            {
                Path = "events",
                Description = "事件卡图替换",
                Enabled = true,
                ResourceMappings = new Dictionary<string, string>
                {
                    // 资源映射格式：{ "Mod文件名", "游戏内资源名" }
                    // 示例：{ "my_event_01.bundle", "eventcard_001.bundle" },
                }
            });

            MelonLogger.Msg("分类配置已初始化");
        }
    }
}
