using System.Collections.Generic;

namespace Mokkosu.ClosureConversion
{
    class ClosureConversionContext
    {
        public string ArgName { get; private set; }
        public string[] Capture { get; private set; }

        public ClosureConversionContext(string arg_name, string[] capture)
        {
            ArgName = arg_name;
            Capture = capture;
        }

        public int GetCaptureIndex(string name)
        {
            for (var i = 0; i < Capture.Length; i++)
            {
                if (Capture[i] == name)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
