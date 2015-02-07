using System;

namespace Mokkosu
{
    class Program
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

            var parse_result = Parser.Parse(parse_context);
            var expr = parse_result.Main;
            var type = Typeinf.Start(expr);
            var clos = ClosureConverter.Start(expr);

            System.Console.WriteLine(expr);
            System.Console.WriteLine(type);
            System.Console.WriteLine(clos);

            var assembly = CodeGenerator.Start("Test", clos);
            assembly.Save("Test.exe");

            Evaluator.Start(expr);
        }
    }
}
