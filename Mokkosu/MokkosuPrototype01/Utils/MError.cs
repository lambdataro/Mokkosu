using System;

namespace Mokkosu.Utils
{
    class MError : Exception
    {
        public MError() : base() { }
        public MError(string message) : base(message) { }
        public MError(string message, Exception inner) : base(message, inner) { }
    }
}
