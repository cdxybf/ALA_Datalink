using ProgrammingParadigms;
using System;
using System.Windows;
using System.Windows.Media;

namespace DomainAbstractions
{
    /// <summary>
    /// Generally, it is a window-style progress bar. Resizable depend on the parent container who owns it.
    /// Has two inputs for displaying the progress, one for current progress and one for total amount.
    /// </summary>
    public class ProgressBar : IUI
    {
        // properties
        public string InstanceName;

        // outputs - actually it's a reversal output which used for input progress data
        private IDataFlowB<string> progressValue;
        private IDataFlowB<string> maximumValue;

        // private fields
        private System.Windows.Controls.ProgressBar progressBar;

        /// <summary>
        /// An IUI abstraction for displaying windows-style progress bar with a current and maximum input.
        /// </summary>
        public ProgressBar()
        {            
            progressBar = new System.Windows.Controls.ProgressBar();
            progressBar.Margin = new Thickness(5);
            progressBar.Height = 30;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Loaded += (object sender, RoutedEventArgs e) =>
            {
                progressBar.ApplyTemplate();
                System.Windows.Controls.Panel p = progressBar.Template.FindName("Animation", progressBar) as System.Windows.Controls.Panel;
                p.Background = new SolidColorBrush(Color.FromRgb(21, 193, 64));
            };
        }

        // binding the events
        private void PostWiringInitialize()
        {
            progressValue.DataChanged += () =>
            {
                progressBar.Value = Convert.ToInt32(progressValue.Data);
            };
            maximumValue.DataChanged += () =>
            {
                progressBar.Maximum = Convert.ToInt32(maximumValue.Data);
            };
        }

        // IUI implmentation -----------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            return progressBar;
        }
    }
}
