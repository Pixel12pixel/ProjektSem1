using ProjektSem1.Listeners;
using ProjektSem1.Models;

namespace ProjektSem1;

class Program
{
    private static void Main()
    {
        Console.ConsoleStartUp();
        
        var tcpListener = new SocketTcpListener(40129, new CancellationTokenSource());
        
        _ = tcpListener.TcpListenerAsync();
        
        Task.Delay(-1).Wait();
    }
}