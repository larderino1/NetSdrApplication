using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Models.Enums
{
    /// <summary>
    /// NetSdr Command codes
    /// </summary>
    public enum Code : byte
    {
        /// <summary>
        /// Command to start I/Q
        /// </summary>
        StartIQ = 0x01,

        /// <summary>
        /// Command to stop I/Q
        /// </summary>
        StopIQ = 0x02,

        /// <summary>
        /// Command to change target frequency
        /// </summary>
        SetFrequency = 0x0020,

        /// <summary>
        /// Default value
        /// </summary>
        Default = 0x00
    }
}
