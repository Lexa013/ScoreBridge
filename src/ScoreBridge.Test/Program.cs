using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ScoreBridge.Test;

static class Program
{
    static async Task Main()
    {

        // Connect to the server
        TcpClient client = new TcpClient();
        await client.ConnectAsync(IPAddress.Parse("192.168.1.30"), 4001);

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

        string? message;

        while (true)
        {
            using (StreamReader reader = new StreamReader("H:\\serialOutput.txt"))
            {
                while (!reader.EndOfStream)
                {
                    // Read between 4 and 10 characters at a time
                    char[] buffer = new char[new Random().Next(4, 11)];
                    reader.Read(buffer, 0, buffer.Length);

                    // Convert the characters to bytes and send them through the serial port
                    byte[] data = System.Text.Encoding.ASCII.GetBytes(buffer);

                    await stream.WriteAsync(data);
                }
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
}