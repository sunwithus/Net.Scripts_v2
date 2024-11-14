namespace MudBlazorWeb2.Components.Methods
{
    public class ConsoleCol
    {
        private static readonly object consoleLock = new object();

        public static void WriteLine(string text, ConsoleColor color)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }

        public static void Write(string text, ConsoleColor color)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ResetColor();
            }
        }
    }
}
