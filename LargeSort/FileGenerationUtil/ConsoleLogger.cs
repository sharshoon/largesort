namespace FileGenerationUtil;

public static class ConsoleLogger
{
    public static void Write(Action action, ConsoleColor consoleColor){
        var temp = Console.ForegroundColor;
        Console.ForegroundColor = consoleColor;
        action.Invoke();
        Console.ForegroundColor = temp;
    }
    
    public static void LogSuccess(string message)
    {
        Write(() => Console.WriteLine(message), ConsoleColor.Green);
    }
    
    public static void LogError(string message)
    {
        Write(() => Console.WriteLine(message), ConsoleColor.Red);
    }
    
    public static void LogWarning(string message)
    {
        Write(() => Console.WriteLine(message), ConsoleColor.Yellow);
    }
    
    public static void LogInfo(string message)
    {
        Write(() => Console.WriteLine(message), ConsoleColor.Blue);
    }
}
