using System.Net.Sockets;
using Client.Listeners;
using Client.Models;

namespace Client.Menus;

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
    
    private void HandleSystemCommand(string command)
    {
        switch (command)
        {
            case "exit":
                cts.Cancel();
                break;
            case "clear":
                Console.Clear();
                break;
        }
    }
}