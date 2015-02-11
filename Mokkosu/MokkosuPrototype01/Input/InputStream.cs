using Mokkosu.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mokkosu.Input
{
    class InputStream
    {
        Queue<SourceFile> _source_queue = new Queue<SourceFile>();
        Stack<SourceFile> _source_stack = new Stack<SourceFile>();
        SourceFile _current_srcfile = null;
        int _ch = -1;
        static string _exe_path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public void AddSourceFile(string fname)
        {
            if (File.Exists(fname))
            {
                var tr = new StreamReader(fname);
                var srcfile = new SourceFile(tr, fname);
                _source_queue.Enqueue(srcfile);
            }
            else
            {
                throw new MError("ソースファイル " + fname + "が見つかりません。");
            }
        }

        public void IncludeSourceFile(string fname)
        {
            if (File.Exists(fname))
            {
                _source_stack.Push(_current_srcfile);
                var tr = new StreamReader(fname);
                _current_srcfile = new SourceFile(tr, fname);
                return;
            }

            var path = Path.Combine(_exe_path, fname);
            if (File.Exists(path))
            {
                _source_stack.Push(_current_srcfile);
                var tr = new StreamReader(path);
                _current_srcfile = new SourceFile(tr, fname);
                return;
            }

            throw new MError(Pos + ": ソースファイル " + fname + "が見つかりません。");
        }

        public void NextChar()
        {
            while (true)
            {
                if (_current_srcfile != null)
                {
                    _ch = _current_srcfile.Read();
                }
                if (_ch == -1)
                {
                    if (_source_stack.Count > 0)
                    {
                        if (_current_srcfile != null)
                        {
                            _current_srcfile.Close();
                        }
                        _current_srcfile = _source_stack.Pop();
                        continue;
                    }
                    else if (_source_queue.Count > 0)
                    {
                        if (_current_srcfile != null)
                        {
                            _current_srcfile.Close();
                        }
                        _current_srcfile = _source_queue.Dequeue();
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public char Char
        {
            get
            {
                if (_ch == -1)
                {
                    throw new MError(Pos + ": 構文エラー (予期しないファイル終端)");
                }
                else
                {
                    return (char)_ch;
                }
            }
        }

        public string Pos
        {
            get
            {
                return _current_srcfile.ToString();
            }
        }

        public bool IsEof()
        {
            return _ch == -1;
        }

        public bool IsWhiteSpace()
        {
            return !IsEof() && char.IsWhiteSpace(Char);
        }

        public bool IsDigit()
        {
            return !IsEof() && char.IsDigit(Char);
        }

        public bool IsHexDigit()
        {
            return !IsEof() &&
                (char.IsDigit(Char) ||
                (Char >= 'a' && Char <= 'f') ||
                (Char >= 'A' && Char <= 'F'));
        }

        public bool IsOctDigit()
        {
            return !IsEof() && Char >= '0' && Char <= '7';
        }

        public bool IsBinDigit()
        {
            return !IsEof() && (Char == '0' || Char == '1'); 
        }

        public bool IsIdStartChar()
        {
            return !IsEof() && (char.IsLetter(Char) || Char == '_');
        }

        public bool IsIdChar()
        {
            return !IsEof() && (char.IsLetterOrDigit(Char) || Char == '_');
        }

        public void SkipSpace()
        {
            while (IsWhiteSpace())
            {
                NextChar();
            }
        }
    }
}
