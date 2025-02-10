using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Services.ConnectionClientController
{
    /// <summary>
    /// Provides an abstraction for network communication with receiver
    /// </summary>
    public interface IConnectionController
    {
        /// <summary>
        /// Connects to the specified host and port
        /// </summary>
        /// <param name="host">The host address</param>
        /// <param name="port">The port number</param>
        /// <returns>A task representing the state of async operation</returns>
        Task ConnectAsync(IPAddress host, int port = 50000);

        /// <summary>
        /// Disconnects from the host
        /// </summary>
        /// <returns>A task representing the state of async operation</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Sends data to the host
        /// </summary>
        /// <param name="data">The data to send</param>
        /// <returns>A task representing the state of async operation</returns>
        Task SendAsync(byte[] data);

        /// <summary>
        /// Receives a complete message from the TCP stream
        /// The message consists of a 2‑byte header followed by the payload
        /// </summary>
        /// <returns></returns>
        Task<byte[]> ReceiveAsync();

        /// <summary>
        /// Represents client connection status
        /// </summary>
        bool IsConnected { get; }
    }
}
