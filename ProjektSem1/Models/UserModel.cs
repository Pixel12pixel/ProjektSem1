using System.Net.Sockets;

namespace ProjektSem1.Models;

public class UserModel
{
    public required string Username { get; init; }
    public required byte[] PublicKey { get; init; }
    
    public required Socket Socket { get; init; }
}