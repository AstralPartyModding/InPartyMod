using System.Collections.Generic;

namespace AstralPartyMod.Core.Modules
{
    /// <summary>
    /// 模块接口 - 所有功能模块必须实现此接口
    /// </summary>
    public interface IModModule
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 模块描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 模块版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 模块依赖的其他模块
        /// </summary>
        IReadOnlyList<string> Dependencies { get; }

        /// <summary>
        /// 模块是否启用
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <returns>初始化是否成功</returns>
        bool Initialize();

        /// <summary>
        /// 关闭模块
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 启用模块（在Initialize之后调用）
        /// </summary>
        bool Enable();

        /// <summary>
        /// 禁用模块
        /// </summary>
        bool Disable();
    }
}
