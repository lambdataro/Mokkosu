using System.Linq;
using Xceed.Wpf.AvalonDock.Layout;

namespace MokkosuPad.Views
{
    class LayoutInitializer : ILayoutUpdateStrategy
    {
        private static int n = 0;

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            n++;
            if (n == 2)
            {
                var panel = layout.Children.ToArray()[0] as LayoutPanel;
                panel.InsertChildAt(1, new LayoutDocumentPane(anchorableToShow));
                return true;
            }
            return false;
        }
    }
}
