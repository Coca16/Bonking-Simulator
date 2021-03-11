using System;
using static System.Console;
using static  Console_App.Output;

namespace Console_App
{
    public static class Input
    {
        public static bool InputBool(string message = "", ConsoleColor? background = null, ConsoleColor? foreground = null)
        {
            WriteMessage(message, background, foreground);
            while (true)
            {
                switch (ReadLine().ToLower())
                {
                    case "y":
                    case "yes":
                        return true;
                    case "n":
                    case "no":
                        return false;
                    default:
                        WriteLine("Input is neither Y or N!");
                        break;
                }
            }
        }

        public static string InputString(string message = "",
            int? max = null,
            int? min = null,
            string errorMax = "Text is too long!",
            string errorMin = "Text is too short!", 
            ConsoleColor? background = null, ConsoleColor? foreground = null)
        {
            WriteMessage(message, background, foreground);
            string output = ReadLine();
            if (max == null && min == null) return output;
            
            while (!SizeChecker(output.Length, max, min, errorMax, errorMin)) { output = ReadLine(); }
            return output;
        }

        public static int InputInt(string message = "",
            int? max = null,
            int? min = null,
            string errorMax = "Number is too big!",
            string errorMin = "Number is too small!",
            string errorMessage = "The input was not a number. Please try again.",
            ConsoleColor? background = null, ConsoleColor? foreground = null)
        {
            int output =  ReadInt(message, errorMessage, background, foreground);
            if (max == null && min == null) return output;
            
            while (!SizeChecker(output, max, min, errorMax, errorMin)) output = ReadInt(message, errorMessage);
            return output;
        }

        private static bool SizeChecker(int size, int? max, int? min, string errorMax, string errorMin)
        {
            if (size >= max) WriteLine(errorMax);
            if (size <= min) WriteLine(errorMin);
            return !(size >= max) && !(size <= min);
        }

        private static int ReadInt(string message, string error, ConsoleColor? background = null, ConsoleColor? foreground = null)
        {
            WriteMessage(message, background, foreground);
            
            int output;
            while (true)
            {
                try { output = Convert.ToInt16(ReadLine()); }
                catch (Exception e)
                {
                    if (e.Message != "Input string was not in a correct format.") throw;
                    WriteLine(error);
                    continue;
                }
                break;
            }
            return output;
        }
    }
}