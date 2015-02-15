using Livet;

namespace MokkosuIDE.ViewModels
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
            { return _Title; }
            set
            { 
                if (_Title == value)
                    return;
                _Title = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public override string ToString()
        {
            return Title;
        }
    }
}
