using System;

namespace Mokkosu.Utils
{
    public class MError : Exception
    {
        public MError() : base() { }
        public MError(string message) : base(message) { }
        public MError(string message, Exception inner) : base(message, inner) { }
    }
}
