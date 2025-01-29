using System.Text.Json;
using Client.Models;

namespace Client;

public class JsonFiles
{
    private const string ConfigFile = "config.json";
    
    private const string ServersFile = "servers.json";

    private readonly string _localDirectory = Directory.GetCurrentDirectory();


    /// <summary>
    /// Read config data from json file
    /// </summary>
    /// <returns>Config object</returns>
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

    
    /// <summary>
    /// Write config data to json file (overwrites existing data)
    /// </summary>
    /// <param name="config">config object</param>
    public void WriteConfig(ConfigModel config)
    {
        var directory = Path.Combine(_localDirectory, ConfigFile);

        var json = JsonSerializer.Serialize(config);
        
        File.WriteAllText(directory, json);
    }
    
    
    /// <summary>
    /// Read servers data from json file
    /// </summary>
    /// <returns>List of saved servers</returns>
    public List<ServerModel> ReadServers()
    {
        var directory = Path.Combine(_localDirectory, ServersFile);

        List<ServerModel> servers;
        
        try
        {
            var json = File.ReadAllText(directory);
        
            servers = JsonSerializer.Deserialize<List<ServerModel>>(json) ?? [];
        }
        catch (Exception e)
        {
            //System.Console.WriteLine(e);

            return [];
        }
        
        return servers!;
    }

    
    /// <summary>
    /// Write new server data to json file
    /// </summary>
    /// <param name="server">Server object</param>
    public void WriteServers(ServerModel server)
    {
        var directory = Path.Combine(_localDirectory, ServersFile);

        var servers = ReadServers();
        
        servers.Add(server);
        
        var json = JsonSerializer.Serialize(servers);
        
        File.WriteAllText(directory, json);
    }
}