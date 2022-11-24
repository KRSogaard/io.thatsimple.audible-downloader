using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudibleDownloader.Exceptions
{
    public class FatalException : Exception
    {
        public FatalException(string message) : base(message)
        {
        }
        public FatalException() : base()
        {
        }
    }

    public class RetryableException : Exception
    {
        public RetryableException(string message) : base(message)
        {
        }
        public RetryableException() : base()
        {
        }
    }
}
