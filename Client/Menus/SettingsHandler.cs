using Client.Models;

namespace Client.Menus;

/// <summary>
/// Menu for settings
/// </summary>
/// <param name="config">config object</param>
/// <param name="_jsonFiles">JsonFiles object</param>
public class SettingsHandler(ConfigModel config, JsonFiles _jsonFiles) : ICommandsHandler
{
    private bool _state = false;

    private int _option = -1;
    
    public void Setup()
    {
        Console.AddLine("==[SETTINGS]==");
        Console.AddLine("1. Change username");
        Console.AddLine("2. Change message count");
        Console.AddLine("3. Back");
    }
    
    public void HandleCommand(string command)
    {
        if (command[0] == '/')
        {
            var message = command[1..];
            
            HandleCommands(message);
        }
        else
        {
            if (_state)
            {
                switch (_option)
                {
                    case 0:
                        config.Username = command;
                        _jsonFiles.WriteConfig(config);
                        break;
                    case 1:
                        config.InitMessages = int.Parse(command);
                        _jsonFiles.WriteConfig(config);
                        break;
                }  
                
                _option = -1;
                
                _state = false;
                
                Console.SetCommandsHandler(new SettingsHandler(config, _jsonFiles));
            }
            else
            {
                switch (command)
                {
                    case "1":
                        Username();
                        break;
                    case "2":
                        MessageCount();
                        break;
                    case "3":
                        Console.SetCommandsHandler(new MainMenuHandler());
                        break;
                }
            }
        }
    }

    private void HandleCommands(string message)
    {
        switch (message)
        {
            case "back":
                Console.SetCommandsHandler(new MainMenuHandler());
                break;
            case "exit":
                Environment.Exit(0);
                break;
        }
    }

    private void MessageCount()
    {
        Console.Clear();
        
        Console.AddLine("Enter new message count:");
        
        _state = true;
        
        _option = 1;
    }
    
    private void Username()
    {
        Console.Clear();
        
        Console.AddLine("Enter new username:");
        
        _state = true;
        
        _option = 0;
    }
}