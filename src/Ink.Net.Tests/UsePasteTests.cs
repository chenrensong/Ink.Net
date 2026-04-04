// Tests ported from hooks-use-paste.tsx
// Covers: PasteHandler — bracketed paste mode, paste events
using Ink.Net.Input;
using Xunit;

namespace Ink.Net.Tests;

/// <summary>Paste handling tests aligned with JS hooks-use-paste.tsx test suite.</summary>
public class UsePasteTests
{
    private static (PasteHandler paste, InputHandler input, StringWriter stdout) CreateSetup()
    {
        var stdout = new StringWriter();
        var paste = new PasteHandler(stdout);
        var input = new InputHandler { ExitOnCtrlC = false };
        // Wire paste events
        input.PasteReceived += text => paste.HandlePaste(text);
        return (paste, input, stdout);
    }

    [Fact]
    public void ReceivesBracketedPasteAsSingleTextBlob()
    {
        var (paste, input, stdout) = CreateSetup();
        string? received = null;
        paste.Register(text => received = text);

        input.HandleData("\u001B[200~hello world\u001B[201~");

        Assert.Equal("hello world", received);
    }

    [Fact]
    public void EnablesBracketedPasteModeOnRegister()
    {
        var (paste, _, stdout) = CreateSetup();

        paste.Register(_ => { });

        var output = stdout.ToString();
        Assert.Contains("\u001B[?2004h", output);
    }

    [Fact]
    public void DisablesBracketedPasteModeOnUnregister()
    {
        var (paste, _, stdout) = CreateSetup();

        var reg = paste.Register(_ => { });
        reg.Dispose();

        var output = stdout.ToString();
        Assert.Contains("\u001B[?2004l", output);
    }

    [Fact]
    public void PasteContentWithEscapeSequencesIsDeliveredVerbatim()
    {
        var (paste, input, _) = CreateSetup();
        string? received = null;
        paste.Register(text => received = text);

        input.HandleData("\u001B[200~hello\u001B[Aworld\u001B[201~");

        Assert.Equal("hello\u001B[Aworld", received);
    }

    [Fact]
    public void UseInputDoesNotReceiveBracketedPasteContent()
    {
        var (paste, input, _) = CreateSetup();
        var inputEvents = new List<string>();
        input.Register((text, _) => inputEvents.Add(text));
        paste.Register(_ => { });

        input.HandleData("\u001B[200~hello\u001B[201~");

        // Input handler should not see the paste content
        Assert.Empty(inputEvents);
    }

    [Fact]
    public void MultiplePasteHandlersBothReceiveSameEvent()
    {
        var (paste, input, _) = CreateSetup();
        string? received1 = null;
        string? received2 = null;

        paste.Register(text => received1 = text);
        paste.Register(text => received2 = text);

        input.HandleData("\u001B[200~hello\u001B[201~");

        Assert.Equal("hello", received1);
        Assert.Equal("hello", received2);
    }

    [Fact]
    public void DisposeCleansUpBracketedPasteMode()
    {
        var stdout = new StringWriter();
        var paste = new PasteHandler(stdout);
        paste.Register(_ => { });

        paste.Dispose();

        var output = stdout.ToString();
        Assert.Contains("\u001B[?2004l", output);
    }
}
