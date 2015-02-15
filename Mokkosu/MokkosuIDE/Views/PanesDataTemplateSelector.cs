using System.Windows;
using System.Windows.Controls;

using MokkosuIDE.ViewModels;

namespace MokkosuIDE.Views
{
    class PanesDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SourcesViewTemplate { get; set; }
        public DataTemplate OutputsViewTemplate { get; set; }
        //public DataTemplate ImagesViewTemplate { get; set; }
        //public DataTemplate ProjectPaneViewTemplate { get; set; }
        //public DataTemplate SettingPaneViewTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SourcesViewModel)
            {
                return SourcesViewTemplate;
            }
            else if (item is OutputsViewModel)
            {
                return OutputsViewTemplate;
            }
        //    else if (item is ImagesViewModel)
        //    {
        //        return ImagesViewTemplate;
        //    }
        //    else if (item is ProjectPaneViewModel)
        //    {
        //        return ProjectPaneViewTemplate;
        //    }
        //    else if (item is SettingPaneViewModel)
        //    {
        //        return SettingPaneViewTemplate;
        //    }
            else
            {
                return base.SelectTemplate(item, container);
            }
        }
    }
}
