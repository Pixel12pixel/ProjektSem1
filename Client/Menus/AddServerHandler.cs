using System.Data;
using Client.Models;

namespace Client.Menus;

/// <summary>
/// Menu for adding a server
/// </summary>
/// <param name="_jsonFiles">JsonFiles Object</param>
public class AddServerHandler(JsonFiles _jsonFiles) : ICommandsHandler
{
    private int _state = 0;
    
    private ServerModel _server = new();
    
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
                    // Server IP
                    _server.ServerIp = command;
                    Console.Clear();
                    Console.AddLine("Enter server port:");
                    _state++;
                    break;
                case 1:
                    // Server port
                    _server.ServerPort = int.Parse(command);
                    Console.Clear();
                    Console.AddLine("Enter server name:");
                    _state++;
                    break;
                case 2:
                    // Server name
                    _server.ServerName = command;
                    _jsonFiles.WriteServers(_server);
                    Console.SetCommandsHandler(new ServerListHandler());
                    break;
            }
        }
    }
    
    

    private void HandleSystemCommand(string command)
    {
        switch (command)
        {
            case "back":
                Console.SetCommandsHandler(new ServerListHandler());
                break;
            case "exit":
                Environment.Exit(0);
                break;
        }
    }

    public void Setup()
    {
        Console.AddLine("Enter server IP:");
    }
}