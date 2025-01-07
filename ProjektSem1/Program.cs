using ProjektSem1.Listeners;

namespace ProjektSem1;

class Program
{
    private static void Main()
    {
        Console.ConsoleStartUp();

        var tcpListener = new SocketTcpListener(1234, new CancellationTokenSource());
        
        _ = tcpListener.TcpListenerAsync();
        
        Task.Delay(-1).Wait();
    }
}