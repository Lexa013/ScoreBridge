using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {

        // Connect to the server
        TcpClient client = new TcpClient();
        await client.ConnectAsync(IPAddress.Parse("192.168.1.30"), 1868);

        // Start tasks to send and receive data
        Task receiveTask = ReceiveDataAsync(client);
        Task sendTask = SendDataAsync(client);

        // Wait for both tasks to complete
        await Task.WhenAll(receiveTask, sendTask);

        // Close the connection
        client.Close();
    }

    static async Task ReceiveDataAsync(TcpClient client)
    {
        byte[] buffer = new byte[1024];
        int bytesRead;
        NetworkStream stream = client.GetStream();

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received message: " + message);
        }
    }

    static async Task SendDataAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        while (true)
        {
            string message = Console.ReadLine();
            byte[] data = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}