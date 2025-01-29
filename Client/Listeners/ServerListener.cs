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

    public ServerListener(ConfigModel config, Cryptography crypto)
    {
        _cts = new CancellationTokenSource();

        _crypto = crypto;
        
        _config = config;
        
        _chatHandler = new ServerChatHandler(_cts, this);
    }

    /// <summary>
    /// Connect to server
    /// </summary>
    /// <param name="port">server port</param>
    /// <param name="ip">server ip</param>
    /// <returns>if connected</returns>
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

    /// <summary>
    /// Handle new connection
    /// </summary>
    private async Task HandleConnection()
    {
        // auth
        var keyTask = Auth();
        
        var key = keyTask.Result;
        
        serverPublicKey = key;
        
        Console.SetCommandsHandler(_chatHandler);
        
        Console.AddLine("[Connected to server]");

        await Initialization(key);
        
        _ = Listener(_cts);
    }

    /// <summary>
    /// Authenticate to server
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Initialization of the connection, receiving messages
    /// </summary>
    /// <param name="key"></param>
    private async Task Initialization(byte[] key)
    {
        const int bufferSize = 1024 * 4;
        
        var buffer = new byte[bufferSize];
        
        
        // send init data

        try
        {
            await serverSocket.SendAsync(_crypto.RsaEncrypt(Encoding.UTF8.GetBytes(_config.InitMessages.ToString()), key), SocketFlags.None);
        
        
            // receive init data count
        
            var receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
        
            var count = int.Parse(Encoding.UTF8.GetString(_crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray())));

            Array.Clear(buffer);
        
        
            // receive init data
            // 1. RSA
            // 2. AES

            var messagesListBytes = new byte[1024 * 4 * (count - 1)];

            byte[] messageListBytesLast = new byte[] { };
        
            for (var i = 0; i < count; i++)
            {
                receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
            
                var secret = _crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray());
            
                Array.Clear(buffer);
            
                receivedBytes = await serverSocket.ReceiveAsync(buffer, SocketFlags.None);
            
                var message = _crypto.AesDecrypt(secret, buffer.Take(receivedBytes).ToArray());

                if (i == count - 1)
                {
                    messageListBytesLast = message;    
                }
                else
                {
                    Array.Copy(message, 0, messagesListBytes, i * 1024 * 4, message.Length);
                }
            }

            var messagesBuffer = new byte[messagesListBytes.Length + messageListBytesLast.Length];
        
            Array.Copy(messagesListBytes, 0, messagesBuffer, 0, messagesListBytes.Length);
        
            Array.Copy(messageListBytesLast, 0, messagesBuffer, messagesListBytes.Length, messageListBytesLast.Length);
        
            var messagesList = Encoding.UTF8.GetString(messagesBuffer);
            
            var messages = JsonSerializer.Deserialize<List<MessageModel>>(messagesList);

            if (messages != null)
                foreach (var message in messages)
                {
                    Console.AddLine("(" + message.Username + "): " + message.Message);
                }
        }
        catch (Exception e)
        {
            //System.Console.WriteLine(e);
        }
    }

    /// <summary>
    /// Normal listener for incoming messages
    /// </summary>
    /// <param name="cts"></param>
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

        try
        {
            await serverSocket.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
            throw;
        }
    }
    
    /// <summary>
    /// send message to server
    /// </summary>
    /// <param name="message"></param>
    public async Task SendMessage(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        var (secret, messageEnc) = _crypto.AesEncrypt(messageBytes);
        
        //TODO: max message size
        
        await serverSocket.SendAsync(_crypto.RsaEncrypt(secret, serverPublicKey), SocketFlags.None);
        
        await serverSocket.SendAsync(messageEnc, SocketFlags.None);
        
    }
}