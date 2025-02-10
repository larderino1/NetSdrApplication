using System.Collections.Generic;
using System;
using Microsoft.Extensions.DependencyInjection;
using NetSdrApplication.Services.ConnectionClientController;
using NetSdrApplication.Services.NetSdrClient;
using System.Net;

namespace NetSdrApplication
{
    /// <summary>
    /// Main class of application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            // Configure services
            using ServiceProvider serviceProvider = ConfigureServices();

            // Retrieve the NetSdrClient instance from DI.
            var netSdrClient = serviceProvider.GetRequiredService<INetSdrClient>();

            try
            {
                // Connect to localhost over TCP
                Console.WriteLine("Connecting to 127.0.0.1...");
                await netSdrClient.ConnectAsync("127.0.0.1");

                // Set receiver state to start IQ and start receiving packages over UDP
                Console.WriteLine("Starting IQ transmission...");
                await netSdrClient.SetReceiverState(Models.Enums.Code.StartIQ);

                // Simulate setting frequency 14,200 MHz
                Console.WriteLine("Setting frequency to 14,200,000 Hz...");
                await netSdrClient.SetTargetFrequency(14200000);

                Console.WriteLine("Press any key to stop IQ transmission...");
                Console.ReadKey();

                // Stop IQ transmission and stop receiving packages over UDP
                Console.WriteLine("Stopping IQ transmission...");
                await netSdrClient.SetReceiverState(Models.Enums.Code.StopIQ);

                // Disconnect from the receiver.
                Console.WriteLine("Disconnecting...");
                await netSdrClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Setup DI container
        /// </summary>
        private static ServiceProvider ConfigureServices() => 
            new ServiceCollection()
                .AddSingleton<IConnectionController, TcpConnectionController>()
                .AddSingleton<INetSdrClient, NetSdrClient>()
                .BuildServiceProvider();
    }
}
