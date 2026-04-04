// -----------------------------------------------------------------------
// <copyright file="OutputTransformer.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) render-node-to-output.ts — OutputTransformer
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Dom;

/// <summary>
/// 输出变换器委托，对应 Ink JS 中的 <c>OutputTransformer = (s: string, index: number) => string</c>。
/// <para>用于在渲染时对文本内容进行转换（如颜色、加粗等）。</para>
/// </summary>
/// <param name="text">待转换的文本。</param>
/// <param name="index">当前子节点在父节点中的索引。</param>
/// <returns>转换后的文本。</returns>
public delegate string OutputTransformer(string text, int index);
