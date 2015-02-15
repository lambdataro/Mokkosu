using System.IO;
using System.Linq;

namespace MokkosuTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var mokkosu = new Mokkosu.Main.Mokkosu();

            foreach (var fname in args)
            {
                mokkosu.AddSourceFile(fname);
            }

            if (args.Length != 0)
            {
                var name = Path.GetFileNameWithoutExtension(args.Last());
                mokkosu.Compile(name);
                mokkosu.Run();
            }
        }
    }
}
