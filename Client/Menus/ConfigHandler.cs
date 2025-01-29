using Client.Models;

namespace Client.Menus;

/// <summary>
/// Menu for setting up the config
/// </summary>
/// <param name="config">config object</param>
/// <param name="jsonFiles">JsonFiles object</param>
public class ConfigHandler(ConfigModel config, JsonFiles jsonFiles) : ICommandsHandler
{
    private int _state = 0;
    
    public void HandleCommand(string command)
    {
        if (command[0] == '/')
        {
            var message = command[1..];
            
            HandleSystemCommand(message);
        }
        else
        {
            switch (_state)
            {
                case 0:
                    config.Username = command;
                    Console.Clear();
                    Console.AddLine("==[CONFIG]==");
                    Console.AddLine("Enter message count:");
                    _state++;
                    break;
                case 1:
                    config.InitMessages = int.Parse(command);
                    config.IsSet = true;
                    jsonFiles.WriteConfig(config);
                    Console.SetCommandsHandler(new MainMenuHandler());
                    break;
            }
        }
        
        
    }
    
    public void Setup()
    {
        Console.AddLine("==[CONFIG]==");
        Console.AddLine("Enter username:");
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