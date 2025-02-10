using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Models.Enums
{
    /// <summary>
    /// NetSdr Message Type
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// A control item message
        /// </summary>
        ControlItem = 0,

        /// <summary>
        /// A data item message
        /// </summary>
        DataItem = 1,

        /// <summary>
        /// An acknowledgment (ACK) message
        /// </summary>
        Ack = 2,

        /// <summary>
        /// A negative acknowledgment (NAK) message
        /// </summary>
        Nak = 3
    }
}
