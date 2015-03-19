using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MokkosuCompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            var mokkosu = new Mokkosu.Main.Mokkosu();
            try
            {
                mokkosu.OutputVersion();
                Console.WriteLine();

                foreach (var fname in args)
                {
                    mokkosu.AddSourceFile(fname);
                }

                if (args.Length != 0)
                {
                    var name = Path.GetFileNameWithoutExtension(args.Last());
                    mokkosu.Compile(name, false);
                    Console.WriteLine(mokkosu.GetOutput());
                    mokkosu.SaveExe(name + ".exe");
                    Console.WriteLine("コンパイルに成功しました。\n");
                }
                return 0;
            }
            catch (Mokkosu.Utils.MError e)
            {
                Console.WriteLine(mokkosu.GetOutput());
                Console.WriteLine(e.Message);
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("致命的なエラー:");
                Console.WriteLine(e.ToString());
                return 2;
            }
        }
    }
}
