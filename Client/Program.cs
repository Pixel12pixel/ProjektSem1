using Client.Listeners;
using Client.Menus;
using Client.Models;

namespace Client;

internal class Program
{
    static void Main()
    {
        MainMenuHandler mainMenu = new MainMenuHandler();
        
        Console.MainSetup();

        Console.SetCommandsHandler(mainMenu);
        
        ConfigModel config = new ConfigModel();
        config.IsSet = true;
        config.Username = "test";
        config.InitMessages = 10;
        
        ServerListener listener = new ServerListener(config);

        listener.Connect(1234, "::1");
        
        Task.Delay(-1).Wait();
    }

    private static void MainSetup()
    {
        var jsonFiles = new JsonFiles();

        var config = jsonFiles.ReadConfig();

        if (config.IsSet == false)
        {
            
        }
        else
        {
            
        }
    }
}