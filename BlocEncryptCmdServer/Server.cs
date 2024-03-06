using System.Net.Sockets;
using System.Net;
using System.Text;
using BlocEncryptCmdServer;

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

var enc = new ServerEncryptor(config.GetValue(ConfigKey.ChatSecret) ?? "ГАММА");

IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("127.0.0.1");
IPAddress ipAddress = ipHostInfo.AddressList.First(address => !address.IsIPv6LinkLocal);
IPEndPoint ipEndPoint = new(ipAddress, 11_000);

Console.WriteLine("Адрес сервера: " + ipEndPoint.ToString());

using Socket listener = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

listener.Bind(ipEndPoint);
listener.Listen(100);

var handler = await listener.AcceptAsync();

var t1 = new Thread(new ThreadStart(ServerSender));
var t2 = new Thread(new ThreadStart(ServerReceiver));
t1.Start();
t2.Start();

while (t2.IsAlive && t1.IsAlive)
{
    await Task.Delay(1000);
}

t1.Interrupt();
t2.Interrupt();

void ServerSender()
{
    while (true)
    {
        var message = Console.ReadLine();
        if (message == null)
        {
            continue;
        }

        try
        {
            message = enc.Encrypt(message);
            var echoBytes = Encoding.UTF8.GetBytes(message);
            handler.Send(echoBytes, 0);
        }
        catch
        {
            break;
        }
    }
}

void ServerReceiver()
{
    while (true)
    {
        // Receive message.
        var buffer = new byte[1_024];
        try
        {
            var received = handler.Receive(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            Console.WriteLine($"Клиент шифр.: {response}");
            response = enc.Decrypt(response);
            Console.WriteLine($"Клиент: {response}");
        }
        catch
        {
            break;
        }
    }
}
