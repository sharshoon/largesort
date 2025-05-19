namespace Sorter;

public static class ConsoleLogger
{
    public static void Write(Action action, ConsoleColor consoleColor){
        var temp = Console.ForegroundColor;
        Console.ForegroundColor =  consoleColor;
        action.Invoke();
        Console.ForegroundColor = temp;
    }
    
    public static void PrintElapsedTime(TimeSpan elapsed, string messagePrefix)
    {
        var elapsedTime =
            $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
        Write(() =>
        {
            Console.WriteLine(messagePrefix + ": " + elapsedTime);
        }, ConsoleColor.Blue);
    }
}