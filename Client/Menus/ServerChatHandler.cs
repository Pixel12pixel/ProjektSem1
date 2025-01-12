using System.Net.Sockets;
using Client.Listeners;
using Client.Models;

namespace Client.Menus;

public class ServerChatHandler : ICommandsHandler
{
    private readonly ServerListener _listener;
    
    private readonly CancellationTokenSource _cts;
    
    public ServerChatHandler(CancellationTokenSource cts, ServerListener listener)
    {
        _cts = cts;
        
        _listener = listener;
    }

    public void HandleCommand(string command)
    {
        if (command[0] == '/')
        {
            var message = command[1..];
            
            
        }
        else
        {
            _listener.SendMessage(command);
        }
        
        
    }
    
    private void HandleSystemCommand(string command)
    {
        switch (command)
        {
            case "exit":
                _cts.Cancel();
                break;
            case "clear":
                Console.Clear();
                break;
        }
    }
}