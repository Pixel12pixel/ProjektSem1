namespace Client;

public class Messages
{
    public void WriteMenu()
    {
        
    }

    public void WriteConfig()
    {
        
        
        
    }

    public void WriteServersList()
    {
        var jsonFiles = new JsonFiles();

        var config = jsonFiles.ReadConfig();

        int it = 1;
        
        foreach (var server in config.Servers)
        {
            Console.AddLine(it.ToString() + ". " + server.ServerName);

            it++;
        }
        
    }
}