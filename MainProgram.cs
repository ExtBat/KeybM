internal static class MainProgram
{
    [STAThread]
    private static void Main()
    {
        using var app = new InputRemapperApp();
        app.Run();
    }
}