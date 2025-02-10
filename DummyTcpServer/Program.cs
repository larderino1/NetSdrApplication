namespace DummyTcpServer
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    namespace DummyTcpServer
    {
        class Program
        {
            static async Task Main(string[] args)
            {
                TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 50000);
                listener.Start();
                Console.WriteLine("Dummy TCP server started on 127.0.0.1:50000");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected.");
                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }

            private static async Task HandleClientAsync(TcpClient client)
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                try
                {
                    while (true)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        Console.WriteLine($"Received {bytesRead} bytes.");

                        await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error handling client: " + ex.Message);
                }
                finally
                {
                    client.Close();
                    Console.WriteLine("Client disconnected.");
                }
            }

        }
    }

}
