# Ink.Net 缺失功能清单

本文档列出 Ink.Net 相对于原始 Ink (JS) 尚未实现的核心功能。

**设计原则**: Ink.Net 提供基础接口，Claude Code Ink 等高级实现可通过扩展包补充。

---

## ✅ 已实现功能回顾

### 核心组件
- [x] `Box` - Flexbox 容器
- [x] `Text` - 文本渲染
- [x] `Spacer` - 弹性空间
- [x] `Newline` - 换行
- [x] `Transform` - 输出转换

### Hooks
- [x] `useApp` → `AppLifecycle`
- [x] `useInput` → `InputHandler`
- [x] `usePaste` → `PasteHandler`
- [x] `useFocus` / `useFocusManager` → `FocusManager`
- [x] `useCursor` → `CursorManager`
- [x] `useWindowSize` → `WindowSizeMonitor`

---

## ❌ 缺失功能（原始 Ink 1:1 对照）

### 1. Static 组件

**原始 Ink API**:
```tsx
import { Static } from 'ink';

<Static items={items}>
  {(item, index) => <Text key={item.id}>{item.message}</Text>}
</Static>
```

**说明**: Static 组件用于渲染不会更新的静态内容（如日志）。这些内容渲染在动态内容上方，且在重渲染时不会被清除。

**实现建议**:
```csharp
// Ink.Net 设计
public sealed class StaticContent
{
    public List<string> Lines { get; } = new();
    public void Add(string line) => Lines.Add(line);
    public void Clear() => Lines.Clear();
}

// InkApplication 扩展
public StaticContent Static { get; }

// TreeBuilder 扩展
public TreeNode Static(string[] lines);
```

---

### 2. measureElement

**原始 Ink API**:
```tsx
import { measureElement } from 'ink';

const ref = useRef();
const { width, height } = measureElement(ref);
```

**说明**: 获取 Yoga 布局计算后的元素尺寸。

**实现建议**:
```csharp
// 扩展现有 MeasureElement 类
public static class MeasureElement
{
    public static ElementSize Measure(DomElement element)
    {
        var yogaNode = element.YogaNode;
        if (yogaNode == null) return new ElementSize(0, 0);

        return new ElementSize(
            (int)YGNodeLayoutGetWidth(yogaNode),
            (int)YGNodeLayoutGetHeight(yogaNode)
        );
    }
}

public readonly record struct ElementSize(int Width, int Height);
```

---

### 3. useStdin / StdinContext

**原始 Ink API**:
```tsx
import { useStdin } from 'ink';

const { stdin, setRawMode } = useStdin();
```

**说明**: 提供对原始 stdin 流的访问和控制。

**实现建议**:
```csharp
public interface IStdinProvider
{
    Stream InputStream { get; }
    bool IsRawMode { get; }
    void SetRawMode(bool raw);
    event Action<string>? DataReceived;
}

// InkApplication 扩展
public IStdinProvider Stdin { get; }
```

---

### 4. useStdout / useStderr

**原始 Ink API**:
```tsx
import { useStdout, useStderr } from 'ink';

const { stdout, write } = useStdout();
```

**说明**: 提供对 stdout/stderr 流的访问。

**实现建议**:
```csharp
public interface IOutputProvider
{
    TextWriter Writer { get; }
    void Write(string data);
    void WriteRaw(string ansiSequence); // 直接写入原始 ANSI
}

// InkApplication 扩展
public IOutputProvider Stdout { get; }
public IOutputProvider Stderr { get; }
```

---

### 5. useBoxMetrics（响应式尺寸监听）

**原始 Ink API**:
```tsx
import { useBoxMetrics } from 'ink';

const { width, height } = useBoxMetrics(ref);
```

**说明**: 与 `measureElement` 不同，这是响应式的，当尺寸变化时会触发更新。

**实现建议**:
```csharp
public interface IBoxMetricsProvider : IDisposable
{
    ElementSize Metrics { get; }
    event Action<ElementSize>? MetricsChanged;
}

// InkApplication 扩展
public IBoxMetricsProvider WatchMetrics(DomElement element);
```

---

### 6. useIsScreenReaderEnabled

**原始 Ink API**:
```tsx
import { useIsScreenReaderEnabled } from 'ink';

const isEnabled = useIsScreenReaderEnabled();
```

**说明**: 检测屏幕阅读器是否启用，用于无障碍优化。

**实现建议**:
```csharp
public interface IAccessibilityProvider
{
    bool IsScreenReaderEnabled { get; }
    event Action<bool>? ScreenReaderStateChanged;
}

// InkApplication 扩展
public IAccessibilityProvider Accessibility { get; }
```

---

## 🔌 扩展接口设计（供 Claude Code Ink 使用）

Ink.Net 提供以下扩展点，允许高级实现（如 Claude Code Ink）添加功能：

### 1. 自定义组件注册

```csharp
public interface IComponentExtension
{
    string Name { get; }
    TreeNode Create(TreeBuilder builder, InkStyle? style, TreeNode[]? children);
}

public static class TreeBuilderExtensions
{
    public static void RegisterComponent(IComponentExtension extension);
}
```

### 2. 渲染器扩展

```csharp
public interface IRenderExtension
{
    // 在渲染前调用
    void BeforeRender(DomElement root);

    // 处理特定节点类型
    bool TryRenderNode(DomElement node, OutputContext context);

    // 在输出写入前调用
    void BeforeOutput(ref string output);
}

public class InkRenderer
{
    public void AddExtension(IRenderExtension extension);
}
```

### 3. 输入处理扩展

```csharp
public interface IInputExtension
{
    // 返回 true 表示已处理，不再传递给其他处理器
    bool HandleInput(string rawInput, InputContext context);
}

public class InputHandler
{
    public void AddExtension(IInputExtension extension);
}
```

### 4. 事件系统接口

```csharp
public interface IEventSystem
{
    void DispatchEvent<T>(T eventData) where T : class;
    void Subscribe<T>(Action<T> handler) where T : class;
}

// 使用示例
public class ClickEvent { public int X { get; set; } public int Y { get; set; } }
```

### 5. 屏幕缓冲区接口

```csharp
public interface IScreenBuffer
{
    int Width { get; }
    int Height { get; }
    void Write(int x, int y, char c, CellStyle style);
    char Read(int x, int y);
    void Clear();
    string ExportText();
}

// 高级实现可替换默认缓冲区
public interface IScreenBufferFactory
{
    IScreenBuffer Create(int width, int height);
}
```

### 6. 时钟/动画接口

```csharp
public interface IClock
{
    long Now { get; }
    long TickInterval { get; }
    IDisposable Subscribe(Action<long> callback, bool keepAlive = false);
}

// InkApplication 可配置自定义时钟
public class InkApplicationOptions
{
    public IClock? Clock { get; init; }
}
```

---

## 📋 实现优先级

| 优先级 | 功能 | 工作量 | 备注 |
|--------|------|--------|------|
| P0 | `measureElement` | 2h | 简单，已有 Yoga 数据 |
| P0 | `Static` 组件 | 4h | 需要修改渲染管线 |
| P1 | `useStdout` / `useStderr` | 2h | 封装现有 TextWriter |
| P1 | `useStdin` | 3h | 需要 Raw Mode 支持 |
| P2 | `useBoxMetrics` | 4h | 需要监听布局变化 |
| P2 | 扩展接口 | 8h | 为 Claude Code Ink 预留 |
| P3 | `useIsScreenReaderEnabled` | 2h | 检测逻辑较复杂 |

---

## 🎯 Claude Code Ink 扩展建议

基于上述接口，Claude Code Ink 可以实现为 Ink.Net 的扩展包：

```csharp
// ClaudeCode.Ink.Extensions 包
public static class InkApplicationExtensions
{
    // 添加 Claude Code 特有的功能
    public static void EnableAdvancedFeatures(this InkApplication app)
    {
        // 鼠标支持
        app.Input.AddExtension(new MouseTrackingExtension());

        // 动画时钟
        app.Options.Clock = new SharedClock();

        // 屏幕缓冲区
        app.Renderer.ScreenBufferFactory = new SelectableScreenBufferFactory();

        // 注册自定义组件
        app.Builder.RegisterComponent(new LinkComponent());
        app.Builder.RegisterComponent(new ButtonComponent());
        app.Builder.RegisterComponent(new ScrollBoxComponent());
    }
}
```

---

## 📚 参考

- [原始 Ink API 文档](https://github.com/vadimdemedes/ink#api)
- [Yoga Layout 文档](https://www.yogalayout.dev/)

---

*最后更新: 2025-01*
