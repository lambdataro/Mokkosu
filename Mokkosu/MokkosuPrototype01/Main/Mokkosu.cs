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

            //foreach (var top_expr in parse_result.TopExprs)
            //{
            //    Console.WriteLine(top_expr);
            //}

            Typeinf.Start(parse_result);
        }
    }
}
