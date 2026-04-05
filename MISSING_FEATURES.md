# Ink.Net 功能对照清单

本文档列出 Ink.Net 相对于 Claude Code 扩展版 Ink (JS) 的功能覆盖情况。

**状态: ✅ 功能完整** — 所有核心功能和性能优化均已实现。

---

## ✅ 已实现功能

### 核心组件 (Builder API)
- [x] `Box` - Flexbox 容器
- [x] `Text` - 文本渲染
- [x] `Spacer` - 弹性空间
- [x] `Newline` - 换行
- [x] `Transform` - 输出转换
- [x] `Static` - 静态内容
- [x] `Link` - 超链接 (OSC 8)
- [x] `RawAnsi` - 预渲染 ANSI 内容

### DOM 节点类型
- [x] Root / Box / Text / VirtualText / TextLiteral / Link / RawAnsi / Progress

### Hooks 对应
- [x] `useApp` → `AppLifecycle`
- [x] `useInput` → `InputHandler`
- [x] `usePaste` → `PasteHandler`
- [x] `useFocus` / `useFocusManager` → `FocusManager`
- [x] `useCursor` → `CursorManager`
- [x] `useWindowSize` → `WindowSizeMonitor`
- [x] `useStdin` → `StdinProvider`
- [x] `useStdout` → `StdoutProvider`
- [x] `useStderr` → `StderrProvider`
- [x] `useAnimationFrame` → `AnimationFrame` + `Clock`
- [x] `useDeclaredCursor` → `DeclaredCursor`
- [x] `useTerminalFocus` → `TerminalFocusTracker`
- [x] `useTerminalTitle` → `TerminalTitle`
- [x] `useTabStatus` → `TabStatus`
- [x] `useSearchHighlight` → `SearchHighlight`
- [x] `useSelection` → `SelectionState`

### 事件系统
- [x] `InkEvent` / `ClickEvent` / `FocusEvent` / `KeyboardEvent` / `InputEvent` / `TerminalFocusEvent`
- [x] `EventDispatcher` — DOM-like capture/bubble 事件分发

### Screen Buffer (性能核心)
- [x] `ScreenBuffer` — 打包 Int32 数组 cell 缓冲区（2 Int32/cell）
- [x] `CharPool` — 字符串内部化池（ASCII 快速路径）
- [x] `HyperlinkPool` — 超链接内部化池
- [x] `StylePool` — ANSI 样式内部化池 + 转换缓存
- [x] `CellWidth` — 宽字符分类（Narrow/Wide/SpacerTail/SpacerHead）
- [x] Damage tracking / Blit / Region clear / Row shift / Cell diff

### Terminal I/O (termio)
- [x] `Tokenizer` — 流式转义序列分词器
- [x] `SgrParser` — SGR 参数解析器（16/256/RGB 颜色）
- [x] `CsiHelper` — CSI 序列生成器
- [x] `DecMode` — DEC 私有模式序列
- [x] `OscHelper` — OSC 序列生成器（超链接/剪贴板/tmux）
- [x] `TermColor` / `TextStyle` / `TermAction` — 语义类型

### 渲染优化
- [x] `NodeCache` — 节点布局缓存（ConditionalWeakTable）
- [x] `DiffOptimizer` — Diff patch 优化（合并/去重/取消）
- [x] `Geometry` — Point / Size / Rectangle（Union/Intersect/Contains）

### 文本处理
- [x] `TabExpander` — ANSI 感知 Tab 展开（8 列间隔）
- [x] `AnsiWrapper` — ANSI 感知文本换行
- [x] `TextWrapper` — 基础文本换行
- [x] `TextMeasurer` — 文本尺寸测量
- [x] `TextSquasher` — 文本节点合并
- [x] `StringWidthHelper` — 终端宽度计算（CJK/Emoji/组合字符）

### 终端交互
- [x] `AlternateScreen` / `MouseTracking` / `TerminalFocusTracker`
- [x] `DeclaredCursor` / `TerminalTitle` / `TabStatus`
- [x] `TerminalQuerier` / `HyperlinkSupport` — 终端能力检测

### 选择与搜索
- [x] `SelectionState` / `NoSelectRegion` / `SearchHighlight`

### 渲染系统
- [x] `InkRenderer` / `Output` / `NodeRenderer` / `BorderRenderer`
- [x] `BackgroundRenderer` / `Colorizer` / `MeasureElement` / `BoxMetrics`
- [x] `HitTest` / `LogUpdate`

### 输入处理
- [x] `InputHandler` / `InputParser` / `KeypressParser`
- [x] `KittyKeyboard` / `PasteHandler`

### ANSI 处理
- [x] `AnsiTokenizer` / `AnsiSanitizer`

### 布局
- [x] Yoga.Net Flexbox 布局
- [x] overflow: visible / hidden / scroll
- [x] 完整样式系统 (InkStyle + StyleApplier)

### 动画
- [x] `Clock` / `AnimationFrame`

---

## 📚 架构映射

```
Claude Code Ink (JS)          →  Ink.Net (C#)
─────────────────────────         ─────────────────
dom.ts                         →  Dom/*
styles.ts                     →  Styles/*
reconciler.ts                 →  Builder/TreeBuilder
renderer.ts                   →  Rendering/InkRenderer
render-node-to-output.ts      →  Rendering/NodeRenderer
output.ts                     →  Rendering/Output
render-border.ts              →  Rendering/BorderRenderer
screen.ts                     →  Rendering/Screen/*
node-cache.ts                 →  Rendering/NodeCache
optimizer.ts                  →  Rendering/DiffOptimizer
layout/geometry.ts            →  Rendering/Geometry
ink.tsx / render.ts            →  InkApp, InkApplication
events/*                       →  Events/*
selection.ts                   →  Selection/*
termio/*                       →  Termio/*
tabstops.ts                   →  Text/TabExpander
wrapAnsi.ts                   →  Text/AnsiWrapper
supports-hyperlinks.ts        →  Terminal/HyperlinkSupport
hooks/*                        →  Terminal/*, Animation/*, Input/*
```

---

*最后更新: 2025-07*
