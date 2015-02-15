using Livet.Commands;
using System;

namespace MokkosuIDE.ViewModels
{
    public class ToolsViewModel : PanesViewModel
    {
        public ToolsViewModel(string title) : base(title)
        {
        }

        #region IsActive変更通知プロパティ
        private bool _IsActive;

        public bool IsActive
        {
            get
            { return _IsActive; }
            set
            { 
                if (_IsActive == value)
                    return;
                _IsActive = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsVisible変更通知プロパティ
        private bool _IsVisible = true;

        public bool IsVisible
        {
            get
            { return _IsVisible; }
            set
            { 
                if (_IsVisible == value)
                    return;
                _IsVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}
