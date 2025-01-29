using Client.Listeners;
using Client.Menus;
using Client.Models;

namespace Client;

internal class Program
{
    internal Program()
    {
        MainSetup();
    }

    private static ConfigModel _config;

    private static Cryptography _crypto;
    
    static void Main()
    {
        Console.MainSetup();
        
        Console.SetCommandsHandler(new LoadingHandler());
        
        _crypto = new Cryptography();
        
        MainSetup();
        
        Task.Delay(-1).Wait();
    }

    /// <summary>
    /// setup main functions of the program
    /// </summary>
    private static void MainSetup()
    {
        var jsonFiles = new JsonFiles();

        var config = jsonFiles.ReadConfig();

        if (config.IsSet == false)
        {
            Console.SetCommandsHandler(new ConfigHandler(config, jsonFiles));
        }
        else
        {
            _config = config;
            
            Console.SetCommandsHandler(new MainMenuHandler());
        }
    }
    
    
    /// <summary>
    /// get cryptography instance
    /// </summary>
    /// <returns>Cryptography object</returns>
    public static Cryptography GetCrypto()
    {
        return _crypto;
    }
}