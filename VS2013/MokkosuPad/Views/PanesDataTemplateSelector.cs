using MokkosuPad.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MokkosuPad.Views
{
    class PanesDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SourceViewTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SourceViewModel)
            {
                return SourceViewTemplate;
            }
            else
            {
                return base.SelectTemplate(item, container);
            }
        }
    }
}
