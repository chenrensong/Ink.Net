// Mirrors ink/test/fixtures/*.tsx — invoked as: dotnet run -- <fixture-name>
using Ink.Net;
using Ink.Net.Builder;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Ink.Net.TestFixtures <fixture-name>");
    return 1;
}

return args[0] switch
{
    "exit-normally" => await RunExitNormallyAsync(),
    "exit-on-unmount" => await RunExitOnUnmountAsync(),
    "use-stdout" => await RunUseStdoutAsync(),
    "exit-on-finish" => await RunExitOnFinishAsync(),
    _ => Unknown(args[0]),
};

static int Unknown(string name)
{
    Console.Error.WriteLine($"Unknown fixture: {name}");
    return 2;
}

static async Task<int> RunExitNormallyAsync()
{
    using var app = InkApplication.Create(
        b => new[] { b.Text("Hello World") },
        new InkApplicationOptions { Columns = 100 });

    app.Lifecycle.Exit();
    await app.WaitUntilExit();
    Console.WriteLine("exited");
    return 0;
}

static async Task<int> RunExitOnUnmountAsync()
{
    var counter = 0;

    TreeNode[] Build(TreeBuilder b) => new[] { b.Text($"Counter: {counter}") };

    var app = InkApplication.Create(Build, new InkApplicationOptions { Columns = 100 });

    _ = Task.Run(async () =>
    {
        while (!app.Lifecycle.HasExited)
        {
            await Task.Delay(100);
            if (app.Lifecycle.HasExited) break;
            counter++;
            try { app.Rerender(Build); }
            catch (ObjectDisposedException) { break; }
        }
    });

    var exitWait = app.WaitUntilExit();
    await Task.Delay(500);
    app.Dispose();
    await exitWait;

    Console.WriteLine("exited");
    return 0;
}

static async Task<int> RunUseStdoutAsync()
{
    using var app = InkApplication.Create(
        b => new[] { b.Text("Hello World") },
        new InkApplicationOptions { Columns = 100 });

    app.Stdout.Write("Hello from Ink to stdout\n");
    app.Lifecycle.Exit();
    await app.WaitUntilExit();
    Console.WriteLine("exited");
    return 0;
}

static async Task<int> RunExitOnFinishAsync()
{
    var counter = 0;
    var inst = InkApp.Render(
        b => new[] { b.Text($"Counter: {counter}") },
        new RenderOptions { Columns = 100 });

    while (counter <= 4)
    {
        await Task.Delay(20);
        counter++;
        inst.Rerender(b => new[] { b.Text($"Counter: {counter}") });
    }

    inst.Unmount();
    return 0;
}
