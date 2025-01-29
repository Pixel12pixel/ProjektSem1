using Client.Models;

namespace Client.Menus;

/// <summary>
/// Main menu handler
/// </summary>
public class MainMenuHandler : ICommandsHandler
{
    private readonly ConfigModel _config;
    
    private readonly JsonFiles _jsonFiles;
    
    public MainMenuHandler()
    {
        _jsonFiles = new JsonFiles();
        
        _config = _jsonFiles.ReadConfig();
    }
    
    public void Setup()
    {
        Console.AddLine("==[MAIN MENU]==");
        Console.AddLine("1. Connect to server");
        Console.AddLine("2. Settings");
        Console.AddLine("3. Exit");
    }


    public void HandleCommand(string command)
    {
        if (command[0] == '/')
        {
            var message = command[1..];
            
            HandleSystemCommand(message);
            
        }
        else
        {
            switch (command)
            {
                case "1":
                    Console.SetCommandsHandler(new ServerListHandler());
                    break;
                case "2":
                    Console.SetCommandsHandler(new SettingsHandler(_config, _jsonFiles));
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
            }
        }
        
        
    }
    
    private void HandleSystemCommand(string command)
    {
        switch (command)
        {
            case "exit":
                Environment.Exit(0);
                break;
        }
    }
}