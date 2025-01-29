namespace ProjektSem1;


/// <summary>
/// Custom Console class
/// </summary>
public static class Console
{
    /// <summary>
    /// Print messages with time and message type
    /// </summary>
    /// <param name="text">message text</param>
    /// <param name="type">message type</param>
    public static void WriteLine(string text, MessagesTypes type = MessagesTypes.SysLog)
    {
        System.Console.Write("[" + DateTime.Now.ToString("HH:mm:ss") + "] ");
        
        MessageMapping.TryGetValue(type, out var typeMessage);
        
        
        
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        
        System.Console.Write(typeMessage);
        
        
        
        System.Console.ResetColor();
        
        System.Console.Write(": ");
        
        
        
        if (ColorMapping.TryGetValue(type, out var consoleColor))
        {
            System.Console.ForegroundColor = consoleColor;
        }
        else
        {
            System.Console.ResetColor();
        }

        System.Console.Write(text + "\n");
        
        
        System.Console.ResetColor();

        if (type == MessagesTypes.SocError || type == MessagesTypes.SysError || type == MessagesTypes.SocFatal || type == MessagesTypes.SysFatal )
        {
            _ = FileError(text, type);
        }
        else
        {
            _ = FileLog(text, type);
        }
        
    }

    /// <summary>
    /// Main filesystem startup
    /// </summary>
    public static void ConsoleStartUp()
    {
        //LOGS
        
        const string folderName = "Logs";
        var currentDirectory = Directory.GetCurrentDirectory();
        var folderPath = Path.Combine(currentDirectory, folderName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            WriteLine($"Folder '{folderName}' not detected, successfully created!", MessagesTypes.SysInfo);
        }
        
        
        //Messages json
        
        const string fileName = "Messages.json";

        if (!File.Exists(fileName))
        {
            File.Create(fileName).Close();
            
            File.WriteAllText(fileName, "[{\"Username\":\"Admin\",\"Message\":\"Hello World\"}]");
            
            WriteLine($"File '{fileName}' not detected, successfully created!", MessagesTypes.SysInfo);
        }

        //other directories and paths
        
        WriteLine("Console startup ready!", MessagesTypes.SysOk);
    }

    /// <summary>
    /// log message with timestamp and message type to file
    /// </summary>
    /// <param name="message">message content</param>
    /// <param name="type">message type</param>
    private static async Task FileLog(string message, MessagesTypes type = MessagesTypes.SysLog)
    {
        const string folderName = "Logs";
        var currentDirectory = Directory.GetCurrentDirectory();
        var folderPath = Path.Combine(currentDirectory, folderName);

        const string fileName = "-log.txt";
        
        MessageMapping.TryGetValue(type, out var typeMessage);
        
        try
        {
            await File.AppendAllTextAsync(Path.Combine(folderPath, (DateTime.Now.ToString("MM-dd") + fileName)),
                "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + typeMessage + ": " + message + "\n");
        }
        catch (Exception e)
        {
            WriteLine($"Error while logging in file: {e.Message}", MessagesTypes.SysFatal);
        }
    }

    /// <summary>
    /// log errors with timestamp and message type to file
    /// </summary>
    /// <param name="message">message content</param>
    /// <param name="type">message type</param>
    private static async Task FileError(string message, MessagesTypes type = MessagesTypes.SysError)
    {
        const string folderName = "Logs";
        var currentDirectory = Directory.GetCurrentDirectory();
        var folderPath = Path.Combine(currentDirectory, folderName);

        const string fileName = "Errors.txt";
        
        MessageMapping.TryGetValue(type, out var typeMessage);
        
        try
        {
            await File.AppendAllTextAsync(Path.Combine(folderPath, fileName),
                "[" + DateTime.Now.ToString("MM/dd-HH:mm:ss") + "] " + typeMessage + ": " + message + "\n");
        }
        catch (Exception e)
        {
            WriteLine($"Error while logging in file: {e.Message}", MessagesTypes.SysFatal); // we have a problem here
        }
    }

    private static readonly Dictionary<MessagesTypes, ConsoleColor> ColorMapping = new Dictionary<MessagesTypes, ConsoleColor>
    {
        { MessagesTypes.SysInfo, ConsoleColor.Cyan },
        { MessagesTypes.SysWarning, ConsoleColor.Yellow },
        { MessagesTypes.SysError, ConsoleColor.Red },
        { MessagesTypes.SysDebug, ConsoleColor.Magenta },
        { MessagesTypes.SysLog, ConsoleColor.Gray },
        { MessagesTypes.SysFatal, ConsoleColor.DarkRed },
        { MessagesTypes.SysOk, ConsoleColor.Green },
        { MessagesTypes.SocInfo, ConsoleColor.Blue },
        { MessagesTypes.SocWarning, ConsoleColor.Yellow },
        { MessagesTypes.SocError, ConsoleColor.Red },
        { MessagesTypes.SocDebug, ConsoleColor.Magenta },
        { MessagesTypes.SocLog, ConsoleColor.Gray },
        { MessagesTypes.SocFatal, ConsoleColor.DarkRed },
        { MessagesTypes.SocOk, ConsoleColor.DarkGreen }
    };
    
    private static readonly Dictionary<MessagesTypes, string> MessageMapping = new Dictionary<MessagesTypes, string>
    {
        { MessagesTypes.SysInfo, "INF" },
        { MessagesTypes.SysWarning, "WRN" },
        { MessagesTypes.SysError, "ERR" },
        { MessagesTypes.SysDebug, "DBG" },
        { MessagesTypes.SysLog, "LOG" },
        { MessagesTypes.SysFatal, "FATAL" },
        { MessagesTypes.SysOk, "LOG" },
        { MessagesTypes.SocInfo, "SOC_INF" },
        { MessagesTypes.SocWarning, "SOC_WRN" },
        { MessagesTypes.SocError, "SOC_ERR" },
        { MessagesTypes.SocDebug, "SOC_DBG" },
        { MessagesTypes.SocLog, "SOC_LOG" },
        { MessagesTypes.SocFatal, "SOC_FATAL" },
        { MessagesTypes.SocOk, "SOC_LOG" },
        { MessagesTypes.Def, "LOG"}
    };
}

public enum MessagesTypes
{
    SysInfo, // cyan
    SysWarning, // yellow
    SysError, // red
    SysDebug, // magenta
    SysLog, // gray
    SysFatal, // dark red
    SysOk, // green
    
    SocInfo, // blue
    SocWarning, // yellow
    SocError, // red
    SocDebug, // magenta
    SocLog, // gray
    SocFatal, // dark red
    SocOk, // dark green
    
    Def // default color
}