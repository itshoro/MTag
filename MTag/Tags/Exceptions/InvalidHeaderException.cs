using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMetaData.Tags.Exceptions
{
    public class InvalidHeaderException : Exception
    {
        public InvalidHeaderException()
        {
        }
        public InvalidHeaderException(string message) : base(message)
        {
        }
        public InvalidHeaderException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
