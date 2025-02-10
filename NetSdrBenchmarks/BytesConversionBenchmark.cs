using BenchmarkDotNet.Attributes;

using NetSdrApplication.Models.ControlItem;
using NetSdrApplication.Models.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrBenchmarks
{
    public class BytesConversionBenchmark
    {
        private ControlItemMessage _controlItemMessage;
        private AckMessage _ackMessage;

        [GlobalSetup]
        public void Setup()
        {
            byte[] parameters = { 0x80, (byte)Code.StartIQ, 0x80, 0x00 };
            _controlItemMessage = new ControlItemMessage(Code.StartIQ, parameters);

            var header = new Header(2, MessageType.Ack);
            _ackMessage = new AckMessage(header);
        }

        [Benchmark]
        public byte[] BenchmarkControlItemMessageSerialization()
        {
            return _controlItemMessage.ToBytes();
        }

        [Benchmark]
        public byte[] BenchmarkAckMessageSerialization()
        {
            return _ackMessage.ToBytes();
        }

        [Benchmark]
        public byte[] BenchmarkHeaderSerialization()
        {
            var header = new Header(4, MessageType.ControlItem);
            return header.ToBytes();
        }
    }
}
