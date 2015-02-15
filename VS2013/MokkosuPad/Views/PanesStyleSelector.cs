using MokkosuPad.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MokkosuPad.Views
{
    class PanesStyleSelector : StyleSelector
    {
        public Style SourceViewStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SourceViewModel)
            {
                return SourceViewStyle;
            }
            else
            {
                return base.SelectStyle(item, container);
            }
        }
    }
}
