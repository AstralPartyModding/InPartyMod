using MelonLoader;
using AstralPartyMod.Core;
using UnityEngine;

[assembly: MelonInfo(typeof(TemplateMod.TemplateMod), "Mod模板", "0.1.0", "YourName")]
[assembly: MelonGame(null, null)]

namespace TemplateMod
{
    /// <summary>
    /// Mod模板 - 用于创建新的星引擎Mod
    /// 
    /// 使用步骤：
    /// 1. 复制此目录并重命名为你的Mod名称
    /// 2. 修改项目文件中的AssemblyName和RootNamespace
    /// 3. 修改此文件中的Mod信息
    /// 4. 根据需要重写基类方法
    /// 5. 添加你的Mod特有功能
    /// </summary>
    public class TemplateMod : CoreMod
    {
        #region 必需：Mod基本信息
        
        protected override string ModName => "Mod模板";
        protected override string ModVersion => "0.1.0";
        protected override string ModAuthor => "YourName";
        protected override string[] ResourceDirectories => new[] { "Resources" };
        
        #endregion
        
        #region 可选：自定义配置
        
        // 热重载按键（默认F10）
        protected override KeyCode ReloadKey => KeyCode.F10;
        
        // 是否启用统计功能（默认false）
        protected override bool EnableStatistics => false;
        
        // 统计按键（仅在EnableStatistics为true时有效，默认F12）
        protected override KeyCode StatisticsKey => KeyCode.F12;
        
        // 是否启用详细日志（默认false）
        protected override bool EnableDetailedLogging => false;
        
        #endregion
        
        #region 可选：重写生命周期方法
        
        public override void OnInitializeMelon()
        {
            // 调用基类初始化（必须）
            base.OnInitializeMelon();
            
            // 在这里添加你的初始化代码
            MelonLogger.Msg("Mod模板已初始化！");
        }
        
        public override void OnUpdate()
        {
            // 调用基类更新（处理热重载和统计）
            base.OnUpdate();
            
            // 在这里添加每帧更新的代码
        }
        
        public override void OnDeinitializeMelon()
        {
            // 在这里添加清理代码
            
            // 调用基类清理（必须）
            base.OnDeinitializeMelon();
        }
        
        #endregion
        
        #region 可选：自定义资源扫描
        
        protected override void ScanResources()
        {
            // 调用基类扫描（必须）
            base.ScanResources();
            
            // 在这里添加自定义资源扫描逻辑
        }
        
        protected override void ReloadResources()
        {
            // 调用基类重载（必须）
            base.ReloadResources();
            
            // 在这里添加自定义重载逻辑
        }
        
        #endregion
        
        #region 可选：自定义统计显示
        
        protected override void ShowStatistics()
        {
            // 调用基类统计（必须）
            base.ShowStatistics();
            
            // 在这里添加自定义统计信息
        }
        
        #endregion
    }
}
