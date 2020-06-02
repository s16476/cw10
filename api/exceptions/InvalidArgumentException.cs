using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.exceptions
{
    [Serializable()]
    public class InvalidArgumentException : Exception
    {

        public InvalidArgumentException()
        {

        }

        public InvalidArgumentException(string text) : base("Invalid argument: " + text)
        {

        }
    }
}
