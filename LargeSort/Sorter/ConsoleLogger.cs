namespace Sorter;

public static class ConsoleLogger
{
    public static void Write(Action action, ConsoleColor consoleColor){
        var temp = Console.ForegroundColor;
        Console.ForegroundColor =  consoleColor;
        action.Invoke();
        Console.ForegroundColor = temp;
    }
}