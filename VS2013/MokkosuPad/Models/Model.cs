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

        public delegate void OutputReceiveDelegate(string line);

        public static event OutputReceiveDelegate OutputReceived;

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
                    return _mokkosu.GetVersionString() + "\n" + _mokkosu.GetOutput() + "エラー:\n" + e.Message;
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

        public static void RunProgram(string src_fname)
        {
            var exe_name = SaveExe(src_fname);

            if (_mokkosu != null && exe_name != "")
            {
                var info = new ProcessStartInfo();
                info.FileName = exe_name;
                info.CreateNoWindow = true;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.RedirectStandardInput = false;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;

                var proc = new Process();
                proc.OutputDataReceived += OutputDataReceived;
                proc.ErrorDataReceived += OutputDataReceived;
                proc.StartInfo = info;
                proc.Start();

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();
                proc.Close();
            }
        }

        static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputReceived(e.Data);
        }

        public static string SaveExe(string src_fname)
        {
            var fname = Path.GetFileNameWithoutExtension(src_fname);

            if (_mokkosu != null)
            {
                var save_name = fname + ".exe";
                _mokkosu.SaveExe(save_name);
                return Path.ChangeExtension(src_fname, ".exe");
            }
            else
            {
                return "";
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
