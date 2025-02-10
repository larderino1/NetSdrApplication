using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Exceptions
{
    /// <summary>
    /// Custom exception to inform about NAK message
    /// </summary>
    public class NAKException : Exception
    {
        /// <summary>
        /// Constructor of <see cref="NAKException"/>
        /// </summary>
        /// <param name="message">Exception message</param>
        public NAKException(string message) : base(message) { }
    }
}
