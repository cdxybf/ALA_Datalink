using ProgrammingParadigms;
using System.Windows;

namespace DomainAbstractions
{
    /// <summary>
    /// This abstration is a general button which can be customized by it's Width, Height, FontSize and Margin. 
    /// It is also interactive as it can be clicked and then generates an IEvent.
    /// It has one input: the IUI to get the WPF element.
    /// </summary>
    public class Button : IUI
    {
        // properties ------------------------------------------------------------
        public string InstanceName;
        // the properties can extend for any UI customizing requirements
        public double Width { set => button.Width = value; }
        public double Height { set => button.Height = value; }
        public double FontSize { set => button.FontSize = value; }
        public Thickness Margin { set => button.Margin = value; }

        // outputs ---------------------------------------------------------------
        private IEvent eventButtonClicked;

        // private fields --------------------------------------------------------
        private System.Windows.Controls.Button button;

        /// <summary>
        /// An interactive UI element which omits an IEvent output when clicked.
        /// It can be customized by setting the Title, Width, Height FontSize and Margin.
        /// </summary>
        /// <param name="title">The text displayed on the button</param>
        public Button(string title)
        {
            button = new System.Windows.Controls.Button();
            button.Content = title;
            button.FontSize = 14;
            button.Click += (object sender, RoutedEventArgs e) =>
            {
                eventButtonClicked.Execute();
            };
        }

        // IUI implmentation ------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            return button;
        }
    }
}
