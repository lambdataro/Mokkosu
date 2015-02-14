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

namespace Mokkosu.Main
{
    static class Mokkosu
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" =============================================");
            Console.WriteLine("  __  __         _     _                      ");
            Console.WriteLine(" |  \\/  |  ___  | | __| | __ ___   ___  _   _ ");
            Console.WriteLine(" | |\\/| | / _ \\ | |/ /| |/ // _ \\ / __|| | | |");
            Console.WriteLine(" | |  | || (_) ||   < |   <| (_) |\\__ \\| |_| |");
            Console.WriteLine(" |_|  |_| \\___/ |_|\\_\\|_|\\_\\\\___/ |___/ \\__,_|");
            Console.WriteLine("");
            Console.WriteLine(" =============== Version 0.0.1 ===============");
            Console.WriteLine("");

            try
            {
                if (args.Length > 0)
                {
                    Start(args);
                    Console.WriteLine();
                    Console.WriteLine("コンパイルに成功しました。");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("入力がありません。");
                }
            }
            catch(MError e)
            {
                Console.Error.WriteLine(e.Message);
            }
            catch(NotImplementedException)
            {
                Console.Error.WriteLine("実装の誤りです。作者に連絡してください。");
            }
        }

        static void Start(string[] args)
        {
            var input_stream = new InputStream();

            var exe_path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            input_stream.AddSourceFile(Path.Combine(exe_path, "Stdlib.m"));

            foreach (var file in args)
            {
                input_stream.AddSourceFile(file);
            }

            var lexer = new Lexer(input_stream);
            var parse_context = new ParseContext(lexer);

            var parse_result = Parser.Start(parse_context);

            Typeinf.Start(parse_result);
            var expr = TopToExpr.Start(parse_result);
            var closure_result = ClosureConverter.Start(expr);

            var name = Path.GetFileNameWithoutExtension(args.Last());

            var assembly = CodeGenerator.Start(name, closure_result);
            assembly.Save(name + ".exe");
        }
    }
}
