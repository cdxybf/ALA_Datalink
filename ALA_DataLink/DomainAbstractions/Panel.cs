using ProgrammingParadigms;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DomainAbstractions
{
    /// <summary>
    /// This UI abstraction is based on DockPanel, which layout it's sub-element by SetDock from any 
    /// directions e.g. Top, Left, Right, Bottom. The last element filled is default to occupied the 
    /// rest space of the DockPanel. 
    /// </summary>
    public class Panel : IUI
    {
        // properties
        public string InstanceName;
        public double Height { set => backgroundPanel.Height = value; }

        // outputs
        private List<IUI> children = new List<IUI>();

        // private fields
        private DockPanel backgroundPanel = new DockPanel();

        /// <summary>
        /// A rectangular IUI container with a title (can be empty) and other UI elements.
        /// </summary>
        /// <param name="title"></param>
        public Panel(string title = null)
        {
            if (!string.IsNullOrEmpty(title))
            {
                TextBlock titleTextBlock = new TextBlock();
                titleTextBlock.Text = title;
                titleTextBlock.Margin = new Thickness(5);
                titleTextBlock.FontSize = 15;

                DockPanel.SetDock(titleTextBlock, Dock.Top);
                backgroundPanel.Children.Add(titleTextBlock);
            }

            backgroundPanel.Background = new SolidColorBrush(Color.FromRgb(235, 235, 235));
        }

        // IUI implementation -------------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            foreach (var c in children)
            {
                var e = c.GetWPFElement();
                DockPanel.SetDock(e, Dock.Top);
                backgroundPanel.Children.Add(e);
            }
            return backgroundPanel;
        }        
    }
}
