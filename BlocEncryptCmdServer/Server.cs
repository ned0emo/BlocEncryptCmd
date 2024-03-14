using System.Net.Sockets;
using System.Net;
using System.Text;
using BlocEncryptCmdServer;
using System.Text.Json;

var config = new ServerConfig();

try
{
    await config.LoadConfig();
}
catch (Exception ex)
{
    Console.WriteLine("Ошибка загрузки настроек:");
    Console.WriteLine(ex.Message);
}


bool alwaysStayAlive = true;

IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("127.0.0.1");
IPAddress ipAddress = ipHostInfo.AddressList.First(address => !address.IsIPv6LinkLocal);

Console.WriteLine("Адрес сервера: " + ipAddress.ToString());
List<Socket> sockets = [];
List<Socket> bannedSockets = [];

const int connNum = 2;
for (int i = 0; i < connNum; i++)
{
    IPEndPoint ipEndPoint = new(ipAddress, 11_000 + i);
    Socket listener = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    listener.Bind(ipEndPoint);
    listener.Listen(100);

    var handler = listener.AcceptAsync();

    var t2 = new Thread(new ParameterizedThreadStart(ServerReceiver));
    t2.Start(handler);
}

string addUsersCheck = "";
do
{
    Console.WriteLine("Добавить еще пользователя? 1 - да, иначе - нет");
    addUsersCheck = Console.ReadLine() ?? "";

    if (addUsersCheck != "1") break;

    await config.AddUser();
} while (true);

do
{
    await Task.Delay(1000);
} while (alwaysStayAlive || sockets.Count > 0);

void SendMessage(string message, params Socket[] sockets)
{
    try
    {
        var echoBytes = Encoding.UTF8.GetBytes(message);
        foreach (var socket in sockets)
        {
            socket.Send(echoBytes, 0);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

async void ServerReceiver(object? handler)
{
    Socket socket;

    if (handler is Task<Socket> skt)
    {
        socket = await skt;
        sockets.Add(socket);
    }
    else
    {
        return;
    }

    while (true)
    {
        var buffer = new byte[1_024];
        try
        {
            var received = socket.Receive(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            try
            {
                var messageData = JsonSerializer.Deserialize<MessageS>(response);
                if (messageData != null && config.CheckUser(messageData.Username, messageData.Secret))
                {
                    SendMessage(response, sockets.Where(s => s != socket && s.Connected).ToArray());
                }
                else
                {
                    throw new Exception("Неверные данные пользователя");
                }
            }
            catch {
                throw;
            }
            //response = enc.Decrypt(response);
        }
        catch (Exception ex)
        {
            socket.Dispose();
            sockets.Remove(socket);
            Console.WriteLine(ex.Message);
            alwaysStayAlive = false;
            break;
        }
    }
}
