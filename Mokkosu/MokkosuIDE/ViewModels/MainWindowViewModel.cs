using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using MokkosuIDE.Models;
using Xceed.Wpf.AvalonDock;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MokkosuIDE.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private SourcesViewModel _input = new SourcesViewModel("ソースコード");
        private OutputsViewModel _output = new OutputsViewModel("出力");

        public void Initialize()
        {
            Documents.Add(_input);
            Documents.Add(_output);
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


        #region Tools変更通知プロパティ
        private ObservableCollection<ToolsViewModel> _Tools = new ObservableCollection<ToolsViewModel>();

        public ObservableCollection<ToolsViewModel> Tools
        {
            get
            { return _Tools; }
            set
            {
                if (_Tools == value)
                    return;
                _Tools = value;
                RaisePropertyChanged();
            }
        }
        #endregion


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

        public void ShowDocumentPane(DocumentsViewModel dvm)
        {
            if (!Documents.Contains(dvm))
            {
                Documents.Add(dvm);
            }
            ActiveContent = dvm;
        }

        #region ShowInputCommand
        private ViewModelCommand _ShowInputCommand;

        public ViewModelCommand ShowInputCommand
        {
            get
            {
                if (_ShowInputCommand == null)
                {
                    _ShowInputCommand = new ViewModelCommand(ShowInput);
                }
                return _ShowInputCommand;
            }
        }

        public void ShowInput()
        {
            ShowDocumentPane(_input);
        }
        #endregion

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
            ShowDocumentPane(_output);
        }
        #endregion

        #region RunCommand
        private ViewModelCommand _RunCommand;

        public ViewModelCommand RunCommand
        {
            get
            {
                if (_RunCommand == null)
                {
                    _RunCommand = new ViewModelCommand(Run);
                }
                return _RunCommand;
            }
        }

        private StringBuilder _str_builder;

        public void Run()
        {
            var info = new ProcessStartInfo();

            info.FileName = "MokkosuPrototype01.exe";
            info.Arguments = "Test.m";

            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = false;
            info.RedirectStandardError = true;

            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            var proc = new Process();
            proc.StartInfo = info;
            proc.OutputDataReceived += OutputDataReceived;
            proc.ErrorDataReceived += OutputDataReceived;

            _str_builder = new StringBuilder();

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            _output.Text = _str_builder.ToString();
        }
        #endregion

        void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _str_builder.Append(e.Data);
            _str_builder.Append("\n");
        }


    }
}
