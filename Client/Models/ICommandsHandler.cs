namespace Client.Models;

public interface ICommandsHandler
{
    public void HandleCommand(string command)
    {
    }
    
    public void Setup()
    {
    }
}