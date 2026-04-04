// -----------------------------------------------------------------------
// 集成测试：验证 Ink.Net DOM + Style + MeasureFunc + Yoga 布局
// -----------------------------------------------------------------------

using Ink.Net.Dom;
using Ink.Net.Styles;
using Ink.Net.Text;
using Facebook.Yoga;
using static Facebook.Yoga.YGNodeAPI;
using static Facebook.Yoga.YGNodeStyleAPI;
using static Facebook.Yoga.YGNodeLayoutAPI;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Ink.Net Integration Test ===\n");

int passed = 0;
int failed = 0;

void Assert(bool condition, string name)
{
    if (condition) { Console.WriteLine($"  ✅ {name}"); passed++; }
    else { Console.WriteLine($"  ❌ {name} FAILED"); failed++; }
}

// ─── Test 1: 基础 DOM 树构建 ─────────────────────────────────────────
Console.WriteLine("--- Test 1: DOM Tree Construction ---");
{
    var root    = DomTree.CreateNode(InkNodeType.Root);
    var box     = DomTree.CreateNode(InkNodeType.Box);
    var text    = DomTree.CreateNode(InkNodeType.Text);
    var literal = DomTree.CreateTextNode("Hello, Ink.Net!");

    DomTree.AppendChildNode(root, box);
    DomTree.AppendChildNode(box, text);
    DomTree.AppendChildNode(text, literal);

    Assert(root.NodeType == InkNodeType.Root, "Root type");
    Assert(box.NodeType == InkNodeType.Box, "Box type");
    Assert(text.NodeType == InkNodeType.Text, "Text type");
    Assert(literal.NodeType == InkNodeType.TextLiteral, "Literal type");
    Assert(root.YogaNode is not null, "Root has YogaNode");
    Assert(box.YogaNode is not null, "Box has YogaNode");
    Assert(text.YogaNode is not null, "Text has YogaNode");
    Assert(literal.YogaNode is null, "TextLiteral has no YogaNode");
    Assert(root.ChildNodes.Count == 1, "Root has 1 child");
    Assert(box.ChildNodes.Count == 1, "Box has 1 child");
    Assert(text.ChildNodes.Count == 1, "Text has 1 child");
    Console.WriteLine();
}

// ─── Test 2: VirtualText 不创建 YogaNode ──────────────────────────────
Console.WriteLine("--- Test 2: VirtualText has no YogaNode ---");
{
    var vtext = DomTree.CreateNode(InkNodeType.VirtualText);
    Assert(vtext.YogaNode is null, "VirtualText has no YogaNode");
    Assert(vtext.NodeType == InkNodeType.VirtualText, "VirtualText type");
    Console.WriteLine();
}

// ─── Test 3: CJK 字符宽度计算 ─────────────────────────────────────────
Console.WriteLine("--- Test 3: CJK Character Width ---");
{
    int w1 = StringWidthHelper.GetStringWidth("Hello");
    int w2 = StringWidthHelper.GetStringWidth("你好世界");
    int w3 = StringWidthHelper.GetStringWidth("Hello 你好");
    int w4 = StringWidthHelper.GetStringWidth("\x1B[31mRed\x1B[0m");
    int w5 = StringWidthHelper.GetStringWidth("");
    int w6 = StringWidthHelper.GetStringWidth("abc");

    Console.WriteLine($"  'Hello'          → width {w1} (expect 5)");
    Console.WriteLine($"  '你好世界'       → width {w2} (expect 8)");
    Console.WriteLine($"  'Hello 你好'     → width {w3} (expect 10)");
    Console.WriteLine($"  '\\e[31mRed\\e[0m' → width {w4} (expect 3)");

    Assert(w1 == 5, "ASCII width = 5");
    Assert(w2 == 8, "CJK '你好世界' width = 8");
    Assert(w3 == 10, "Mixed 'Hello 你好' width = 10");
    Assert(w4 == 3, "ANSI stripped 'Red' width = 3");
    Assert(w5 == 0, "Empty string width = 0");
    Assert(w6 == 3, "'abc' width = 3");
    Console.WriteLine();
}

// ─── Test 4: TextMeasurer ──────────────────────────────────────────────
Console.WriteLine("--- Test 4: TextMeasurer ---");
{
    var d1 = TextMeasurer.Measure("Hello\nWorld");
    var d2 = TextMeasurer.Measure("你好\n世界测试");
    var d3 = TextMeasurer.Measure("");
    var d4 = TextMeasurer.Measure("Single");

    Console.WriteLine($"  'Hello\\nWorld'    → {d1.Width}x{d1.Height} (expect 5x2)");
    Console.WriteLine($"  '你好\\n世界测试'  → {d2.Width}x{d2.Height} (expect 8x2)");

    Assert(d1.Width == 5 && d1.Height == 2, "Measure 'Hello\\nWorld' = 5x2");
    Assert(d2.Width == 8 && d2.Height == 2, "Measure '你好\\n世界测试' = 8x2");
    Assert(d3.Width == 0 && d3.Height == 0, "Measure '' = 0x0");
    Assert(d4.Width == 6 && d4.Height == 1, "Measure 'Single' = 6x1");
    Console.WriteLine();
}

// ─── Test 5: TextSquasher ──────────────────────────────────────────────
Console.WriteLine("--- Test 5: TextSquasher ---");
{
    var text = DomTree.CreateNode(InkNodeType.Text);
    var t1 = DomTree.CreateTextNode("Hello");
    var t2 = DomTree.CreateTextNode(" ");
    var t3 = DomTree.CreateTextNode("World");

    DomTree.AppendChildNode(text, t1);
    DomTree.AppendChildNode(text, t2);
    DomTree.AppendChildNode(text, t3);

    var squashed = TextSquasher.Squash(text);
    Console.WriteLine($"  Squashed: '{squashed}' (expect 'Hello World')");
    Assert(squashed == "Hello World", "Squash 3 text nodes = 'Hello World'");
    Console.WriteLine();
}

// ─── Test 6: TextSquasher with nested VirtualText ──────────────────────
Console.WriteLine("--- Test 6: TextSquasher with nested VirtualText ---");
{
    var text  = DomTree.CreateNode(InkNodeType.Text);
    var vtext = DomTree.CreateNode(InkNodeType.VirtualText);
    var t1    = DomTree.CreateTextNode("Hello ");
    var t2    = DomTree.CreateTextNode("World");

    DomTree.AppendChildNode(text, t1);
    DomTree.AppendChildNode(text, vtext);
    DomTree.AppendChildNode(vtext, t2);

    var squashed = TextSquasher.Squash(text);
    Console.WriteLine($"  Squashed (nested): '{squashed}' (expect 'Hello World')");
    Assert(squashed == "Hello World", "Squash with nested VirtualText = 'Hello World'");
    Console.WriteLine();
}

// ─── Test 7: StyleApplier + Yoga Flex 布局 ─────────────────────────────
Console.WriteLine("--- Test 7: StyleApplier + Yoga Flex Layout ---");
{
    var root = DomTree.CreateNode(InkNodeType.Root);
    var box1 = DomTree.CreateNode(InkNodeType.Box);
    var box2 = DomTree.CreateNode(InkNodeType.Box);

    // Root: 80 列宽, row direction
    DomTree.SetStyle(root, new InkStyle
    {
        Width = 80,
        Height = 24,
        FlexDirection = FlexDirectionMode.Row,
    });
    StyleApplier.Apply(root.YogaNode!, root.Style);

    // Box1: flex-grow 1
    DomTree.SetStyle(box1, new InkStyle { FlexGrow = 1 });
    StyleApplier.Apply(box1.YogaNode!, box1.Style);

    // Box2: 固定宽度 20
    DomTree.SetStyle(box2, new InkStyle { Width = 20 });
    StyleApplier.Apply(box2.YogaNode!, box2.Style);

    DomTree.AppendChildNode(root, box1);
    DomTree.AppendChildNode(root, box2);

    // 计算布局
    YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

    float rootW = YGNodeLayoutGetWidth(root.YogaNode!);
    float box1W = YGNodeLayoutGetWidth(box1.YogaNode!);
    float box2W = YGNodeLayoutGetWidth(box2.YogaNode!);
    float box2X = YGNodeLayoutGetLeft(box2.YogaNode!);

    Console.WriteLine($"  Root:  width={rootW}");
    Console.WriteLine($"  Box1:  width={box1W} (flex-grow:1, expect 60)");
    Console.WriteLine($"  Box2:  width={box2W}, left={box2X} (fixed 20, expect left=60)");

    Assert(rootW == 80, "Root width = 80");
    Assert(box1W == 60, "Box1 (flex-grow) width = 60");
    Assert(box2W == 20, "Box2 (fixed) width = 20");
    Assert(box2X == 60, "Box2 left = 60");
    Console.WriteLine();
}

// ─── Test 8: MeasureFunc 文本测量 (ASCII) ──────────────────────────────
// 使用 alignItems:flexStart 使文本节点按固有尺寸布局，而非拉伸到容器宽度
Console.WriteLine("--- Test 8: MeasureFunc with ASCII Text ---");
{
    var root    = DomTree.CreateNode(InkNodeType.Root);
    var text    = DomTree.CreateNode(InkNodeType.Text);
    var literal = DomTree.CreateTextNode("Hello World");

    DomTree.SetStyle(root, new InkStyle
    {
        Width = 80,
        Height = 24,
        AlignItems = AlignItemsMode.FlexStart, // 不拉伸子节点
    });
    StyleApplier.Apply(root.YogaNode!, root.Style);

    DomTree.AppendChildNode(root, text);
    DomTree.AppendChildNode(text, literal);

    YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

    float textW = YGNodeLayoutGetWidth(text.YogaNode!);
    float textH = YGNodeLayoutGetHeight(text.YogaNode!);

    Console.WriteLine($"  Text: 'Hello World' → layout width={textW}, height={textH}");
    Console.WriteLine($"  Expected: width=11, height=1");

    Assert(textW == 11, "MeasureFunc: 'Hello World' width = 11");
    Assert(textH == 1, "MeasureFunc: 'Hello World' height = 1");
    Console.WriteLine();
}

// ─── Test 9: MeasureFunc 文本测量 (含中文) ─────────────────────────────
Console.WriteLine("--- Test 9: MeasureFunc with CJK Text ---");
{
    var root    = DomTree.CreateNode(InkNodeType.Root);
    var text    = DomTree.CreateNode(InkNodeType.Text);
    var literal = DomTree.CreateTextNode("你好世界 Hello");

    DomTree.SetStyle(root, new InkStyle
    {
        Width = 80,
        Height = 24,
        AlignItems = AlignItemsMode.FlexStart, // 不拉伸子节点
    });
    StyleApplier.Apply(root.YogaNode!, root.Style);

    DomTree.AppendChildNode(root, text);
    DomTree.AppendChildNode(text, literal);

    YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

    float textW = YGNodeLayoutGetWidth(text.YogaNode!);
    float textH = YGNodeLayoutGetHeight(text.YogaNode!);

    // "你好世界 Hello" = 8(CJK) + 1(space) + 5(ASCII) = 14 宽, 1 行
    Console.WriteLine($"  Text: '你好世界 Hello' → layout width={textW}, height={textH}");
    Console.WriteLine($"  Expected: width=14, height=1");

    Assert(textW == 14, "MeasureFunc: CJK text width = 14");
    Assert(textH == 1, "MeasureFunc: CJK text height = 1");
    Console.WriteLine();
}

// ─── Test 10: Border + Padding 布局 ────────────────────────────────────
Console.WriteLine("--- Test 10: Border + Padding Layout ---");
{
    var root    = DomTree.CreateNode(InkNodeType.Root);
    var box     = DomTree.CreateNode(InkNodeType.Box);
    var text    = DomTree.CreateNode(InkNodeType.Text);
    var literal = DomTree.CreateTextNode("Hi");

    DomTree.SetStyle(root, new InkStyle { Width = 40, Height = 10 });
    StyleApplier.Apply(root.YogaNode!, root.Style);

    DomTree.SetStyle(box, new InkStyle
    {
        BorderStyle = "single",
        Padding = 1,
    });
    StyleApplier.Apply(box.YogaNode!, box.Style);

    DomTree.AppendChildNode(root, box);
    DomTree.AppendChildNode(box, text);
    DomTree.AppendChildNode(text, literal);

    YGNodeCalculateLayout(root.YogaNode!, 40, 10, YGDirection.LTR);

    float boxW  = YGNodeLayoutGetWidth(box.YogaNode!);
    float textX = YGNodeLayoutGetLeft(text.YogaNode!);
    float textY = YGNodeLayoutGetTop(text.YogaNode!);

    Console.WriteLine($"  Box:  width={boxW}");
    Console.WriteLine($"  Text: left={textX}, top={textY} (expect border+padding = 2)");

    Assert(textX == 2, "Border+Padding: text left = 2");
    Assert(textY == 2, "Border+Padding: text top = 2");
    Console.WriteLine();
}

// ─── Test 11: Margin 布局 ──────────────────────────────────────────────
Console.WriteLine("--- Test 11: Margin Layout ---");
{
    var root = DomTree.CreateNode(InkNodeType.Root);
    var box  = DomTree.CreateNode(InkNodeType.Box);

    DomTree.SetStyle(root, new InkStyle { Width = 80, Height = 24 });
    StyleApplier.Apply(root.YogaNode!, root.Style);

    DomTree.SetStyle(box, new InkStyle
    {
        Width = 20,
        Height = 5,
        MarginLeft = 10,
        MarginTop = 3,
    });
    StyleApplier.Apply(box.YogaNode!, box.Style);

    DomTree.AppendChildNode(root, box);
    YGNodeCalculateLayout(root.YogaNode!, 80, 24, YGDirection.LTR);

    float boxX = YGNodeLayoutGetLeft(box.YogaNode!);
    float boxY = YGNodeLayoutGetTop(box.YogaNode!);

    Console.WriteLine($"  Box: left={boxX}, top={boxY} (expect 10, 3)");

    Assert(boxX == 10, "Margin: box left = 10");
    Assert(boxY == 3, "Margin: box top = 3");
    Console.WriteLine();
}

// ─── Test 12: RemoveChildNode ──────────────────────────────────────────
Console.WriteLine("--- Test 12: RemoveChildNode ---");
{
    var root = DomTree.CreateNode(InkNodeType.Root);
    var box1 = DomTree.CreateNode(InkNodeType.Box);
    var box2 = DomTree.CreateNode(InkNodeType.Box);

    DomTree.AppendChildNode(root, box1);
    DomTree.AppendChildNode(root, box2);
    Assert(root.ChildNodes.Count == 2, "Before remove: 2 children");

    DomTree.RemoveChildNode(root, box1);
    Assert(root.ChildNodes.Count == 1, "After remove: 1 child");
    Assert(box1.ParentNode is null, "Removed node has no parent");
    Console.WriteLine();
}

// ─── Test 13: SetAttribute ─────────────────────────────────────────────
Console.WriteLine("--- Test 13: SetAttribute ---");
{
    var box = DomTree.CreateNode(InkNodeType.Box);
    // 通过隐式转换将 string 转为 DomNodeAttribute
    DomTree.SetAttribute(box, "testKey", "testValue");
    Assert(box.Attributes.ContainsKey("testKey"), "Attribute set");
    Assert(box.Attributes["testKey"].StringValue == "testValue", "Attribute value correct");

    // 测试数值属性
    DomTree.SetAttribute(box, "numKey", 42);
    Assert(box.Attributes["numKey"].NumberValue == 42, "Number attribute correct");

    // 测试布尔属性
    DomTree.SetAttribute(box, "boolKey", true);
    Assert(box.Attributes["boolKey"].BoolValue == true, "Bool attribute correct");
    Console.WriteLine();
}

// ─── Test 14: Layout Listeners (Root) ──────────────────────────────────
Console.WriteLine("--- Test 14: Layout Listeners ---");
{
    var root = DomTree.CreateNode(InkNodeType.Root);
    int callCount = 0;
    var disposable = DomTree.AddLayoutListener(root, () => callCount++);

    DomTree.EmitLayoutListeners(root);
    Assert(callCount == 1, "Listener called once");

    DomTree.EmitLayoutListeners(root);
    Assert(callCount == 2, "Listener called twice");

    disposable.Dispose();
    DomTree.EmitLayoutListeners(root);
    Assert(callCount == 2, "After dispose, no more calls");
    Console.WriteLine();
}

// ─── Test 15: DimensionValue 解析 ──────────────────────────────────────
Console.WriteLine("--- Test 15: DimensionValue ---");
{
    DimensionValue v1 = 42;
    DimensionValue v2 = DimensionValue.Percent(50);
    DimensionValue v3 = DimensionValue.Parse("75%");
    DimensionValue v4 = DimensionValue.AutoValue;

    Assert(v1.IsPoints && v1.Value == 42, "Implicit int → Points(42)");
    Assert(v2.IsPercent && v2.Value == 50, "Percent(50)");
    Assert(v3.IsPercent && v3.Value == 75, "Parse('75%') → Percent(75)");
    Assert(v4.IsAuto, "AutoValue is auto");
    Console.WriteLine();
}

// ═══ 结果汇总 ══════════════════════════════════════════════════════════
Console.WriteLine("=== Results ===");
Console.WriteLine($"  Passed: {passed}");
Console.WriteLine($"  Failed: {failed}");
Console.WriteLine($"  Total:  {passed + failed}");
Console.WriteLine();

if (failed > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("⚠️  SOME TESTS FAILED!");
    Console.ResetColor();
    Environment.Exit(1);
}
else
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("🎉 ALL TESTS PASSED!");
    Console.ResetColor();
}
