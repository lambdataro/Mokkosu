using System.IO;

namespace Mokkosu.Input
{
    class SourceFile
    {
        string _name;
        int _line;
        TextReader _tr;

        public SourceFile(TextReader tr, string name)
        {
            _name = name;
            _line = 1;
            _tr = tr;
        }

        public int Read()
        {
            var ch = _tr.Read();
            if (ch == '\n')
            {
                _line++;
            }
            return ch;
        }

        public void Close()
        {
            _tr.Close();
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", _name, _line);
        }
    }
}
