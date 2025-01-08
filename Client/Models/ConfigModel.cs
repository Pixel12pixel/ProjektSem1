namespace Client.Models;

public class ConfigModel
{
    public string Username { get; set; } = string.Empty;
    
    public int InitMessages { get; set; }

    public List<ServerModel> Servers { get; set; } = [];

    public bool IsSet { get; set; } = false;
}