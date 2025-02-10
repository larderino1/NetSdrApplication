using NetSdrApplication.Exceptions;
using NetSdrApplication.Models.ControlItem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Services.ConnectionClientController
{
    /// <summary>
    /// Implements <see cref="IConnectionController"/> using TCP protocol.
    /// </summary>
    public class TcpConnectionController : IConnectionController
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream? _stream;

        /// <summary>
        /// Represents connection state
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Constructor for <see cref="TcpConnectionController"/>
        /// </summary>
        public TcpConnectionController()
        {
            _tcpClient = new TcpClient();
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(IPAddress host, int port = 50000)
        {
            try
            {
                await _tcpClient.ConnectAsync(host, port);

                _stream = _tcpClient.GetStream();
                Console.WriteLine($"Connection successful on {host.ToString()}:{port}");

                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw new ConnectionException($"Failed to Connect from server, reason : {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync()
        {
            try
            {
                if (!IsConnected)
                    return;

                if(_stream != null)
                {
                    _stream.Close();
                    await _stream.DisposeAsync();
                }


                _tcpClient.Client.Close();
                _tcpClient.Close();
                _tcpClient.Dispose();

                IsConnected = false;

                Console.WriteLine("Successfully disconnected");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new ConnectionException("Failed to disconnect from server, reason : ", ex);
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync(byte[] data)
        {
            try
            {
                if (_stream == null || !_stream.CanWrite)
                    throw new ConnectionException("Failed to connect to , reason : ");

                await _stream.WriteAsync(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReceiveAsync()
        {
            if (_stream == null)
                throw new ConnectionException("Not connected.");

            // Read exactly 2 bytes for the header.
            byte[] headerBytes = await ReadExactAsync(_stream, 2);
            Header header = Header.FromBytes(headerBytes);

            int payloadLength = header.Length - 2; // Total length minus header
            byte[] payload = payloadLength > 0
                ? await ReadExactAsync(_stream, payloadLength)
                : [];

            byte[] fullMessage = new byte[header.Length];
            Buffer.BlockCopy(headerBytes, 0, fullMessage, 0, headerBytes.Length);
            Buffer.BlockCopy(payload, 0, fullMessage, headerBytes.Length, payloadLength);
            return fullMessage;
        }

        /// <summary>
        /// Reads exactly specified number of bytes from provided <see cref="NetworkStream"/>
        /// </summary>
        /// <param name="stream">Network stream from which data will be read</param>
        /// <param name="count">Exact number of bytes to read from the stream</param>
        /// <returns>
        /// A byte array containing exactly <paramref name="count"/> bytes that were read from the stream
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ends before the specified number of bytes could be read
        /// </exception>
        private static async Task<byte[]> ReadExactAsync(NetworkStream stream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, count - offset));
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream.");
                }
                offset += bytesRead;
            }
            return buffer;
        }
    }
}
