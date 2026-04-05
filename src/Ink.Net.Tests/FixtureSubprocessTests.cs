// -----------------------------------------------------------------------
// Spawns Ink.Net.TestFixtures (mirrors ink/test/helpers/run.ts + fixtures/*.tsx).
// -----------------------------------------------------------------------

using System.Diagnostics;
using Xunit;

namespace Ink.Net.Tests;

[Trait("Category", "Integration")]
public class FixtureSubprocessTests
{
    private static string TestFixturesProjectPath
    {
        get
        {
            var testBin = AppContext.BaseDirectory;
            var src = Path.GetFullPath(Path.Combine(testBin, "..", "..", "..", ".."));
            return Path.Combine(src, "Ink.Net.TestFixtures", "Ink.Net.TestFixtures.csproj");
        }
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunFixtureAsync(
        string fixtureName,
        CancellationToken cancellationToken = default)
    {
        var proj = TestFixturesProjectPath;
        Assert.True(File.Exists(proj), $"Fixture project not found: {proj}");

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{proj}\" -f net9.0 --no-launch-profile -- {fixtureName}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
        p.Start();
        var stdout = await p.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await p.StandardError.ReadToEndAsync(cancellationToken);
        await p.WaitForExitAsync(cancellationToken);
        return (p.ExitCode, stdout, stderr);
    }

    [Fact]
    public async Task ExitNormally_PrintsExited()
    {
        var (code, stdout, _) = await RunFixtureAsync("exit-normally", TestContext.Current.CancellationToken);
        Assert.Equal(0, code);
        Assert.Contains("exited", stdout, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExitOnUnmount_PrintsExited()
    {
        var (code, stdout, _) = await RunFixtureAsync("exit-on-unmount", TestContext.Current.CancellationToken);
        Assert.Equal(0, code);
        Assert.Contains("exited", stdout, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UseStdout_WritesLineAndExited()
    {
        var (code, stdout, _) = await RunFixtureAsync("use-stdout", TestContext.Current.CancellationToken);
        Assert.Equal(0, code);
        Assert.Contains("Hello from Ink to stdout", stdout, StringComparison.Ordinal);
        Assert.Contains("exited", stdout, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExitOnFinish_ExitsZero()
    {
        var (code, _, _) = await RunFixtureAsync("exit-on-finish", TestContext.Current.CancellationToken);
        Assert.Equal(0, code);
    }
}
