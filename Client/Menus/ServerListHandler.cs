using Client.Listeners;
using Client.Models;

namespace Client.Menus;

/// <summary>
/// Menu for server list
/// </summary>
public class ServerListHandler : ICommandsHandler
{
    private List<ServerModel> _servers;
    
    private readonly JsonFiles _jsonFiles;
    
    public ServerListHandler()
    {
        _jsonFiles = new JsonFiles();

        _servers = _jsonFiles.ReadServers();
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
            if (int.TryParse(command, out var serverIndex))
            {
                if (serverIndex == 0)
                {
                    Console.SetCommandsHandler(new AddServerHandler(_jsonFiles));
                }
                else
                {
                    var server = _servers[serverIndex - 1];
                    
                    var listener = new ServerListener(_jsonFiles.ReadConfig(), Program.GetCrypto());
                    
                    Task.Run(() => listener.Connect(server.ServerPort, server.ServerIp));
                    
                    Console.SetCommandsHandler(new LoadingHandler());
                }
            }
        }
    }
    
    private void HandleSystemCommand(string command)
    {
        switch (command)
        {
            case "back":
                Console.SetCommandsHandler(new MainMenuHandler());
                break;
            case "exit":
                Environment.Exit(0);
                break;
        }
    }
    
    public void Setup()
    {
        var iterator = 1;
        
        Console.AddLine("==[SERVERS LIST]==");
        Console.AddLine("0. Add server");
        Console.AddLine(" ");
        foreach (var server in _servers)
        {
            Console.AddLine($"{iterator}. {server.ServerName}");
            iterator++;
        }
    }
}