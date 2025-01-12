using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using Client.Menus;
using Client.Models;

namespace Client.Listeners;

public class ServerListener
{
    private readonly Cryptography _crypto;
    
    private readonly ConfigModel _config;
    
    private readonly ServerChatHandler _chatHandler;

    private readonly CancellationTokenSource _cts;

    private byte[] serverPublicKey;

    private Socket serverSocket;

    public ServerListener(ConfigModel config)
    {
        _cts = new CancellationTokenSource();
        
        _crypto = new Cryptography(); //TODO: not here becouse of recreating object when connecting
        
        _config = config;
        
        _chatHandler = new ServerChatHandler(_cts, this);
    }

    public async Task<bool> Connect(int port, string ip)
    {
        var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
     
        try
        {
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
            
            serverSocket = socket;
            
            _ = HandleConnection();
        }
        catch (Exception e)
        {
            return false; // cant connect
        }
        
        return true;
    }

    private async Task HandleConnection()
    {
        // auth
        var keyTask = Auth();
        
        var key = keyTask.Result;
        
        serverPublicKey = key;
        
        Console.SetCommandsHandler(_chatHandler);
        
        Console.AddLine("test ok");

        await Initialization2(key);
        
        Console.AddLine("test ok2");
    }

    private async Task<byte[]> Auth()
    {
        const int bufferSize = 1024 * 4;
        
        var buffer = new byte[bufferSize];
        
        
        // send public key
        
        await serverSocket.SendAsync(_crypto.GetPublicKey(), SocketFlags.None);
        
        
        // receive public key
        
        var receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
        
        var key = new byte[receivedBytes];
        
        Array.Copy(buffer, key, receivedBytes);
        
        Array.Clear(buffer);

        
        // send username
        
        await serverSocket.SendAsync(_crypto.RsaEncrypt(Encoding.UTF8.GetBytes(_config.Username), key), SocketFlags.None);

        return key;
    }

    private async Task Initialization(byte[] key)
    {
        const int bufferSize = 1024 * 4;
        
        var buffer = new byte[bufferSize];
        
        
        // send init data
        
        await serverSocket.SendAsync(_crypto.RsaEncrypt(Encoding.UTF8.GetBytes(_config.InitMessages.ToString()), key), SocketFlags.None);
        
        
        // receive init data count
        
        var receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
        
        var count = int.Parse(Encoding.UTF8.GetString(_crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray())));

        Array.Clear(buffer);
        
        
        // receive init data
        // 1. RSA
        // 2. AES

        var messagesListBytes = new byte[1024 * 4 * count];
        
        for (var i = 0; i < count; i++)
        {
            receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
            
            var secret = _crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray());
            
            Array.Clear(buffer);
            
            receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
            
            var message = _crypto.AesDecrypt(secret, buffer.Take(receivedBytes).ToArray());
            
            Array.Copy(message, 0, messagesListBytes, i * 1024 * 4, message.Length);
        }

        var messagesList = Encoding.UTF8.GetString(messagesListBytes);
        
        System.Console.Write(messagesList);
    }

    private async Task Listener(CancellationTokenSource cts)
    {
        const int bufferSize = 1024 * 4;

        var buffer = new byte[bufferSize];
        
        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None, cts.Token);
                
                var secret = _crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray());
                
                Array.Clear(buffer);
                
                receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None, cts.Token);
                
                var messageBytes = _crypto.AesDecrypt(secret, buffer.Take(receivedBytes).ToArray());
                
                Array.Clear(buffer);
                
                var message = JsonSerializer.Deserialize<MessageModel>(Encoding.UTF8.GetString(messageBytes));
                
                Console.AddLine("(" + message!.Username + "): " + message.Message);
            }
        }
        catch (Exception e)
        {
            
        }
    }
    
    public async Task SendMessage(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        var (secret, messageEnc) = _crypto.AesEncrypt(messageBytes);
        
        //TODO: max message size
        
        await serverSocket.SendAsync(_crypto.RsaEncrypt(secret, serverPublicKey), SocketFlags.None);
        
        await serverSocket.SendAsync(messageEnc, SocketFlags.None);
        
    }
    
    private async Task Initialization2(byte[] key)
    {
        const int bufferSize = 1024 * 8;
        
        var buffer = new byte[bufferSize];
        
        
        // send init data
        
        await serverSocket.SendAsync(_crypto.RsaEncrypt(Encoding.UTF8.GetBytes(_config.InitMessages.ToString()), key), SocketFlags.None);
        
        
        // receive init data count
        
        var receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
        
        //
        //
        //

        var test1 = _crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray());

        var test2 = Encoding.UTF8.GetString(test1);
        
        System.Console.SetCursorPosition(0, 0);
        
        System.Console.WriteLine(test2);
        
        var count = int.Parse(test2);
        
        System.Console.WriteLine(count);

        Array.Clear(buffer);
        
        
        // receive init data
        // 1. RSA
        // 2. AES

        var messagesListBytes = new byte[1024 * 4 * count];
        
        for (var i = 0; i < count; i++)
        {
            receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
            
            System.Console.WriteLine(receivedBytes);
            
            var secret = _crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray());
            
            System.Console.WriteLine("secret: " + secret.Length);
            
            Array.Clear(buffer);
            
            receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
            
            System.Console.WriteLine(receivedBytes);
            
            var message = _crypto.AesDecrypt(secret, buffer.Take(receivedBytes).ToArray());
            
            System.Console.WriteLine("message: " + message.Length);
            
            Array.Copy(message, 0, messagesListBytes, i * 1024 * 4, message.Length);
        }

        var messagesList = Encoding.UTF8.GetString(messagesListBytes);
        
        System.Console.Write(messagesList);
    }
}