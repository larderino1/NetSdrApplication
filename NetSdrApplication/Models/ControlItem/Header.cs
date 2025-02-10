using NetSdrApplication.Models.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Models.ControlItem
{
    /// <summary>
    /// Represents the 16-bit header of a NetSDR message
    /// Header consists of:
    /// - 8 bits: Length LSB  
    /// - 3 bits: Message type  
    /// - 5 bits: Length MSB  
    /// Total of 13-bit length is the total number of bytes in the message (including the header)
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Gets the total message length (in bytes), including this header
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the message type
        /// </summary>
        public MessageType MessageType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Header" /> class
        /// </summary>
        /// <param name="length">The 13-bit length value</param>
        /// <param name="messageType">The message type (3 bits)</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Length must be between 0 and 8191 bytes</exception>
        public Header(int length, MessageType messageType)
        {
            if (length < 0 || length > 8191)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 0 and 8191 bytes.");

            Length = length;
            MessageType = messageType;
        }

        /// <summary>
        /// Encodes header into a 2‑byte array using little‑endian byte order
        /// </summary>
        /// <returns>A 2‑byte array representing header</returns>
        public byte[] ToBytes()
        {
            byte lengthLSB = (byte)(Length & 0xFF);
            byte lengthMSB = (byte)((Length >> 8) & 0x1F);

            byte typeBits = (byte)((byte)MessageType & 0x07);

            ushort headerValue = (ushort)((lengthMSB << 11) | (typeBits << 8) | lengthLSB);

            return BitConverter.GetBytes(headerValue);
        }

        /// <summary>
        /// Decodes a <see cref="Header" /> from a 2‑byte array
        /// </summary>
        /// <param name="data">A 2‑byte array containing the header data</param>
        /// <returns>A <see cref="Header" /> instance</returns>
        /// <exception cref="System.ArgumentException">Data must be at least 2 bytes</exception>
        public static Header FromBytes(byte[] data)
        {
            if (data == null || data.Length < 2)
                throw new ArgumentException("Data must be at least 2 bytes.", nameof(data));

            ushort headerValue = BitConverter.ToUInt16(data, 0);

            byte lengthLSB = (byte)(headerValue & 0xFF);
            byte typeBits = (byte)((headerValue >> 8) & 0x07);
            byte lengthMSB = (byte)((headerValue >> 11) & 0x1F);

            int length = (lengthMSB << 8) | lengthLSB;
            MessageType messageType = (MessageType)typeBits;

            return new Header(length, messageType);
        }
    }
}
