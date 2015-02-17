using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace MokkosuPad.Models
{
    public class Model : NotificationObject
    {
        static Mokkosu.Main.Mokkosu _mokkosu;

        public static void SaveFile(string fname, string contents)
        {
            using (var writer = new StreamWriter(fname))
            {
                writer.Write(contents);
            }
        }

        public static string OpenFile(string fname)
        {
            using (var reader = new StreamReader(fname))
            {
                return reader.ReadToEnd();
            }
        }

        public static string CompileProgram(string fname, bool is_dynamic)
        {
            try
            {
                _mokkosu = new Mokkosu.Main.Mokkosu();
                _mokkosu.AddSourceFile(fname);

                var name = Path.GetFileNameWithoutExtension(fname);
                _mokkosu.Compile(fname, is_dynamic);
                return _mokkosu.GetVersionString() + "\n" + _mokkosu.GetOutput();
            }
            catch (Mokkosu.Utils.MError e)
            {
                if (_mokkosu == null)
                {
                    return "エラー:\n" + e.Message;
                }
                else
                {
                    return _mokkosu.GetVersionString() + "\nエラー:\n" + e.Message;
                }
            }
            catch (Exception e)
            {
                return "致命的なエラー:\n" + e.ToString();
            }
            finally
            {
                if (_mokkosu != null)
                {
                    _mokkosu.Close();
                }
            }
        }

        public static string RunProgram()
        {
            if (_mokkosu != null)
            {
                try
                {
                    _mokkosu.Run();
                }
                catch (Exception e)
                {
                    return "実行時エラー\n" + e.ToString();
                }
            }
            return "";
        }

        public static void SaveExe(string src_fname)
        {
            var fname = Path.GetFileNameWithoutExtension(src_fname);

            if (_mokkosu != null)
            {
                _mokkosu.SaveExe(fname + ".exe");
            }
        }

        public static string GetLogoString()
        {
            _mokkosu = new Mokkosu.Main.Mokkosu();
            return _mokkosu.GetVersionString();
        }

        public static string GetSampleProgramString()
        {
            var exe_path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            using (var strm = new StreamReader(Path.Combine(exe_path, "Startup.mok")))
            {
                var str = strm.ReadToEnd();
                return str;
            }
        }
    }
}
