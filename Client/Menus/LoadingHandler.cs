using Client.Models;

namespace Client.Menus;

/// <summary>
/// Menu for loading data
/// </summary>
public class LoadingHandler : ICommandsHandler
{
    public void HandleCommand(string command)
    {
    }
    
    public void Setup()
    {
        Console.AddLine("Loading...");
    }
}