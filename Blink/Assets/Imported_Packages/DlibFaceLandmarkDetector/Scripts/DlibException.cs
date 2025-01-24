using System;

namespace DlibFaceLandmarkDetector
{

    /// <summary>
    /// The exception that is thrown by DlibFaceLandmarkDetector. 
    /// </summary>
    public class DlibException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DlibException"/> class.
        /// </summary>
        public DlibException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DlibException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public DlibException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DlibException"/> class with a formatted error message.
        /// </summary>
        /// <param name="messageFormat">
        /// The composite format string that contains text intermixed with format items.
        /// </param>
        /// <param name="args">
        /// An array of objects to format.
        /// </param>
        public DlibException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DlibException"/> class with a specified error message and a reference to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference if no inner exception is specified.
        /// </param>
        public DlibException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
