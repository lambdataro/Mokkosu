using System;

using Livet;
using Livet.Commands;

using Microsoft.Win32;

using MokkosuPad.Models;
using System.Collections.ObjectModel;
using System.Windows;
using Xceed.Wpf.AvalonDock;
using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Xml;
using System.Windows.Threading;
using System.Threading;

namespace MokkosuPad.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private SourceViewModel _source_vm = new SourceViewModel("ソースファイル");
        private OutputViewModel _output_vm = new OutputViewModel("出力");
        private string _source_fname = "";
        private ConsoleRedirectWriter consoleRedirectWriter;
        private const string ProgramName = "MokkosuPad";

        public void Initialize()
        {
            _source_vm.Highlighting = LoadHighlight("MokkosuPad.Resources.Mokkosu.xshd");

            Documents.Add(_source_vm);
            Documents.Add(_output_vm);

            Model.OutputReceived += OutputReceived;
        }

        private static IHighlightingDefinition LoadHighlight(string name)
        {
            using (Stream s = typeof(MainWindowViewModel).Assembly.GetManifestResourceStream(name))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    return ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        #region ExitCommand
        private ViewModelCommand _ExitCommand;

        public ViewModelCommand ExitCommand
        {
            get
            {
                if (_ExitCommand == null)
                {
                    _ExitCommand = new ViewModelCommand(Exit);
                }
                return _ExitCommand;
            }
        }

        public void Exit()
        {
            Environment.Exit(0);
        }
        #endregion


        #region SaveCommand
        private ViewModelCommand _SaveCommand;

        public ViewModelCommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null)
                {
                    _SaveCommand = new ViewModelCommand(Save);
                }
                return _SaveCommand;
            }
        }

        public void Save()
        {
            SaveSourceFile();
        }
        #endregion


        #region SaveAsCommand
        private ViewModelCommand _SaveAsCommand;

        public ViewModelCommand SaveAsCommand
        {
            get
            {
                if (_SaveAsCommand == null)
                {
                    _SaveAsCommand = new ViewModelCommand(SaveAs);
                }
                return _SaveAsCommand;
            }
        }

        public void SaveAs()
        {
            _source_fname = "";
            WindowTitle = ProgramName;
            SaveSourceFile();
        }
        #endregion


        #region OpenCommand
        private ViewModelCommand _OpenCommand;

        public ViewModelCommand OpenCommand
        {
            get
            {
                if (_OpenCommand == null)
                {
                    _OpenCommand = new ViewModelCommand(Open);
                }
                return _OpenCommand;
            }
        }

        public void Open()
        {
            OpenSourceFile();
        }
        #endregion


        #region NewCommand
        private ViewModelCommand _NewCommand;

        public ViewModelCommand NewCommand
        {
            get
            {
                if (_NewCommand == null)
                {
                    _NewCommand = new ViewModelCommand(New);
                }
                return _NewCommand;
            }
        }

        public void New()
        {
            NewSourceFile();
        }
        #endregion



        #region Documents変更通知プロパティ
        private ObservableCollection<PanesViewModel> _Documents = new ObservableCollection<PanesViewModel>();

        public ObservableCollection<PanesViewModel> Documents
        {
            get
            { return _Documents; }
            set
            { 
                if (_Documents == value)
                    return;
                _Documents = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region WindowTitle変更通知プロパティ
        private string _WindowTitle = ProgramName;

        public string WindowTitle
        {
            get
            { return _WindowTitle; }
            set
            { 
                if (_WindowTitle == value)
                    return;
                _WindowTitle = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public void SaveSourceFile()
        {
            if (_source_fname == "")
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Mokkosuソースファイル(*.mok)|*.mok";

                var result = dialog.ShowDialog();
                if (result.HasValue && dialog.FileName != "")
                {
                    _source_fname = dialog.FileName;
                    WindowTitle = ProgramName + " - " + Path.GetFileName(_source_fname);
                }
                else
                {
                    return;
                }
            }

            Model.SaveFile(_source_fname, _source_vm.Text);
            _source_vm.DirtyFlag = false;
        }

        public void OpenSourceFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Mokkosuソースファイル(*.mok)|*.mok";

            var result = dialog.ShowDialog();
            if (result.HasValue && dialog.FileName != "")
            {
                if (_source_vm.DirtyFlag)
                {
                    var mr = MessageBox.Show("内容を置き換えますか？", "確認",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (mr != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                _source_fname = dialog.FileName;
                WindowTitle = ProgramName + " - " + Path.GetFileName(_source_fname);
            }
            else
            {
                return;
            }

            

            var str = Model.OpenFile(_source_fname);
            _source_vm.Text = str;
            _source_vm.DirtyFlag = false;
        }

        public void NewSourceFile()
        {
            if (_source_vm.DirtyFlag)
            {
                var mr = MessageBox.Show("内容を消去しますか？", "確認",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (mr != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            _source_vm.Text = "";
            _source_vm.DirtyFlag = false;
            _source_fname = "";
            WindowTitle = ProgramName;
            
        }

        public void DocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            PanesViewModel pvm = null;

            foreach (var vm in Documents)
            {
                if (vm.ContentId == e.Document.ContentId)
                {
                    pvm = vm;
                    break;
                }
            }

            if (pvm != null)
            {
                Documents.Remove(pvm);
            }
        }

        #region ActiveContent変更通知プロパティ
        private PanesViewModel _ActiveContent;

        public PanesViewModel ActiveContent
        {
            get
            { return _ActiveContent; }
            set
            {
                if (_ActiveContent == value)
                    return;
                _ActiveContent = value;
                RaisePropertyChanged();
            }
        }
        #endregion



        #region ShowSourceCommand
        private ViewModelCommand _ShowSourceCommand;

        public ViewModelCommand ShowSourceCommand
        {
            get
            {
                if (_ShowSourceCommand == null)
                {
                    _ShowSourceCommand = new ViewModelCommand(ShowSource);
                }
                return _ShowSourceCommand;
            }
        }

        public void ShowSource()
        {
            ShowSourcePane();
        }
        #endregion


        public void ShowSourcePane()
        {
            if (!Documents.Contains(_source_vm))
            {
                Documents.Add(_source_vm);
            }
            ActiveContent = _source_vm;
        }



        #region ShowOutputCommand
        private ViewModelCommand _ShowOutputCommand;

        public ViewModelCommand ShowOutputCommand
        {
            get
            {
                if (_ShowOutputCommand == null)
                {
                    _ShowOutputCommand = new ViewModelCommand(ShowOutput);
                }
                return _ShowOutputCommand;
            }
        }

        public void ShowOutput()
        {
            ShowOutputPane();
        }
        #endregion


        public void ShowOutputPane()
        {
            if (!Documents.Contains(_output_vm))
            {
                Documents.Add(_output_vm);
            }
            ActiveContent = _output_vm;
        }


        #region RunProgramCommand
        private ViewModelCommand _RunProgramCommand;

        public ViewModelCommand RunProgramCommand
        {
            get
            {
                if (_RunProgramCommand == null)
                {
                    _RunProgramCommand = new ViewModelCommand(RunProgram);
                }
                return _RunProgramCommand;
            }
        }

        public void RunProgram()
        {
            SaveSourceFile();
            if (_source_fname != "")
            {
                var output_name = Path.ChangeExtension(_source_fname, ".exe");

                if (File.Exists(output_name))
                {
                    File.Delete(output_name);
                }

                var output = Model.CompileProgram(_source_fname, false);
                _output_vm.Text = output + "\n";

                Model.SaveExe(output_name);

                if (File.Exists(output_name))
                {
                    consoleRedirectWriter = new ConsoleRedirectWriter();
                    consoleRedirectWriter.OnWrite += new Action<string>(AppendOutput);
                    var ret = Model.RunProgram(output_name);
                    consoleRedirectWriter.Release();

                    if (ret != "")
                    {
                        MessageBox.Show(ret, "実行時エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion

        #region CompileProgramCommand
        private ViewModelCommand _CompileProgramCommand;

        public ViewModelCommand CompileProgramCommand
        {
            get
            {
                if (_CompileProgramCommand == null)
                {
                    _CompileProgramCommand = new ViewModelCommand(CompileProgram);
                }
                return _CompileProgramCommand;
            }
        }

        public void CompileProgram()
        {
            SaveSourceFile();
            if (_source_fname != "")
            {
                var output = Model.CompileProgram(_source_fname, false);
                _output_vm.Text = output;
                Model.SaveExe(_source_fname);
            }
        }
        #endregion

        public void AppendOutput(string line)
        {
            _output_vm.Text += line + "\n";
        }

        void OutputReceived(string line)
        {
            DispatcherHelper.UIDispatcher.InvokeAsync(() => AppendOutput(line));
        }
    }
}
