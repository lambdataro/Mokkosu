using Mokkosu.ClosureConversion;
using Mokkosu.CodeGenerate;
using Mokkosu.Input;
using Mokkosu.Lexing;
using Mokkosu.Parsing;
using Mokkosu.TypeInference;
using System;

namespace Mokkosu.Main
{
    static class Mokkosu
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Mokkosu");
            Test(args);
        }

        static void Test(string[] args)
        {
            var input_stream = new InputStream();
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

            Console.WriteLine(closure_result);

            var assembly = CodeGenerator.Start("Test", closure_result);
            assembly.Save("Test.exe");
        }
    }
}
