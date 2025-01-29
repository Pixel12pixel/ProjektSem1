using System.Net.Sockets;
using Client.Listeners;
using Client.Models;

namespace Client.Menus;

/// <summary>
/// Menu for server chat
/// </summary>
/// <param name="cts">Cancellation Token</param>
/// <param name="listener">Listener object</param>
public class ServerChatHandler(CancellationTokenSource cts, ServerListener listener) : ICommandsHandler
{
    public void HandleCommand(string command)
    {
        if (command[0] == '/')
        {
            var message = command[1..];
            
            HandleSystemCommand(message);
        }
        else
        {
            _ = listener.SendMessage(command);
        }
        
        
    }
    
    public void Setup()
    {
        
    }
    
    private void HandleSystemCommand(string command)
    {
        switch (command)
        {
            case "quit":
                cts.Cancel();
                Console.SetCommandsHandler(new MainMenuHandler());
                break;
            case "back":
                cts.Cancel();
                Console.SetCommandsHandler(new MainMenuHandler());
                break;
            case "exit":
                Environment.Exit(0);
                break;
        }
    }
}