using System.Text.Json;
using ProjektSem1.Models;

namespace ProjektSem1;

public class JsonFiles
{
    private const string MessagesFileName = "messages.json";
    
    /// <summary>
    /// Read all messages from file
    /// </summary>
    /// <returns>list of messages</returns>
    public List<MessageModel> MessagesReadAll()
    {
        var deserialised = JsonSerializer.Deserialize<List<MessageModel>>(File.ReadAllText(MessagesFileName));

        if (deserialised is null)
        {
            return [];
        }

        return deserialised;
    }

    /// <summary>
    /// Write message to file
    /// </summary>
    /// <param name="message">new message object</param>
    public async Task MessagesWrite(MessageModel message)
    {
        var previousMessages = MessagesReadAll();
        
        previousMessages.Add(message);
        
        var serialized = JsonSerializer.Serialize(previousMessages);
        
        await File.WriteAllTextAsync(MessagesFileName, serialized);
    }
}