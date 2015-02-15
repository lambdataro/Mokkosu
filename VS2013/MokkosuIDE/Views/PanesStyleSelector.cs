using System.Windows;
using System.Windows.Controls;

using MokkosuIDE.ViewModels;

namespace MokkosuIDE.Views
{
    class PanesStyleSelector : StyleSelector
    {
        public Style SourcesStyle { get; set; }
        public Style OutputsStyle { get; set; }
        //public Style ImagesStyle { get; set; }
        //public Style ProjectPaneStyle { get; set; }
        //public Style SettingPaneStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SourcesViewModel)
            {
                return SourcesStyle;
            }
            else if (item is OutputsViewModel)
            {
                return OutputsStyle;
            }
            //else if (item is ImagesViewModel)
            //{
            //    return ImagesStyle;
            //}
            //else if (item is ProjectPaneViewModel)
            //{
            //    return ProjectPaneStyle;
            //}
            //else if (item is SettingPaneViewModel)
            //{
            //    return SettingPaneStyle;
            //}
            else
            {
                return base.SelectStyle(item, container);
            }
        }
    }
}
