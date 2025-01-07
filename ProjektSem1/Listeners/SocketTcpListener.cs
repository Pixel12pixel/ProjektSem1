using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ProjektSem1.Models;

namespace ProjektSem1.Listeners;

public class SocketTcpListener
{

    private readonly CancellationTokenSource _cts;
    
    private readonly int _port;
    
    private readonly Cryptography _crypto;
    
    private readonly JsonFiles _jsonFiles;
    
    private List<UserModel> _users;

    public SocketTcpListener(int port, CancellationTokenSource token)
    {
        _crypto = new Cryptography();
        
        Console.WriteLine($"Successfully generated cryptography keys!", MessagesTypes.SysOk);
        
        _port = port;
        
        _cts = token;
        
        _jsonFiles = new JsonFiles();
        
        _users = [];
    }

    /// <summary>
    /// Start async tcp listener
    /// </summary>
    public async Task TcpListenerAsync()
    {
        
        var serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        
        serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, _port));
        serverSocket.Listen(2);
        
        Console.WriteLine($"TCP server listening on {serverSocket.LocalEndPoint}", MessagesTypes.SysInfo);

        try
        {
            while (_cts.IsCancellationRequested == false)
            {
                var socketTask = Task.Factory.FromAsync(serverSocket.BeginAccept, serverSocket.EndAccept, null);

                var completedTask = await Task.WhenAny(socketTask, Task.Delay(-1, _cts.Token));

                if (completedTask == socketTask)
                {
                    var clientSocket = socketTask.Result;
                    
                    _ = HandleSocket(clientSocket);
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in TCP Listener!! - {e.Message}", MessagesTypes.SocFatal);
        }
        finally
        {
            serverSocket.Close();
            Console.WriteLine("TCP server closed", MessagesTypes.SocInfo);
        }
    }

    /// <summary>
    /// Handle new connected client
    /// </summary>
    /// <param name="socket">client socket</param>
    private async Task HandleSocket(Socket socket)
    {
        //max connections
        
        
        // Auth
        var auth = SocketAuth(socket); //TODO: check if it work without await

        var user = auth.Result;
        
        _users.Add(user);

        // send history
        await SendHistory(user);
        

        // Listener
        _ = Listener(user);
        
        Console.WriteLine($"Client from: {user.Socket} is now known as: '{user.Username}'. Listener started.",
            MessagesTypes.SocInfo);
    }

    /// <summary>
    /// New client auth
    /// </summary>
    /// <param name="socket">client socket</param>
    /// <returns>UserModel object</returns>
    private async Task<UserModel> SocketAuth(Socket socket)
    {
        const int bufferSize = 1024 * 4;
        
        var buffer = new byte[bufferSize];

        try
        {
            //RSA public key receive
        
            var receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None, _cts.Token);

            if (_cts.IsCancellationRequested)
            {
                return null!; //yyyyyyyyyyyy... ok
            }
        
            var publicKey = new byte[receivedBytes];
        
            Array.Copy(buffer, publicKey, receivedBytes);
        
            Array.Clear(buffer);
        
        
            //RSA public key send
        
            await socket.SendAsync(_crypto.GetPublicKey(), SocketFlags.None);
        
        
            //receive client username
        
            receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None, _cts.Token);
        
            var username = Encoding.UTF8.GetString(_crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray()));

            var user = new UserModel()
            {
                Username = username,
                PublicKey = publicKey,
                Socket = socket
            };
        
            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error from: {socket.RemoteEndPoint}, in auth listener: {e.Message}");
        }

        try
        {
            socket.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error from: {socket.RemoteEndPoint}, in auth listener: {e.Message}",
                MessagesTypes.SocError);
        }
        
        return null!;
    }

    /// <summary>
    /// Socket listener
    /// </summary>
    /// <param name="user">UserModel Object</param>
    private async Task Listener(UserModel user)
    {
        const int bufferSize = 1024 * 4;
        
        var buffer = new byte[bufferSize];
        
        try
        {   
            // 1 - key and nonce [RSA]
            // 2 - tag and message [AES]

            while (true)
            {
                var receivedBytes =  await user.Socket.ReceiveAsync(buffer, SocketFlags.None,
                    new CancellationToken());
            
                var secret = _crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray());
            
                Array.Clear(buffer);
            
                receivedBytes = await user.Socket.ReceiveAsync(buffer, SocketFlags.None, new CancellationToken());
            
                var message = _crypto.AesDecrypt(secret, buffer.Take(receivedBytes).ToArray());
                
                Array.Clear(buffer);

                var messageM = new MessageModel()
                {
                    Message = Encoding.UTF8.GetString(message),
                    Username = user.Username
                };
            
                await _jsonFiles.MessagesWrite(messageM);
                
                await SendMessages(user, Encoding.UTF8.GetString(message));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error from: {user.Socket.RemoteEndPoint}, in listener: {e.Message}",
                MessagesTypes.SocError);
            
            _users.Remove(user);
        }
    }

    /// <summary>
    /// Send message to targer socket/client
    /// </summary>
    /// <param name="userSender">Sender UserModel Object</param>
    /// <param name="msg">message to send</param>
    private async Task SendMessages(UserModel userSender, string msg)
    {
        var message = new MessageModel()
        {
            Username = userSender.Username,
            Message = msg
        };

        var mess = JsonSerializer.Serialize(message);
            
        var (secret, messageBytes) = _crypto.AesEncrypt(mess);
            
        foreach (var user in _users)
        {
            try
            {
                if (user == userSender)
                    continue;
                
                var secEncrypt = _crypto.RsaEncrypt(secret, user.PublicKey);
                
                await user.Socket.SendAsync(secEncrypt, SocketFlags.None);
                
                await user.Socket.SendAsync(messageBytes, SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while sending message with code: {e.Message}", MessagesTypes.SocError);
            }
        }
    }

    /// <summary>
    /// Send chat history to target client/socket
    /// </summary>
    /// <param name="user">Destination UserModel</param>
    private async Task SendHistory(UserModel user)
    {
        const int bufferSize = 1024 * 4;

        var buffer = new byte[bufferSize];

        int messagesCount;

        try
        {
            //how many messages send to client

            var receivedBytes = await user.Socket.ReceiveAsync(buffer, SocketFlags.None, new CancellationToken());

            var countBytes = _crypto.RsaDecrypt(buffer.Take(receivedBytes).ToArray());

            var countString = Encoding.UTF8.GetString(countBytes);

            messagesCount = int.Parse(countString);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while reciving data from {user.Socket.RemoteEndPoint}: {e.Message}",
                MessagesTypes.SocError);
            messagesCount = 0;
        }

        var messages = _jsonFiles.MessagesReadAll();

        if (messagesCount == 0)
            return;

        var history = new List<MessageModel>();

        if (messages.Count < messagesCount)
        {
            messages.CopyTo(history.ToArray(), messages.Count - messagesCount);
        }

        history = messages;

        var segments = ArraySegments(bufferSize, history);

        //TCP transfers count
        //messages

        try
        {
            await user.Socket.SendAsync(_crypto.RsaEncrypt(segments.Count.ToString(), user.PublicKey),
                SocketFlags.None);

            foreach (var seg in segments)
            {
                var (secret, message) = _crypto.AesEncrypt(seg);

                var secretEncrypt = _crypto.RsaEncrypt(secret, user.PublicKey);

                await user.Socket.SendAsync(secretEncrypt, SocketFlags.None);

                await user.Socket.SendAsync(message, SocketFlags.None);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while sending message history to: {user.Socket.RemoteEndPoint}: {e.Message}",
                MessagesTypes.SocError);
        }
    }

    /// <summary>
    /// Segmentation List od messages by max message payload size (- Tag Bytes)
    /// </summary>
    /// <param name="bufferSize">buffer / payload max size</param>
    /// <param name="messages">message list</param>
    /// <returns>List of bytes arrays</returns>
    private List<byte[]> ArraySegments(int bufferSize, List<MessageModel> messages)
    {
        bufferSize = bufferSize - 16; //AES-GCM tag size
        
        var jsonMessages = JsonSerializer.Serialize(messages);
        
        var messageArray = Encoding.UTF8.GetBytes(jsonMessages);
        
        var segmentsCount = (messageArray.Length + bufferSize) / bufferSize;

        var segments = new List<byte[]>();
        
        var segment = new byte[bufferSize];
        
        for (var i = 0; i < segmentsCount; i++)
        {
            if (i + 1 == segmentsCount)
            {
                Array.Copy(messageArray, i * bufferSize, segment, 0,
                    (messageArray.Length - i * bufferSize));

                segments.Add(segment);
            }
            else
            {
                Array.Copy(messageArray, i * bufferSize, segment, 0, bufferSize);

                segments.Add(segment);
            }

            Array.Clear(messageArray);
        }
        
        return segments;
    }
}