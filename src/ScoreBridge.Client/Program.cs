using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    public static async Task Main(string[] args)
    {
        var localport = 5659;
        var serverport = 1868;
        var udpClient = new UdpClient(localport);
        
        try
        {
            udpClient.Connect(IPAddress.Parse("192.168.1.30"), serverport);

            byte[] passwordBytes = Encoding.ASCII.GetBytes("pass");
            udpClient.Send(passwordBytes, passwordBytes.Length);

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, localport);
            
            while (true)
            {
                byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                string receivedData = Encoding.ASCII.GetString(receivedBytes);
                Console.WriteLine($"Received data: {receivedData}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
            throw;
        }
        
        
    }
}