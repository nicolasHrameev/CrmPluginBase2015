using System;

namespace CrmPluginBase.Exceptions
{
    /// <summary>
    /// Internal crm exception without any information for user
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "All ok - justified")]
    public class CrmException : Exception
    {
        private readonly Exception sourceException;

        /// <summary>
        /// Internal crm exception .ctor
        /// </summary>
        /// <param name="message">Message for debug.</param>
        /// <param name="errorcode">Error code</param>
        /// <param name="expected">Exception is expected by the business logic - used for diagnostic purposes</param>
        public CrmException(string message, long errorcode = -1, bool expected = false)
            : base(message)
        {
            ErrorCode = errorcode;
            Expected = expected;
        }

        public CrmException(string message, Exception sourceException)
            : base(message, sourceException)
        {
            this.sourceException = sourceException;
        }

        public CrmException(string errorFormat, params object[] args)
            : base(string.Format(errorFormat, args))
        {
        }

        public CrmException(Exception sourceException, string errorFormat, params object[] args)
            : this(errorFormat, args)
        {
            this.sourceException = sourceException;
        }

        public long ErrorCode { get; }

        public bool Expected { get; }

        public override Exception GetBaseException() => sourceException;
    }
}
