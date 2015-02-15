using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

namespace MokkosuIDE.ViewModels
{
    public class DocumentsViewModel : PanesViewModel
    {
        public DocumentsViewModel(string title) : base(title)
        {
            IsDirty = false;
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
        private TextDocument _Document = new TextDocument();

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

        #region Highlighting変更通知プロパティ
        private IHighlightingDefinition _Highlighting;

        public IHighlightingDefinition Highlighting
        {
            get
            { return _Highlighting; }
            set
            { 
                if (_Highlighting == value)
                    return;
                _Highlighting = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool IsDirty { get; set; }

        public void DocumentChanged(object sender, System.EventArgs e)
        {
            IsDirty = true;
        }
    }
}
