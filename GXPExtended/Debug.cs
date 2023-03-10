using System;

public class Debug
{
    public static void Log(string message)
    {
        string[] values = message.Split(' ');
        foreach (string value in values)
        {
            if (bool.TryParse(value, out bool boolResult))
            {
                Console.ForegroundColor = boolResult ? ConsoleColor.Green : ConsoleColor.DarkRed;
                Console.Write(boolResult + " ");
                Console.ResetColor();
            }
            else if (int.TryParse(value, out int intResult))
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(intResult + " ");
                Console.ResetColor();
            }
            else if (float.TryParse(value, out float floatResult))
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write(floatResult + " ");
                Console.ResetColor();
            }
            else
            {
                Console.Write(value + " ");
            }
        }
        Console.WriteLine("");
    }
}