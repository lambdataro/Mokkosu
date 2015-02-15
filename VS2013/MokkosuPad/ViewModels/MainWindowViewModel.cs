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

using MokkosuPad.Models;
using System.Collections.ObjectModel;

namespace MokkosuPad.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private SourceViewModel _source_vm = new SourceViewModel("ソースファイル");

        public void Initialize()
        {
            Documents.Add(_source_vm);
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

    }
}
