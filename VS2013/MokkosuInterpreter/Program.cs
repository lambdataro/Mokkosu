using System.Linq;
using System.IO;
using System;

namespace MokkosuInterpreter
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var mokkosu = new Mokkosu.Main.Mokkosu();

                foreach (var fname in args)
                {
                    mokkosu.AddSourceFile(fname);
                }

                if (args.Length != 0)
                {
                    var name = Path.GetFileNameWithoutExtension(args.Last());
                    mokkosu.Compile(name, true);
                    mokkosu.Run();
                }
                return 0;
            }
            catch (Mokkosu.Utils.MError e)
            {
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
