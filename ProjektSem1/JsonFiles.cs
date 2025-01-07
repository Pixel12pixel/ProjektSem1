using System.Text.Json;
using ProjektSem1.Models;

namespace ProjektSem1;

public class JsonFiles
{
    private const string MessagesFileName = "messages.json";
    
    public List<MessageModel> MessagesReadAll()
    {
        var deserialised = JsonSerializer.Deserialize<List<MessageModel>>(File.ReadAllText(MessagesFileName));

        if (deserialised is null)
        {
            return [];
        }

        return deserialised;
    }

    public async Task MessagesWrite(MessageModel message)
    {
        var previousMessages = MessagesReadAll();
        
        previousMessages.Add(message);
        
        var serialized = JsonSerializer.Serialize(previousMessages);
        
        await File.WriteAllTextAsync(MessagesFileName, serialized);
    }
}