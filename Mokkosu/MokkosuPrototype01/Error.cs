﻿using System;

namespace Mokkosu
{
    class Error : Exception
    {
        public Error() : base() { }
        public Error(string message) : base(message) { }
        public Error(string message, Exception inner) : base(message, inner) { }
    }
}
