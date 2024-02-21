using System.Net.Sockets;
using System.Net;
using System.Text;

IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("127.0.0.1");
IPAddress ipAddress = ipHostInfo.AddressList.Where(address => !address.IsIPv6LinkLocal).First();
IPEndPoint ipEndPoint = new(ipAddress, 11_000);

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

            Console.WriteLine($"Севрер: {response}");
        }
        catch
        {
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
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _ = client.Send(messageBytes, SocketFlags.None);
        }
        catch
        {
            break;
        }
    }
}
