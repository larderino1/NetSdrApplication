using NetSdrApplication.Models.Enums;

namespace NetSdrApplication.Models.ControlItem
{
    /// <summary>
    /// NetSdr Base Message entity
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Message Header <see cref="Header"/>
        /// </summary>
        public Header Header { get; protected set; }

        /// <summary>
        /// Message payload
        /// </summary>
        public abstract byte[] GetPayload();

        /// <summary>
        /// Serializes message (header and payload) to a byte array
        /// </summary>
        /// <returns>Message data converted to byte array</returns>
        public byte[] ToBytes()
        {
            byte[] headerBytes = Header.ToBytes();
            byte[] payload = GetPayload();
            byte[] result = new byte[headerBytes.Length + payload.Length];

            Buffer.BlockCopy(headerBytes, 0, result, 0, headerBytes.Length);
            Buffer.BlockCopy(payload, 0, result, headerBytes.Length, payload.Length);

            return result;
        }
    }

    /// <summary>
    /// Control Item Message
    /// Format:
    ///   16-bit header,
    ///   16-bit control item code (little‑endian)
    ///   Followed by zero or more parameter bytes
    /// </summary>
    public class ControlItemMessage : Message
    {
        /// <summary>
        /// Control Item Code <see cref="Code"/>
        /// </summary>
        public Code ControlItemCode { get; set; }

        /// <summary>
        /// Parameters byte array
        /// </summary>
        public byte[] Parameters { get; set; }

        /// <summary>
        /// Constructor of <see cref="ControlItemMessage"/> class
        /// </summary>
        /// <param name="controlItemCode">The control item code</param>
        /// <param name="parameters">The parameter bytes</param>
        public ControlItemMessage(Code controlItemCode, byte[] parameters)
        {
            ControlItemCode = controlItemCode;
            Parameters = parameters ?? [];

            // header (2 bytes) + control item code (2 bytes) + parameter bytes.
            int totalLength = 2 + 2 + Parameters.Length;
            Header = new Header(totalLength, MessageType.ControlItem);
        }

        /// <inheritdoc/>
        public override byte[] GetPayload()
        {
            byte[] controlItemBytes = BitConverter.GetBytes((ushort)ControlItemCode);
            byte[] payload = new byte[controlItemBytes.Length + Parameters.Length];

            Buffer.BlockCopy(controlItemBytes, 0, payload, 0, controlItemBytes.Length);
            Buffer.BlockCopy(Parameters, 0, payload, controlItemBytes.Length, Parameters.Length);

            return payload;
        }
    }

    /// <summary>
    /// Acknowledgment Message Entity
    /// </summary>
    public class AckMessage : Message
    {
        /// <summary>
        /// Constructor of the <see cref="AckMessage"/>
        /// </summary>
        /// <param name="header">Message Header</param>
        public AckMessage(Header header)
        {
            Header = header;
        }

        /// <summary>
        /// Empty payload for ACK
        /// </summary>
        public override byte[] GetPayload() => [];
    }
}
