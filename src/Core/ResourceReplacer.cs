using System;
using System.Collections.Generic;
using System.IO;

namespace AstralPartyMod.Core
{
    /// <summary>
    /// 资源替换器 - 管理Mod资源替换映射
    /// </summary>
    public class ResourceReplacer
    {
        // 资源映射表：原始文件名 -> Mod文件路径
        private readonly Dictionary<string, string> _replacements = new(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// 资源替换事件
        /// </summary>
        public event Action? OnResourceReplaced;
        
        /// <summary>
        /// 获取替换映射数量
        /// </summary>
        public int Count => _replacements.Count;
        
        /// <summary>
        /// 添加替换映射
        /// </summary>
        /// <param name="fileName">原始文件名</param>
        /// <param name="modPath">Mod文件路径</param>
        public void AddReplacement(string fileName, string modPath)
        {
            _replacements[fileName] = modPath;
        }
        
        /// <summary>
        /// 尝试获取替换路径
        /// </summary>
        /// <param name="fileName">原始文件名</param>
        /// <param name="modPath">Mod文件路径（输出）</param>
        /// <returns>是否找到替换</returns>
        public bool TryGetReplacement(string fileName, out string? modPath)
        {
            return _replacements.TryGetValue(fileName, out modPath);
        }
        
        /// <summary>
        /// 检查是否存在替换
        /// </summary>
        public bool HasReplacement(string fileName)
        {
            return _replacements.ContainsKey(fileName);
        }
        
        /// <summary>
        /// 获取所有替换映射
        /// </summary>
        public IReadOnlyDictionary<string, string> GetAllReplacements()
        {
            return _replacements;
        }
        
        /// <summary>
        /// 清空所有替换映射
        /// </summary>
        public void Clear()
        {
            _replacements.Clear();
        }
        
        /// <summary>
        /// 移除指定替换映射
        /// </summary>
        public bool RemoveReplacement(string fileName)
        {
            return _replacements.Remove(fileName);
        }
        
        /// <summary>
        /// 触发资源替换事件
        /// </summary>
        internal void NotifyResourceReplaced()
        {
            OnResourceReplaced?.Invoke();
        }
    }
}
