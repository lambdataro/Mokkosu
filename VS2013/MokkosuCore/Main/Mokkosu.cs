using Mokkosu.ClosureConversion;
using Mokkosu.CodeGenerate;
using Mokkosu.Input;
using Mokkosu.Lexing;
using Mokkosu.Parsing;
using Mokkosu.TypeInference;
using Mokkosu.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Mokkosu.Main
{
    public class Mokkosu
    {
        InputStream _input_stream;
        AssemblyBuilder _assembly;

        public Mokkosu()
        {
            _input_stream = new InputStream();

            var exe_path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            AddSourceFile(Path.Combine(exe_path, "Stdlib.mok"));
        }

        public string GetVersionString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@" =============================================");
            sb.AppendLine(@"  __  __         _     _                      ");
            sb.AppendLine(@" |  \/  |  ___  | | __| | __ ___   ___  _   _ ");
            sb.AppendLine(@" | |\/| | / _ \ | |/ /| |/ // _ \ / __|| | | |");
            sb.AppendLine(@" | |  | || (_) ||   < |   <| (_) |\__ \| |_| |");
            sb.AppendLine(@" |_|  |_| \___/ |_|\_\|_|\_\\___/ |___/ \__,_|");
            sb.AppendLine(@"");
            sb.AppendLine(@" =============== Version 1.1.3 ===============");
            return sb.ToString();
        }

        public void OutputVersion()
        {
            Console.WriteLine(GetVersionString());
        }

        public void AddSourceFile(string fname)
        {
            _input_stream.AddSourceFile(fname);
        }

        public void Compile(string path, bool is_dynamic)
        {
            Global.ClearOutput();

            var lexer = new Lexer(_input_stream);
            var parse_context = new ParseContext(lexer);

            var parse_result = Parser.Start(parse_context);

            Typeinf.Start(parse_result);

            if (Global.IsDefineKey("SHOW_PARSE_RESULT"))
            {
                Global.OutputString(parse_result.ToString());
            }

            var expr = TopToExpr.Start(parse_result);
            var closure_result = ClosureConverter.Start(expr);

            if (Global.IsDefineKey("SHOW_CLOSURE_RESULT"))
            {
                Global.OutputString(closure_result.ToString());
            }

            var path2 = Path.GetFullPath(path);
            var name = Path.GetFileNameWithoutExtension(path2);
            var dir = Path.GetDirectoryName(path2);

            _assembly = CodeGenerator.Start(dir, name, closure_result, is_dynamic);
        }

        public void SaveExe(string savename)
        {
            if (_assembly != null)
            {
                _assembly.Save(savename);
            }
        }

        public string GetOutput()
        {
            return Global.GetOutput();
        }

        public void Close()
        {
            _input_stream.CloseFile();
        }
    }
}
