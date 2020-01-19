using ProgrammingParadigms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DomainAbstractions
{
    /// <summary>
    /// Just a button that looks like a row of a grid. But still can be wired to any kind of WPF element.
    /// It has two inputs:
    /// 1. IUI for the WPF element
    /// 2. IDataFlow<string> for the content it displays.
    /// </summary>
    public class RowButton : IUI, IDataFlow<string>
    {
        // properties
        public Thickness Margin { set => backgroundBorder.Margin = value;  }
        public double Height { set => textBackground.Height = value;  }

        // outputs
        private IEvent eventButtonClicked;

        // private fields
        private Border backgroundBorder = new Border();
        private Border textBackground = new Border();
        private TextBlock contentTextBlock = new TextBlock();

        /// <summary>
        /// A specific IUI button which displays itself in a grid cell and occupies the full space of the cell.
        /// </summary>
        /// <param name="title">the content of the button</param>
        public RowButton(string title = null)
        {
            contentTextBlock.Text = title;
            contentTextBlock.FontSize = 15;
            contentTextBlock.Foreground = Brushes.White;
            contentTextBlock.VerticalAlignment = VerticalAlignment.Center;
            contentTextBlock.IsEnabled = string.IsNullOrEmpty(title) ? false : true;
            contentTextBlock.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {
                eventButtonClicked?.Execute();
            };

            textBackground.Margin = new Thickness(2);
            textBackground.Child = contentTextBlock;
            textBackground.VerticalAlignment = VerticalAlignment.Center;

            backgroundBorder.Background = Brushes.White;
            backgroundBorder.Child = textBackground;
        }

        // IUI implementation -----------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {            
            return backgroundBorder;
        }

        // IDataFlow<string> implementation -----------------------------------------------------------
        string IDataFlow<string>.Data {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    textBackground.Background = Brushes.White;
                    textBackground.Height = 50;
                    contentTextBlock.Foreground = Brushes.White;
                    contentTextBlock.Text = value;
                    contentTextBlock.IsEnabled = false;
                }
                else
                {
                    textBackground.Background = Brushes.DodgerBlue;
                    textBackground.Height = 30;
                    contentTextBlock.Foreground = Brushes.White;
                    contentTextBlock.Text = value;
                    contentTextBlock.IsEnabled = true;
                }
            }
        }
    }
}
