using Client.Models;

namespace Client;

internal class Program
{
    static void Main()
    {
        Console.MainSetup();

        
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