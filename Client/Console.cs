using Client.Menus;
using Client.Models;
using static System.Console;

namespace Client;

public static class Console
{
    private static List<string>? _lines;

    private static string? _command;
    
    private static readonly object? ConsoleLock = new();

    private static readonly object? ListLock = new();
    
    private static ICommandsHandler? _commandsHandler;

    /// <summary>
    /// Main console setup
    /// </summary>
    public static void MainSetup()
    {
        lock (ListLock!)
        {
            _lines = [];
        }
        
        _command = string.Empty;
        
        ConsoleSetup();
        
        UpdateConsole();
    }

    /// <summary>
    /// Async Tasks for console
    /// </summary>
    private static void ConsoleSetup()
    {
        Task.Run(ResizeTask);

        Task.Run(InputHandler);
    }

    /// <summary>
    /// Resize content in console
    /// </summary>
    /// <returns></returns>
    private static Task ResizeTask()
    {
        var lastHeight = -1;

        while (true)
        {
            Task.Delay(100).Wait();

            var height = WindowHeight;
            
            if (lastHeight == height)
                continue;
            
            UpdateConsole();
            
            lastHeight = height;
        }
        // ReSharper disable once FunctionNeverReturns
    }

    /// <summary>
    /// Handle input from user
    /// </summary>
    /// <returns></returns>
    private static Task InputHandler()
    {
        while (true)
        {
            ConsoleKeyInfo key;
            
            do
            {
                key = ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (_command!.Length > 0)
                        {
                            _command = _command.Remove(_command.Length - 1);
                            
                            lock (ConsoleLock!)
                            {
                                SetCursorPosition(CursorLeft - 1, CursorTop);
                                Write(" ");
                                SetCursorPosition(CursorLeft - 1, CursorTop);
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
                        break;
                    default:
                        _command += key.KeyChar;
                    
                        lock (ConsoleLock!)
                        {
                            Write(key.KeyChar);
                        }
                        break;
                }
            } while (key.Key != ConsoleKey.Enter);
            
            //what to do with command

            if (_command != null) _commandsHandler!.HandleCommand(_command);

            //add command handler from interface based on what async Task is now active in other classes
            
            _command = string.Empty;
            
            UpdateConsole();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    
    /// <summary>
    /// Add new line to console
    /// </summary>
    /// <param name="line">string line to add</param>
    public static void AddLine(string line)
    {
        lock (ListLock!)
        {
            _lines!.Add(line);
        }
        
        UpdateConsole();
    }

    
    /// <summary>
    /// clear all lines from console
    /// </summary>
    public static void Clear()
    {
        lock (ListLock!)
        {
            _lines!.Clear();
        }
        
        UpdateConsole();
    }


    /// <summary>
    /// Set new command handler from interface
    /// </summary>
    /// <param name="commandsHandler">Command Handler</param>
    public static void SetCommandsHandler(ICommandsHandler commandsHandler)
    {
        Clear();
        
        _commandsHandler = commandsHandler;
        
        _commandsHandler.Setup();
    }

    /// <summary>
    /// Update console content
    /// </summary>
    private static void UpdateConsole()
    {
        System.Console.Clear();
        
        var height = WindowHeight;

        lock (ListLock!)
        {
            if (_lines!.Count < height - 2)
            {
                foreach (var t in _lines)
                {
                    lock (ConsoleLock!)
                    {
                        WriteLine(t);
                    }
                }

                for (var i = 1; i < (height - _lines.Count - 1); i++)
                {
                    lock (ConsoleLock!)
                    {
                        WriteLine("");
                    }
                }
                //working only on windows
#pragma warning disable CA1416
                BufferHeight = height;
#pragma warning restore CA1416
            }
            else
            {
#pragma warning disable CA1416
                BufferHeight = _lines.Count + 2; // blank line + command line
#pragma warning restore CA1416
                
                foreach (var t in _lines)
                {
                    lock (ConsoleLock!)
                    {
                        WriteLine(t);
                    }
                }
            }
        
            lock (ConsoleLock!)
            {
                Write($"\n> {_command}"); 
            }
        }
    }
}