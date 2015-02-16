using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MokkosuPad.ViewModels
{
    class OutputViewModel : PanesViewModel
    {
        public OutputViewModel(string title) : base(title)
        {
        }

        #region Text変更通知プロパティ

        public string Text
        {
            get
            { return Document.Text; }
            set
            {
                Document = new TextDocument(value);
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Document変更通知プロパティ
        private TextDocument _Document;

        public TextDocument Document
        {
            get
            { return _Document; }
            set
            {
                if (_Document == value)
                    return;
                _Document = value;
                RaisePropertyChanged();
                RaisePropertyChanged("Text");
            }
        }
        #endregion
    }
}
