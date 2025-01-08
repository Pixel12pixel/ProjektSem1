using System.Net;
using System.Net.Sockets;

namespace Client.Listeners;

public class ServerListener
{
    
    public static void ListenForMessages()
    {
        Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        
        socket.Connect(new IPEndPoint(IPAddress.Parse("[::]"), 1234));
        
        System.Console.WriteLine("ok");
    }
    
}