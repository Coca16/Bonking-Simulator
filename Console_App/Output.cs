using System;
using System.Threading;
using static System.Console;

namespace Console_App
{
    public static class Output
    {
        public static void WriteMessage( string message, ConsoleColor? background = null, ConsoleColor? foreground = null)
        {
            if (background != null) { BackgroundColor = background.Value; }
            if (foreground != null) { ForegroundColor = foreground.Value; }
            WriteLine(message);
            ResetColor();
        }
        
        public static void LoadingWriter(string message)
        {
            Write(message);
            int times = 0;
            for (int i = 0; i < 3; i++)
            {
                Write(".");
                Thread.Sleep(500);
                if (i != 2) continue;
                Write("\r".PadRight(message.Length) + "\r" + message);
                i = -1;
                Thread.Sleep(500);
                if (times == 2) break;
                times++;
            }
            WriteLine();
        }
    }
}