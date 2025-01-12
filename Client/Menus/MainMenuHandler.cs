using Client.Models;

namespace Client.Menus;

public class MainMenuHandler : ICommandsHandler
{
    public void HandleCommand(string command)
    {
        if (command[0] == '/')
        {
            var message = command[1..];
            
            
        }
        else
        {
            //_ = _listener.SendMessage(message);
        }
        
        
    }
}