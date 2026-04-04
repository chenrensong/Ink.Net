// -----------------------------------------------------------------------
// <copyright file="SynchronizedWrite.cs" company="Ink.Net">
//   1:1 Port from Ink (JS) write-synchronized.ts
// </copyright>
// -----------------------------------------------------------------------

namespace Ink.Net.Terminal;

/// <summary>
/// Synchronized output constants and helpers.
/// <para>1:1 port of Ink JS <c>write-synchronized.ts</c>.</para>
/// </summary>
public static class SynchronizedWrite
{
    /// <summary>Begin Synchronized Update (BSU) escape sequence.</summary>
    public const string Bsu = "\u001B[?2026h";

    /// <summary>End Synchronized Update (ESU) escape sequence.</summary>
    public const string Esu = "\u001B[?2026l";

    /// <summary>
    /// Determine whether synchronized output should be used.
    /// <para>Corresponds to JS <c>shouldSynchronize(stream, interactive)</c>.</para>
    /// </summary>
    /// <param name="isRedirected">Whether the output stream is redirected (piped).</param>
    /// <param name="interactive">Whether the app is interactive. Defaults to non-CI detection.</param>
    public static bool ShouldSynchronize(bool isRedirected, bool? interactive = null)
    {
        bool isTty = !isRedirected;
        bool isInteractive = interactive ?? !IsInCi();
        return isTty && isInteractive;
    }

    /// <summary>
    /// Simple CI detection based on environment variables.
    /// <para>Port of <c>is-in-ci</c> npm package.</para>
    /// </summary>
    private static bool IsInCi()
    {
        return Environment.GetEnvironmentVariable("CI") is not null
            || Environment.GetEnvironmentVariable("CONTINUOUS_INTEGRATION") is not null
            || Environment.GetEnvironmentVariable("BUILD_NUMBER") is not null
            || Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null
            || Environment.GetEnvironmentVariable("TRAVIS") is not null
            || Environment.GetEnvironmentVariable("CIRCLECI") is not null
            || Environment.GetEnvironmentVariable("JENKINS_URL") is not null
            || Environment.GetEnvironmentVariable("GITLAB_CI") is not null
            || Environment.GetEnvironmentVariable("TF_BUILD") is not null;
    }
}
