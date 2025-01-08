using System.Text.Json;
using Client.Models;

namespace Client;

public class JsonFiles
{
    private const string ConfigFile = "config.json";

    private readonly string _localDirectory = Directory.GetCurrentDirectory();


    public ConfigModel ReadConfig()
    {
        var directory = Path.Combine(_localDirectory, ConfigFile);

        var config = new ConfigModel();
        
        try
        {
            var json = File.ReadAllText(directory);
        
            config = JsonSerializer.Deserialize<ConfigModel>(json);
        }
        catch (Exception e)
        {
            //System.Console.WriteLine(e);
        }
        
        return config!;
    }


}