using ProgrammingParadigms;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DomainAbstractions
{
    /// <summary>
    /// Boolean data output that is true for the selected radio button and false for the rest, and an event. 
    /// Goes false when the wizard is cancelled or the whole operation completes.
    /// </summary>
    public class WizardItem : IUIWizard
    {
        // properties
        public string ContentText { set => checkRadioButton.Content = value; }
        public string ImageName { set => imageView.Source = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + @";component/Application/" + string.Format("Resources/{0}", value), UriKind.Absolute)); }
        public bool Checked { get => (bool)checkRadioButton.IsChecked; set => checkRadioButton.IsChecked = value; }

        // outputs
        private IEvent eventOutput;

        // private fields
        private Image imageView = new Image();
        private RadioButton checkRadioButton = new RadioButton();

        /// <summary>
        /// An IUI element that only exists in a Wizard.
        /// </summary>
        /// <param name="contentText">the text displayed on the item</param>
        public WizardItem(string contentText = null)
        {
            checkRadioButton.Content = contentText;
        }

        // IUI implementation ------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            StackPanel contentPanel = new StackPanel();
            contentPanel.Orientation = Orientation.Horizontal;
            contentPanel.Height = 48;

            imageView.Width = 38;
            imageView.Height = 38;
            imageView.VerticalAlignment = VerticalAlignment.Center;
            imageView.Margin = new Thickness(10, 0, 6, 0);
            contentPanel.Children.Add(imageView);

            checkRadioButton.VerticalAlignment = VerticalAlignment.Center;
            checkRadioButton.Margin = new Thickness(6, 0, 6, 0);
            checkRadioButton.GroupName = "groupName";
            checkRadioButton.FontSize = 17;
            contentPanel.Children.Add(checkRadioButton);

            return contentPanel;
        }


        // IUIWizard implementation -------------------------------------------------
        bool IUIWizard.Checked => Checked;

        void IUIWizard.GenerateOutputEvent()
        {
            eventOutput?.Execute();
        }
    }
}
