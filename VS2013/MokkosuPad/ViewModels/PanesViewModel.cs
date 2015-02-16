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

namespace MokkosuPad.ViewModels
{
    public class PanesViewModel : ViewModel
    {
        public PanesViewModel(string title)
        {
            Title = title;
            ContentId = GetHashCode().ToString();
        }

        #region ContentId変更通知プロパティ
        private string _ContentId;

        public string ContentId
        {
            get
            { return _ContentId; }
            set
            { 
                if (_ContentId == value)
                    return;
                _ContentId = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Title変更通知プロパティ
        private string _Title;

        public string Title
        {
            get
            { return _Title + (DirtyFlag ? "*" : ""); }
            set
            { 
                if (_Title == value)
                    return;
                _Title = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region DirtyFlag変更通知プロパティ
        private bool _DirtyFlag = false;

        public bool DirtyFlag
        {
            get
            { return _DirtyFlag; }
            set
            { 
                if (_DirtyFlag == value)
                    return;
                _DirtyFlag = value;
                RaisePropertyChanged();
                RaisePropertyChanged("Title");
            }
        }
        #endregion


        public override string ToString()
        {
            return Title;
        }
    }
}
