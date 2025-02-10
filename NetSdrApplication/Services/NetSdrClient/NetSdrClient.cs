using NetSdrApplication.Exceptions;
using NetSdrApplication.Models.ControlItem;
using NetSdrApplication.Models.Enums;
using NetSdrApplication.Services.ConnectionClientController;

using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace NetSdrApplication.Services.NetSdrClient
{
    /// <summary>
    /// Implements NetSDR client functionality, including TCP command
    /// operations and UDP data reception for I/Q samples
    /// </summary>
    public class NetSdrClient : INetSdrClient, IDisposable
    {
        private readonly IConnectionController _tcpConnectionController;

        private readonly CancellationTokenSource _cts;

        private Task? _backgroundTask;

        private UdpClient? _udpClient;

        private bool _disposed;

        private readonly string _udpOutputFilePath = "udp_data.bin";

        private Channel<byte[]>? _udpChannel;

        /// <summary>
        /// Constructs a new instance of NetSdrClient
        /// </summary>
        /// <param name="tcpConnectionController">  TCP connection controller</param>
        public NetSdrClient(IConnectionController tcpConnectionController)
        {
            _tcpConnectionController = tcpConnectionController;
            _cts = new CancellationTokenSource();
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(string ip, int port = 50000)
        {
            if (IPAddress.TryParse(ip, out var address))
            {
                await _tcpConnectionController.ConnectAsync(address, port);
                return;
            }

            throw new ArgumentException();
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync()
        {
            try
            {
                await _tcpConnectionController.DisconnectAsync();
            }
            catch (ConnectionException)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SetReceiverState(Code controlItemCode, byte channelSpecifier = 0x80, byte captureMode = 0x80, byte fifoCount = 0x00)
        {
            EnsureConnected();

            byte[] parameters;

            switch (controlItemCode)
            {
                case Code.StartIQ:
                    parameters =
                    [
                        channelSpecifier,   // Parameter 1: Data channel/type specifier (used default value)
                        (byte)Code.StartIQ, // Parameter 2: Run command (0x02 to start)
                        captureMode,        // Parameter 3: Capture mode (used default value)
                        fifoCount           // Parameter 4: FIFO count (used default value)
                    ];
                    break;

                case Code.StopIQ:
                    // Parameters 1,3,4 ignored by the spec
                    parameters =
                    [
                        (byte)Code.Default,
                        (byte)Code.StopIQ,
                        (byte)Code.Default,
                        (byte)Code.Default
                    ];
                    break;

                default:
                    throw new ArgumentException("Invalid control item code for receiver state.", nameof(controlItemCode));
            }

            var message = new ControlItemMessage(controlItemCode, parameters);

            byte[] commandBytes = message.ToBytes();

            await _tcpConnectionController.SendAsync(commandBytes);

            byte[] response = await _tcpConnectionController.ReceiveAsync();

            ProcessResponse(response);

            if (controlItemCode == Code.StartIQ)
            {
                StartUdpReceiver(_udpOutputFilePath);
            }
            else if (controlItemCode == Code.StopIQ)
            {
                await StopUdpReceiverAsync();
            }
        }

        /// <inheritdoc/>
        public async Task SetTargetFrequency(ulong frequency, byte channelId = 0xFF)
        {
            EnsureConnected();

            byte[] frequencyBytes = new byte[5];
            ulong temp = frequency;
            for (int i = 0; i < 5; i++)
            {
                frequencyBytes[i] = (byte)(temp & 0xFF);
                temp >>= 8;
            }
            Array.Reverse(frequencyBytes);

            byte[] parameters = new byte[1 + frequencyBytes.Length];
            parameters[0] = channelId;
            Array.Copy(frequencyBytes, 0, parameters, 1, frequencyBytes.Length);

            ControlItemMessage setFrequencyMessage = new(Code.SetFrequency, parameters);

            byte[] command = setFrequencyMessage.ToBytes();

            await _tcpConnectionController.SendAsync(command);

            byte[] response = await _tcpConnectionController.ReceiveAsync();

            ProcessResponse(response);
        }

        /// <summary>
        /// Disposes of all managed resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _cts.Dispose();

            _tcpConnectionController.DisconnectAsync();

            _disposed = true;
        }

        /// <summary>
        /// Starts UDP receiver to listen on port 60000 for I/Q sample data
        /// Received data is written to specified output file
        /// </summary>
        /// <param name="outputFilePath">  path of   file where UDP data will be written</param>
        private void StartUdpReceiver(string outputFilePath)
        {
            if (_udpClient != null && _udpClient.Client.IsBound)
            {
                throw new InvalidOperationException("UDP Receiver is already running");
            }

            _udpClient = new UdpClient(60000);

            _udpChannel = Channel.CreateUnbounded<byte[]>();

            Task udpReceiverTask = Task.Run(() => ReceiveUdpDataAsync(_udpClient, _udpChannel, _cts.Token));
            Task udpWriterTask = Task.Run(() => ProcessUdpDataAsync(_udpOutputFilePath, _udpChannel, _cts.Token));

            _backgroundTask = Task.WhenAll(udpReceiverTask, udpWriterTask);
        }

        /// <summary>
        /// Stops   UDP receiver by canceling its background tasks and resources
        /// </summary>
        /// <returns>A Task representing   state of async operation</returns>
        private async Task StopUdpReceiverAsync()
        {
            if (!_cts.IsCancellationRequested)
            {
                await _cts.CancelAsync();

                try
                {
                    if (_backgroundTask != null)
                    {
                        await _backgroundTask;
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("UDP receiver cancellation requested");
                }
                finally
                {
                    DisposeUdpResources();
                }
            }
        }

        /// <summary>
        /// Receives UDP data and writes each received payload into tread safe channel
        /// </summary>
        /// <param name="udpClient">  UDP client to receive data from</param>
        /// <param name="channel">  channel used to handle received data</param>
        /// <param name="cancellationToken">Token to signal cancellation</param>
        /// <returns>A Task representing   async operation</returns>
        private async Task ReceiveUdpDataAsync(UdpClient udpClient, Channel<byte[]> channel, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync(cancellationToken);
                    await channel.Writer.WriteAsync(result.Buffer, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancellation of UDP receiver requested");

                DisposeUdpResources();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving UDP data: " + ex.Message);
            }
        }

        /// <summary>
        /// Processes data from UDP channel and writes it to specified output file
        /// </summary>
        /// <param name="outputFilePath"> file path where UDP data should be written</param>
        /// <param name="channel"> channel from which to read UDP data</param>
        /// <param name="cancellationToken">Token to signal cancellation</param>
        /// <returns>A Task representing async operation</returns>
        private async Task ProcessUdpDataAsync(string outputFilePath, Channel<byte[]> channel, CancellationToken cancellationToken)
        {
            try
            {
                await using FileStream fileStream = new FileStream(
                    outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                await foreach (var buffer in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    await fileStream.WriteAsync(buffer, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                DisposeUdpResources();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing UDP data to file: " + ex.Message);
            }
        }

        /// <summary>
        /// Disposes UDP channel and client
        /// </summary>
        private void DisposeUdpResources()
        {
            if (_udpChannel != null && _udpChannel.Writer.TryComplete())
            {
                _udpChannel = null;

                if (_udpClient != null && _udpClient.Client.Connected)
                {
                    _udpClient.Client.Close();
                    _udpClient.Dispose();
                    _udpClient = null;
                }
            }
        }

        /// <summary>
        /// Ensures that TCP connection is active
        /// Throws an InvalidOperationException if not connected
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Client is not connected</exception>
        private void EnsureConnected()
        {
            if (!_tcpConnectionController.IsConnected)
                throw new InvalidOperationException("Client is not connected");
        }

        /// <summary>
        /// Processes a response received from receiver
        /// It extracts header and payload and acts based on MessageType
        /// For NAK messages, it throws a NAKException with error code
        /// </summary>
        /// <param name="response">byte array containing response</param>
        /// <returns>A Message object representing response</returns>
        /// <exception cref="NetSdrApplication.Exceptions.NAKException">Received NAK with error code</exception>
        /// <exception cref="System.InvalidOperationException">Unexpected response type received</exception>
        private static Message ProcessResponse(byte[] response)
        {
            byte[] headerData = new byte[2];

            Array.Copy(response, 0, headerData, 0, 2);

            Header header = Header.FromBytes(headerData);

            int payloadLength = header.Length - 2;
            byte[] payload = payloadLength > 0 ? new byte[payloadLength] : Array.Empty<byte>();

            if (payloadLength > 0)
                Array.Copy(response, 2, payload, 0, payloadLength);

            switch (header.MessageType)
            {
                case MessageType.Ack:
                    Console.WriteLine("Received ACK");
                    return new AckMessage(header);
                case MessageType.ControlItem:
                    Console.WriteLine("Received valid control item response");
                    return new ControlItemMessage(Code.Default, payload);
                case MessageType.Nak:
                    // In NAK message trying extract error code (first byte of payload)
                    byte errorCode = (payloadLength > 0) ? response[2] : (byte)0;
                    throw new NAKException($"Received NAK with error code: {errorCode}");
                default:
                    throw new InvalidOperationException("Unexpected response type received.");
            }
        }
    }
}
