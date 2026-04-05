# ink ↔ Ink.Net 测试与示例对照

本表用于追踪官方 [ink](https://github.com/vadimdemedes/ink) 仓库中 `examples/`、`test/` 与 Ink.Net 的对应关系。条目会随移植进度扩展。

## Examples（`ink/examples/`）

| ink 目录 | Ink.Net |
|----------|---------|
| `use-transition` | `Ink.Net.Examples/UseTransitionExample.cs` + `Program.cs` → `use-transition` |
| `suspense` | `SuspenseExample.cs` → `suspense` |
| `concurrent-suspense` | `ConcurrentSuspenseExample.cs` → `concurrent-suspense` |
| `jest` | `JestStyleExample.cs` → `jest` |
| 其余目录 | 已有同名职责的 `*.cs`（见 `Program.cs` switch） |

## 顶层测试文件（`ink/test/*.tsx` / `*.ts`）

| ink 源文件 | Ink.Net |
|------------|---------|
| `reconciler.tsx` | `Ink.Net.Tests/ReconcilerTests.cs`（含 Suspense 两条语义单测） |
| `alternate-screen-example.tsx`（gameReducer） | `Ink.Net.SnakeGame/SnakeGameEngine.cs` + `AlternateScreenGameTests.cs` |
| `ansi-tokenizer.ts` | `AnsiTokenizerTests.cs` |
| `sanitize-ansi.ts` | `AnsiSanitizerTests.cs` |
| `input-parser.ts` | `InputParserTests.cs` |
| `parse-keypress.ts` | `KeypressParserTests.cs` |
| 其他 `*.tsx` 布局/组件/钩子等 | 多为 `*Tests.cs`，文件头注释常含对齐的 JS 文件名 |

## 子进程 Fixtures（`ink/test/fixtures/*.tsx`）

由 `dotnet run --project Ink.Net.TestFixtures -- <name>` 承载；集成测试见 `FixtureSubprocessTests.cs`（`[Trait("Category","Integration")]`）。

| ink fixture | Ink.Net.TestFixtures |
|-------------|----------------------|
| `exit-normally.tsx` | `exit-normally` |
| `exit-on-unmount.tsx` | `exit-on-unmount` |
| `use-stdout.tsx` | `use-stdout` |
| `exit-on-finish.tsx` | `exit-on-finish` |

其余 fixture 可按 Phase B 在 `Program.cs` 的 switch 中增量添加。

## `components.tsx` 等大型套件

用例数量多、含 CI/TTY/并发等场景；当前由多个 `*Tests.cs` 分摊覆盖。新增用例时请在本文件或对应 `*Tests.cs` 顶部注释中补充一行映射。
