using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrApplication.Exceptions
{
    /// <summary>
    /// Custom exception to inform about connection issues
    /// </summary>
    public class ConnectionException : Exception
    {
        /// <summary>
        /// Constructor of <see cref="ConnectionException"/> with inner exception capture
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="ex">Inner exception</param>
        public ConnectionException(string message, Exception ex) : base(message, ex) { }

        /// <summary>
        /// Constructor of <see cref="ConnectionException"/>
        /// </summary>
        /// <param name="message">Exception message</param>
        public ConnectionException(string message) : base(message) { }
    }
}
