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

        public static string RunProgram(string exe_name)
        {
            if (_mokkosu != null && exe_name != "")
            {
                var proc = new Process();

                try
                {
                    var info = new ProcessStartInfo();
                    info.FileName = exe_name;
                    info.CreateNoWindow = true;
                    info.RedirectStandardOutput = true;
                    info.RedirectStandardError = true;
                    info.RedirectStandardInput = false;
                    info.UseShellExecute = false;
                    info.CreateNoWindow = true;

                    proc.OutputDataReceived += OutputDataReceived;
                    proc.ErrorDataReceived += OutputDataReceived;
                    proc.StartInfo = info;
                    proc.Start();

                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    proc.WaitForExit();
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                finally
                {
                    proc.Close();
                }
            }
            return "";
        }

        static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputReceived(e.Data);
        }

        public static void SaveExe(string save_fname)
        {
            if (_mokkosu != null)
            {
                _mokkosu.SaveExe(Path.GetFileName(save_fname));
            }
        }

        public static string GetLogoString()
        {
            _mokkosu = new Mokkosu.Main.Mokkosu();
            return _mokkosu.GetVersionString();
        }

        static string GetProgramString(string path)
        {
            using (var strm = new StreamReader(path))
            {
                return strm.ReadToEnd();
           }
        }

        public static string GetSampleOrFileString()
        {
            var name = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
            if (name != null)
            {
                return GetProgramString(name);
            }

            var exe_path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return GetProgramString(Path.Combine(exe_path, "Startup.mok"));
        }
    }
}
