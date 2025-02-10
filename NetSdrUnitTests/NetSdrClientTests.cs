using Moq;

using NetSdrApplication.Exceptions;
using NetSdrApplication.Models.ControlItem;
using NetSdrApplication.Models.Enums;
using NetSdrApplication.Services.ConnectionClientController;
using NetSdrApplication.Services.NetSdrClient;

using System.Net;

namespace NetSdrUnitTests
{
    [TestClass]
    [DoNotParallelize]
    public class NetSdrClientTests
    {
        private const string _validIpAddress = "127.0.0.1";

        [TestMethod]
        public async Task ConnectAsync_ValidIp_ShouldCallConnectAsyncOnController()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController
                .Setup(c => c.ConnectAsync(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.ConnectAsync(_validIpAddress);

            // Assert
            mockController.Verify(c => c.ConnectAsync(IPAddress.Parse(_validIpAddress), 50000), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ConnectAsync_InvalidIp_ShouldThrowArgumentException()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.ConnectAsync("invalid_ip");
        }

        [TestMethod]
        public async Task DisconnectAsync_ShouldCallDisconnectAsyncOnController()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController
                .Setup(c => c.DisconnectAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Simulate connected state.
            mockController.Setup(c => c.IsConnected).Returns(true);
            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.DisconnectAsync();

            // Assert
            mockController.Verify(c => c.DisconnectAsync(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ConnectionException))]
        public async Task DisconnectAsync_ShouldThrowConnectionException()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController
                .Setup(c => c.DisconnectAsync())
                .Throws(new ConnectionException(string.Empty))
                .Verifiable();

            mockController.Setup(c => c.IsConnected).Returns(false);
            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.DisconnectAsync();

            // Assert
            mockController.Verify(c => c.DisconnectAsync(), Times.Once);

        }

        [TestMethod]
        public async Task SetReceiverState_NotConnected_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(false);
            var client = new NetSdrClient(mockController.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => client.SetReceiverState(Code.StartIQ));
        }

        [TestMethod]
        public async Task SetReceiverState_StartIQ_ShouldCallSendAndReceiveAsync_ForAck()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var ackHeader = new Header(2, MessageType.Ack);
            var ackMessage = new AckMessage(ackHeader);
            var ackBytes = ackMessage.ToBytes();

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(ackBytes)
                          .Verifiable();

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.SetReceiverState(Code.StartIQ);

            // Assert
            mockController.Verify(c => c.SendAsync(It.IsAny<byte[]>()), Times.Once);
            mockController.Verify(c => c.ReceiveAsync(), Times.Once);
        }

        [TestMethod]
        public async Task SetReceiverState_StartIQ_ShouldCallSendAndReceiveAsync_ForControlItem()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var controlItemHeader = new Header(2, MessageType.ControlItem);
            byte[] controlItemParameters =
                [
                    0x80, 
                    (byte)Code.StartIQ,
                    0x80,
                    (byte)Code.Default
                ];
            var controlItemMessage = new ControlItemMessage(Code.StartIQ, controlItemParameters);
            var controlItemBytes = controlItemMessage.ToBytes();

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(controlItemBytes)
                          .Verifiable();

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.SetReceiverState(Code.StartIQ);

            // Assert
            mockController.Verify(c => c.SendAsync(It.IsAny<byte[]>()), Times.Once);
            mockController.Verify(c => c.ReceiveAsync(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(NAKException))]
        public async Task SetReceiverState_StartIQ_ShouldThrowNAKException()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);
            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var nakHeader = new Header(3, MessageType.Nak);
            byte[] headerBytes = nakHeader.ToBytes();
            byte errorCode = 0x05;
            byte[] nakResponse = new byte[3];
            Array.Copy(headerBytes, 0, nakResponse, 0, 2);
            nakResponse[2] = errorCode;

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(nakResponse)
                          .Verifiable();

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.SetReceiverState(Code.StartIQ);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetReceiverState_StartIQ_ShouldThrowException()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var dataItemHeader = new Header(2, MessageType.DataItem);
            var dataItemBytes = dataItemHeader.ToBytes();

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(dataItemBytes)
                          .Verifiable();

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.SetReceiverState(Code.StartIQ);

            // Assert
            mockController.Verify(c => c.SendAsync(It.IsAny<byte[]>()), Times.Once);
            mockController.Verify(c => c.ReceiveAsync(), Times.Once);
        }

        [TestMethod]
        public async Task SetReceiverState_StopIQ_ShouldCallSendAndReceiveAsync()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var ackHeader = new Header(2, MessageType.Ack);
            var ackMessage = new AckMessage(ackHeader);
            var ackBytes = ackMessage.ToBytes();

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(ackBytes)
                          .Verifiable();

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.SetReceiverState(Code.StopIQ);

            // Assert
            mockController.Verify(c => c.SendAsync(It.IsAny<byte[]>()), Times.Once);
            mockController.Verify(c => c.ReceiveAsync(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetReceiverState_StopIQ_ShouldThrowArgumentException()
        {
            // Arrange

            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.SetReceiverState(Code.Default);

            // Assert
            mockController.Verify(c => c.SendAsync(It.IsAny<byte[]>()), Times.Never);
            mockController.Verify(c => c.ReceiveAsync(), Times.Never);
        }

        [TestMethod]
        public async Task SetTargerFrequency_ShouldCallSendAndReceiveAsync()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var ackHeader = new Header(2, MessageType.Ack);
            var ackMessage = new AckMessage(ackHeader);
            var ackBytes = ackMessage.ToBytes();

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(ackBytes)
                          .Verifiable();

            var client = new NetSdrClient(mockController.Object);
            ulong frequency = 14200000;

            // Act
            await client.SetTargetFrequency(frequency);

            // Assert
            mockController.Verify(c => c.SendAsync(It.IsAny<byte[]>()), Times.Once);
            mockController.Verify(c => c.ReceiveAsync(), Times.Once);
        }

        [TestMethod]
        public void Dispose_CalledMultipleTimes_ShouldNotThrow()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            var client = new NetSdrClient(mockController.Object);

            // Act
            client.Dispose();
            client.Dispose();
        }

        [TestMethod]
        public async Task UdpReceiver_StartAndStop_ShouldWorkCorrectly()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask);

            var ackHeader = new Header(2, MessageType.Ack);
            var ackMessage = new AckMessage(ackHeader);
            var ackBytes = ackMessage.ToBytes();

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(ackBytes);

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.ConnectAsync(_validIpAddress);

            await client.SetReceiverState(Code.StartIQ);
            await Task.Delay(100);
            await client.SetReceiverState(Code.StopIQ);

            mockController.Verify(c => c.ConnectAsync(It.IsAny<IPAddress>(), It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public async Task MultipleCommands_InSequence_ShouldCallConnectionControllerMultipleTimes()
        {
            // Arrange
            var mockController = new Mock<IConnectionController>();
            mockController.Setup(c => c.IsConnected).Returns(true);

            mockController.Setup(c => c.SendAsync(It.IsAny<byte[]>()))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var ackHeader = new Header(2, MessageType.Ack);
            var ackMessage = new AckMessage(ackHeader);
            var ackBytes = ackMessage.ToBytes();

            mockController.Setup(c => c.ReceiveAsync())
                          .ReturnsAsync(ackBytes)
                          .Verifiable();

            var client = new NetSdrClient(mockController.Object);

            // Act
            await client.SetReceiverState(Code.StartIQ);
            await client.SetTargetFrequency(14200000);
            await client.SetReceiverState(Code.StopIQ);
            await client.DisconnectAsync();

            // Assert
            mockController.Verify(c => c.SendAsync(It.IsAny<byte[]>()), Times.Exactly(3));
            mockController.Verify(c => c.ReceiveAsync(), Times.Exactly(3));
        }
    }
}