namespace NuGetPackageExplorer.Utilities;

public static class InputHelper
{
    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(value)) return value;
            Console.WriteLine("Please enter a value.");
        }
    }

    public static int ReadNumber(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
            {
                return value;
            }
            Console.WriteLine($"Enter a number from {min} to {max}.");
        }
    }

    public static void Pause()
    {
        Console.WriteLine("\nPress Enter to continue.");
        Console.ReadLine();
    }
}
