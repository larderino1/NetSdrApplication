using NetSdrApplication.Models.ControlItem;
using NetSdrApplication.Models.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Services.NetSdrClient
{
    /// <summary>
    /// Interface for NetSdr instance
    /// </summary>
    public interface INetSdrClient
    {
        /// <summary>
        /// Connects to NetSDR receiver using provided IP address and port
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="port">TCP port number (default is 50000)</param>
        /// <returns>A Task representing state of async operation</returns>
        Task ConnectAsync(string ip, int port = 50000);

        /// <summary>
        /// Disconnects from NetSDR receiver
        /// </summary>
        /// <returns>A Task representing state of async operation</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Sets receiver state (start or stop IQ transmission)
        /// After processing response, starts or stops UDP based on received code
        /// </summary>
        /// <param name="controlItemCode">Control item code <see cref="Code"/></param>
        /// <param name="channelSpecifier">Data channel/type specifier (default: 0x80)</param>
        /// <param name="captureMode">Capture mode (default: 0x80)</param>
        /// <param name="fifoCount">FIFO count (default: 0x00)</param>
        /// <returns>A Task representing state of async operation</returns>
        Task SetReceiverState(Code controlItemCode, byte channelSpecifier = 0x80, byte captureMode = 0x80, byte fifoCount = 0x00);

        /// <summary>
        /// Sets receiver frequency by converting given frequency into a 5-byte array
        /// and sending it along with channel identifier
        /// </summary>
        /// <param name="frequency"> frequency value (in Hz)</param>
        /// <param name="channelId"> channel identifier (default: 0xFF)</param>
        /// <returns>A Task representing   async operation</returns>
        Task SetTargetFrequency(ulong frequency, byte channelId = 0xFF);
    }
}
