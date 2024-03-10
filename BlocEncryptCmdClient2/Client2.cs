using System.Net.Sockets;
using System.Net;
using System.Text;
using BlocEncryptCmdClient2;

var config = new Client2Config();

try
{
    await config.LoadConfig();
}
catch (Exception ex)
{
    Console.WriteLine("Ошибка загрузки настроек:");
    Console.WriteLine(ex.Message);
    Console.ReadKey();
    return;
}

Console.WriteLine("Чат:");

var enc = new Client2Encryptor(config.GetValue(ConfigKey.ChatSecret)!);

IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("127.0.0.1");
IPAddress ipAddress = ipHostInfo.AddressList.First(address => !address.IsIPv6LinkLocal);
IPEndPoint ipEndPoint = new(ipAddress, 11_001);

using Socket client = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

await client.ConnectAsync(ipEndPoint);
var t1 = new Thread(new ThreadStart(ClientSender));
var t2 = new Thread(new ThreadStart(ClientReciever));
t1.Start();
t2.Start();

while (t2.IsAlive && t1.IsAlive)
{
    await Task.Delay(1000);
}

t1.Interrupt();
t2.Interrupt();

client.Shutdown(SocketShutdown.Both);
return;

void ClientReciever()
{
    while (true)
    {
        var buffer = new byte[1_024];
        try
        {
            var received = client.Receive(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            var username = response[0..response.IndexOf('|')];
            var secret = response[(response.IndexOf('|')+1)..response.IndexOf("||")];
            var message = new Client2Encryptor(secret).Decrypt(response[(response.IndexOf("||")+2)..]);

            //response = enc.Decrypt(response);
            Console.WriteLine($"{username}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            client.Shutdown(SocketShutdown.Both);
            Console.ReadKey();
            break;
        }
    }
}

void ClientSender()
{
    while (true)
    {
        var message = Console.ReadLine();
        if (message == null) continue;
        try
        {
            message = $"{config.GetValue(ConfigKey.Username)}|{config.GetValue(ConfigKey.ChatSecret)}||" + enc.Encrypt(message);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _ = client.Send(messageBytes, SocketFlags.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            client.Shutdown(SocketShutdown.Both);
            Console.ReadKey();
            break;
        }
    }
}
