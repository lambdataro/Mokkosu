using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using MokkosuPad.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace MokkosuPad.ViewModels
{
    class SourceViewModel : PanesViewModel
    {
        public SourceViewModel(string title) : base(title)
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

        public void DocumentChanged(object sender, System.EventArgs e)
        {
            DirtyFlag = true;
        }

        static bool loaded_flg = false;

        public void Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded_flg)
            {
                Text = Model.GetSampleOrFileString();
                DirtyFlag = false;
                loaded_flg = true;
            }
            
        }
    }
}
